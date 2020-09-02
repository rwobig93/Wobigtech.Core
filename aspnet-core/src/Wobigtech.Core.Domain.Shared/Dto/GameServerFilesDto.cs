using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.Dto
{
    public class GameServerFilesDto
    {
        public string Game { get; set; }
        public string Server { get; set; }
        public List<string> Executables { get; set; } = new List<string>();
        public List<string> FilesScripts { get; set; } = new List<string>();
        public List<string> FilesLogs { get; set; } = new List<string>();
    }
}
