using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Wobigtech.Core.GameServer
{
    public class GameServerDto : AuditedEntityDto<Guid>
    {
        public GameServerDto()
        {
        }

        public Guid GameID { get; set; }
        public Guid ToolID { get; set; }
        public string ServerName { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public string ExtHostname { get; set; }
        public int GamePort { get; set; }
        public int QueryPort { get; set; }
        public int RconPort { get; set; }
        public bool Modded { get; set; }
        public string ModList { get; set; }
        public string ServerPathExe { get; set; }
        public string ServerPathBatch { get; set; }
        public string ServerPathLog { get; set; }
        public string ServerProcessName { get; set; }
    }
}
