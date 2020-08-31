﻿using Microsoft.Extensions.Logging;
using NATS.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Wobigtech.Core.Comm;
using Wobigtech.Core.Crypto;
using static Wobigtech.Core.AppConsts;

namespace Wobigtech.Core.AppComm
{
    public static class NatUtil
    {

        public static void SetupNatConnection()
        {
            if (NatFactory == null)
            {
                Log.Debug("NatFactory was null, calling InitializeNatFactory()");
                InitializeNatFactory();
            }
            if (NatConn != null)
            {
                Log.Debug("NatConn isn't null, flushing and closing previous connection");
                NatConn.Flush();
                NatConn.Close();
                Log.Information("Flushed and closed previous natConn");
            }
            InitializeNatServersListener();
        }

        private static void InitializeNatServersListener()
        {
            Log.Debug("Starting InitializeNatServersListener()");
            if (NatConn == null)
            {
                Log.Debug("NatConn was null, initializing new NatConn");
                var opts = ConnectionFactory.GetDefaultOptions();
                opts.Url = "nats://localhost:9595";
                NatConn = NatFactory.CreateConnection(opts);
                //NatConn = NatFactory.CreateConnection(Certs.GetNATConnOptions("localhost", AppConsts.PathCerts));
                // Need to create connection options and generate self-signed cert if one isn't set
            }
            else
            {
                Log.Warning("NatConn is not null, we must have attempted to initialize the Nat Listener after it was already started!");
            }
            StartNatMsgHandler();
            SendTestNatMsg("Message", NatSubjects.Join);
            Log.Information("Nat Server Listeners started!");
        }

        private static void SendTestNatMsg(string message, string subject)
        {
            Log.Debug($"Sending test NAT message: [sub]{subject} [msg] {message}");
            NatConn.Publish(subject, NatComm.NatMsgSend(Enums.NatCommType.JoinReq, message));
            Log.Information("Sent test NAT message");
        }

        private static void StartNatMsgHandler()
        {
            Log.Debug("Starting Nat Message Handler");
            IAsyncSubscription aSubscription = NatConn.SubscribeAsync(NatSubjects.Join);
            aSubscription.MessageHandler += NatEvents.NatMsgHandlerGeneral;
            aSubscription.Start();
            Log.Information("Nat Message Handler Started");
        }

        private static void InitializeNatFactory()
        {
            Log.Debug("Starting InitializeNatFactory()");
            NatFactory = new NATS.Client.ConnectionFactory();
            Log.Debug("Initialized NatFactory");
        }
    }
}
