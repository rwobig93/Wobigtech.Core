using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Wobigtech.Core.Comm;

namespace Wobigtech.Core.Manage
{
    public class CompanionRequest : AuditedAggregateRoot<Guid>
    {

        public NatDtoJoinReq JoinRequest { get; set; }
        public DateTime InitialRequest { get; set; }
        public DateTime LastRequest { get; set; }
        public List<string> PublicIPs { get; set; } = new List<string>();
        public int TotalRequests { get; set; }

        public CompanionRequest(Guid id) : base(id)
        {
        }

        public CompanionRequest(NatDtoJoinReq joinRequest, DateTime initialRequest, DateTime lastRequest, List<string> publicIPs, int totalRequests)
        {
            JoinRequest = joinRequest;
            InitialRequest = initialRequest;
            LastRequest = lastRequest;
            PublicIPs = publicIPs;
            TotalRequests = totalRequests;
        }
    }
}
