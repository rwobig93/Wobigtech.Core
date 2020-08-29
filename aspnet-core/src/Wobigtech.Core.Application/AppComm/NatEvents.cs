using NATS.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.AppComm
{
    public static class NatEvents
    {
        public static EventHandler<MsgHandlerEventArgs> NatMsgHandler = (s, a) =>
        {
            Log.Information($"NAT-MSG: {a.Message}");
        };
    }
}
