using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.Comm
{
    public static class NatSubjects
    {
        public static string Join => "WTJoin";

        public static string Telemetry => "WTTelemetry";

        public static string FileUpdates => "WTFileUpdates";

        public static string CompanionChannel(string companionID)
        {
            return $"{companionID}_CMD";
        }
    }
}
