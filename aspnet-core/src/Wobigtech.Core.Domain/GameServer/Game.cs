using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Wobigtech.Core.Enums;

namespace Wobigtech.Core.GameServer
{
    public class Game : AuditedAggregateRoot<Guid>
    {
        public string FriendlyName { get; set; }
        public int SteamID { get; set; }
        public SteamAppType SteamAppType { get; set; }
        public string SteamName { get; set; }
        public string SteamSupportedSystems { get; set; }
        public string SteamLastRecordUpdate { get; set; }
        public string SteamLastChangeNumber { get; set; }
        public string ServerPathExe { get; set; }
        public string ServerPathBatch { get; set; }
        public string ServerPathLog { get; set; }
        public string ServerProcessName { get; set; }

        protected Game()
        {

        }
        public Game(Guid id, string friendlyName, int steamID, SteamAppType steamAppType, string steamName, string steamSupportedSystems, string steamLastRecordUpdate, string steamLastChangeNumber) : base(id)
        {
            FriendlyName = friendlyName;
            SteamID = steamID;
            SteamAppType = steamAppType;
            SteamName = steamName;
            SteamSupportedSystems = steamSupportedSystems;
            SteamLastRecordUpdate = steamLastRecordUpdate;
            SteamLastChangeNumber = steamLastChangeNumber;
        }
    }
}
