using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Wobigtech.Core.Enums;
using Wobigtech.Core.Tools;

namespace Wobigtech.Core.GameServer
{
    public class GameServer : AuditedAggregateRoot<Guid>
    {
        public Guid GameID { get; set; }
        public Guid ToolID { get; set; }
        public Guid ServerID { get; set; }
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
        public DateTime StatusLastDown { get; set; }
        public StatusAvailable StatusUpDown { get; set; } = StatusAvailable.Unknown;
        public bool StatusJoinable { get; set; }
        public Guid OwnerID { get; set; } = (Guid)Profile.GetCurrentUser()?.Id;
        protected GameServer()
        {
            
        }
        public GameServer(Guid gameServerID, Guid gameID, Guid toolID, Guid serverID, string serverName, string password, string iPAddress, string extHostname, int gamePort, int queryPort, int rconPort, bool modded, string modList, string serverPathExe, string serverPathBatch, string serverPathLog, string serverProcessName, DateTime statusLastDown, StatusAvailable statusUpDown, bool statusJoinable, Guid ownerID) : base(gameServerID)
        {
            GameID = gameID;
            ToolID = toolID;
            ServerID = serverID;
            ServerName = serverName;
            Password = password;
            IPAddress = iPAddress;
            ExtHostname = extHostname;
            GamePort = gamePort;
            QueryPort = queryPort;
            RconPort = rconPort;
            Modded = modded;
            ModList = modList;
            ServerPathExe = serverPathExe;
            ServerPathBatch = serverPathBatch;
            ServerPathLog = serverPathLog;
            ServerProcessName = serverProcessName;
            StatusLastDown = statusLastDown;
            StatusUpDown = statusUpDown;
            StatusJoinable = statusJoinable;
            OwnerID = ownerID;
        }
    }
}
