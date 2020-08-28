using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Wobigtech.Core.Enums;

namespace Wobigtech.Core.GameServer
{
    public class CreateUpdateGameServerDto
    {
        [Required]
        public string ServerName { get; set; }
        public string Password { get; set; }
        [Required]
        public string IPAddress { get; set; }
        public string ExtHostname { get; set; }
        [Required]
        public int GamePort { get; set; }
        public int QueryPort { get; set; }
        public int RconPort { get; set; }
        [Required]
        public bool Modded { get; set; }
        public string ModList { get; set; }
        [Required]
        public string ServerPathExe { get; set; }
        public string ServerPathBatch { get; set; }
        public string ServerPathLog { get; set; }
        [Required]
        public string ServerProcessName { get; set; }
    }
}
