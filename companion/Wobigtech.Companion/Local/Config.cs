using Serilog.Core;
using Serilog.Events;
using Wobigtech.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Companion.Shared
{
    public class Config
    {
        public string HomeServerSocket { get; set; }
        public string HomeServerSecret { get; set; }
        public string CompanionID { get; set; } = CBase.GenerateString(64);
        public string CompanionSecret { get; set; }
        public string CompanionSecretHash { get; set; }
        public string CompanionCertLocation { get; set; }
        public string PathGameServerLocation { get; set; }
        public LogEventLevel LoggingLevel { get; set; } = LogEventLevel.Information;
    }
}
