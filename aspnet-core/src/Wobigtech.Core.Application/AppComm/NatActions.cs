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
            Log.Information($"Join Request: Received [ID]{joinReq.CompanionID} [V]{joinReq.VersionNumber} " +
                $"[HST]{joinReq.HostName} [S]{joinReq.CompanionSecret}");
            // TO-DO: AddJoinRequestToQueue(joinReq);
            ApproveJoinRequest(joinReq);
        }

        private static void ApproveJoinRequest(NatDtoJoinReq joinReq)
        {
            Log.Debug($"Approving Join Request: [ID]{joinReq.CompanionID} [V]{joinReq.VersionNumber} " +
                $"[HST]{joinReq.HostName} [S]{joinReq.CompanionSecret}");
            // TO-DO: Add entry to EF Table for Joined Companions
        }
    }
}
