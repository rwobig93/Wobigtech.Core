using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wobigtech.Companion.Dto
{
    public class RunSteamDto
    {
        public string Command { get; set; }
        public DataReceivedEventHandler OutputHandler { get; set; }
        public EventHandler ExitHandler { get; set; }
    }
}
