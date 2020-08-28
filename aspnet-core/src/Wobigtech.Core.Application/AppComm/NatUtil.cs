using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
            InitializeNatServerListener();
        }

        private static void InitializeNatServerListener()
        {
            Log.Debug("Starting InitializeNatServerListener()");
            if (NatConn == null)
            {
                Log.Debug("NatConn was null, initializing new NatConn");
                //NatConn = NatFactory.CreateConnection(Certs.GetNATConnOptions("localhost", AppConsts.PathCerts));
                // Need to create connection options and generate self-signed cert if one isn't set
            }
        }

        private static void InitializeNatFactory()
        {
            Log.Debug("Starting InitializeNatFactory()");
            NatFactory = new NATS.Client.ConnectionFactory();
            Log.Debug("Initialized NatFactory");
        }
    }
}
