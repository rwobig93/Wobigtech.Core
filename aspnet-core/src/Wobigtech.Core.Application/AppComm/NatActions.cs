using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using Wobigtech.Core.Comm;

namespace Wobigtech.Core.AppComm
{
    public static class NatActions
    {
        internal static void ProcessJoinRequest(NatDtoJoinReq joinReq)
        {
            // TO-DO: Add join request to queue so admin can verify and accept/deny companion join request
            Log.Debug($"JOIN REQUEST: Received [ID]{joinReq.CompanionID} [V]{joinReq.VersionNumber} [HST]{joinReq.HostName} [S]{joinReq.CompanionSecret}");
        }
    }
}
