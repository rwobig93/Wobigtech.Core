using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Security.Principal;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.IdentityServer;
using Volo.Abp.IdentityServer.Clients;
using Wobigtech.Core.Enums;
using Wobigtech.Core.Tools;
using Wobigtech.Core.Users;

namespace Wobigtech.Core.GameServer
{
    public class Server : AuditedAggregateRoot<Guid>
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Socket { get { return $"{this.IPAddress}:{this.Port}"; } }
        public string Hostname { get; set; }
        public string FriendlyName { get; set; }
        public StatusAvailable StatusUpDown { get; set; } = StatusAvailable.Unknown;
        public string Secret { get; set; }
        public OS OS { get; set; } = OS.Unknown;
        public Guid OwnerID { get; set; } = (Guid)Tools.Profile.GetCurrentUser()?.Id;
        protected Server()
        {

        }
        public Server(Guid serverID, string iPAddress, int port, string hostname, string friendlyName, string secret) : base(serverID)
        {
            IPAddress = iPAddress;
            Port = port;
            Hostname = hostname;
            FriendlyName = friendlyName;
            Secret = secret;
        }
    }
}
