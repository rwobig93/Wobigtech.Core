using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Wobigtech.Core.GameServer
{
    public class GameDto : AuditedEntityDto<Guid>
    {
        public string FriendlyName { get; set; }
        public Guid SteamGameID { get; set; }
        public Guid SteamToolID { get; set; }
        public string ServerPathExe { get; set; }
        public string ServerPathBatch { get; set; }
        public string ServerPathLog { get; set; }
        public string ServerProcessName { get; set; }
    }
}
