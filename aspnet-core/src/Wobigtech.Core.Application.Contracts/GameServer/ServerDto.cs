using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Wobigtech.Core.GameServer
{
    public class ServerDto : AuditedEntityDto<Guid>
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Socket { get; set; }
        public string Hostname { get; set; }
        public string FriendlyName { get; set; }
        public Guid OwnerID { get; set; }
    }
}
