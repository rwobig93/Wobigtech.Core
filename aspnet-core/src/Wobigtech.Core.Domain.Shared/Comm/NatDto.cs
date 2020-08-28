using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using Wobigtech.Core.Dto;
using Wobigtech.Core.Enums;

namespace Wobigtech.Core.Comm
{
    public class NatDtoMsg
    {
        public NatCommType NatType { get; set; }
        public object NatDto { get; set; }
        public static string ConvertToString(NatCommType natType, object natDto)
        {
            return $"{natType};{JsonConvert.SerializeObject(natDto)}";
        }
        public static NatDtoMsg GetFromString(string asciiString)
        {
            var split = asciiString.Split(';');
            Enum.TryParse(split[0], out NatCommType _natType);
            return new NatDtoMsg()
            {
                NatType = _natType,
                NatDto = JsonConvert.DeserializeObject(split[1])
            };
        }
    }
    public class NatDtoGen
    {
        public string CompanionID { get; set; }
        public string Message { get; set; }
    }

    public class NatDtoCmdReq
    {
        public CommandType CmdType { get; set; }
        public string Command { get; set; }
    }

    public class NatDtoCmdReqBulk
    {
        public CommandType CmdType { get; set; }
        public List<string> CommandList { get; set; }
    }

    public class NatDtoCmdResp
    {
        public bool ActionPerformed { get; set; }
    }

    public class NatDtoJoinReq
    {
        public string HostName { get; set; }
        public string VersionNumber { get; set; }
        public string CompanionID { get; set; }
        public string CompanionSecret { get; set; }
    }

    public class NatDtoJoinResp
    {
        public string HomeServerSecret { get; set; }
    }

    public class NatDtoInitialMessage
    {
        public string CompanionID { get; set; }
        public string VersionNumber { get; set; }
    }

    public class NatDtoTelemetryInfo
    {
        public string CompanionID { get; set; }
        public float Cpu { get; set; }
        public float Ram { get; set; }
        public float Uptime { get; set; }
        public DateTime TimeSent { get; set; } = DateTime.Now.ToUniversalTime();
    }

    public class NatDtoFileUpdates
    {
        public string CompanionID { get; set; }
        public List<GameServerFilesDto> FileUpdates { get; set; }
    }
}
