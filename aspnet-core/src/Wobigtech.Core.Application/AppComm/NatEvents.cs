using NATS.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using Wobigtech.Core.Comm;

namespace Wobigtech.Core.AppComm
{
    public static class NatEvents
    {
        public static EventHandler<MsgHandlerEventArgs> NatMsgHandlerGeneral = (s, a) =>
        {
            Log.Information($"NAT-MSG: {a.Message}");
        };

        public static EventHandler<MsgHandlerEventArgs> NatMsgHandlerJoinReq = (s, a) =>
        {
            try
            {
                var msgRec = NatComm.NatMsgReceive(a.Message.Data);
                Log.Information($"NAT-MSG: [type] {msgRec.NatType} [msg] {msgRec.NatDto}");
                if (msgRec.NatType == Enums.NatCommType.JoinReq)
                {
                    NatDtoJoinReq joinReq = (NatDtoJoinReq)msgRec.NatDto;
                    NatActions.ProcessJoinRequest(joinReq);
                }
                else
                {
                    Log.Debug("Nat Type wasn't a Join Request, skipping");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "NAT-JOINREQ ERROR");
            }
        };
    }
}
