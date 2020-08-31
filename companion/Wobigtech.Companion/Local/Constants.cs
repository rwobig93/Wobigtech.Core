using NATS.Client;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Wobigtech.Companion.Shared;
using Wobigtech.Core.Comm;
using Wobigtech.Core.Dto;
using Wobigtech.Core.Enums;

namespace Wobigtech.Companion
{
    public static class Constants
    {
        // Strings
        public static string PathConfigDefault { get; set; } = OSDynamic.GetConfigPath();
        public static string PathLogs { get; set; } = OSDynamic.GetLoggingPath();
        public static string PathCache { get { return $@"{Config.PathGameServerLocation}\.Cache"; } }
        public static string PathSourceFolder { get { return $@"{Config.PathGameServerLocation}\.Source"; } }
        public static string URLSteamCMDDownload { get { return "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip"; } }
        public static string CertPass { get; } = $"{Environment.MachineName}_WT";
        // Bools
        public static bool CloseApp { get; set; } = false;
        public static bool WorkerTelemetryRunning { get; set; } = false;
        public static bool TestMode { get; set; } = false;
        // Classes
        public static Logger Logger { get; set; }
        public static Config Config { get; set; }
        public static ConnectionFactory NatFactory { get; set; }
        public static IConnection NatConn { get; set; }
        public static LoggingLevelSwitch LevelSwitch { get; set; } = new LoggingLevelSwitch();
        public static BackgroundWorker TelemetryWorker { get; set; }
        // Repositories
        public static ConcurrentBag<NatDtoTelemetryInfo> TelemetryBag { get; set; } = new ConcurrentBag<NatDtoTelemetryInfo>();
        public static List<GameServerFilesDto> RepoFileChanges { get; set; } = new List<GameServerFilesDto>();
    }
}
