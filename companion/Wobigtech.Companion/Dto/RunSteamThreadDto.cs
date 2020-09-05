using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Companion.Dto
{
    class RunSteamThreadDto
    {
        public Action<RunSteamDto> RunSteamMethod { get; set; }
        public RunSteamDto SteamDto { get; set; }
    }
}
