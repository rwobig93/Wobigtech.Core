using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wobigtech.Core.Manage
{
    public class CompanionManaged : AuditedAggregateRoot<Guid>
    {

        public string CompanionID { get; set; }
        public string Version { get; set; }
        public string Hostname { get; set; }
        public string CompanionSecretHash { get; set; }
        public string PublicIP { get; set; }
        public string PrivateIP { get; set; }
        public List<int> PortsAllowed { get; set; } = new List<int>();
        public List<int> PortsUsed { get; set; } = new List<int>();

        public CompanionManaged(Guid id) : base(id)
        {
        }

        public CompanionManaged(string companionID, string version, string hostname, string companionSecretHash, string publicIP, string privateIP, List<int> portsAllowed, List<int> portsUsed)
        {
            CompanionID = companionID;
            Version = version;
            Hostname = hostname;
            CompanionSecretHash = companionSecretHash;
            PublicIP = publicIP;
            PrivateIP = privateIP;
            PortsAllowed = portsAllowed;
            PortsUsed = portsUsed;
        }
    }
}
