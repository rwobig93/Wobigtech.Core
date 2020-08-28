using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.Dto
{
    public class GameServerFilesDto
    {
        public string Game { get; set; }
        public string Server { get; set; }
        public List<string> Executables { get; set; }
        public List<string> FilesScripts { get; set; }
        public List<string> FilesLogs { get; set; }
    }
}
