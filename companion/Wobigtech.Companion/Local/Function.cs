﻿using Hangfire;
using Hangfire.MemoryStorage;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static Wobigtech.Companion.Constants;
using static Wobigtech.Companion.Local.Housekeeping;
using Wobigtech.Core.Enums;
using Wobigtech.Companion.Handlers;
using Wobigtech.Companion.Local;

namespace Wobigtech.Companion
{
    public static class Function
    {
        public static void InitializeLogger()
        { 
            Logger = new LoggerConfiguration()
                .WriteTo.File($"{PathLogs}\\Wobigtech_Companion.log", rollingInterval: RollingInterval.Day)
                .MinimumLevel.ControlledBy(LevelSwitch)
                .CreateLogger();
#if DEBUG
            LevelSwitch.MinimumLevel = LogEventLevel.Debug;
#else
            LevelSwitch.MinimumLevel = LogEventLevel.Information;
#endif
            Log.Information("Logger started");
        }

        internal static void ParseLaunchArgs(string[] args)
        {
            foreach (var arg in args)
            {
                Log.Information($"Launch arg passed: {arg}");
                if (arg.ToLower() == "-debug")
                {
                    Constants.LevelSwitch.MinimumLevel = LogEventLevel.Debug;
                    Log.Debug("Launch Arg is -debug, switched logging to debug");
                }
            }
        }

        public static void CloseCompanion()
        {
            Log.Information("Closing Companion");
            StopTelemetry();
            CloseApp = true;
        }

        internal static void StartCompanionWork()
        {
            if (CloseApp)
            {
                return;
            }
            Log.Debug("Starting StartCompanionWork()");
            if (!VerifyHomeServerConnectivity())
            {
                Log.Error("Connectivity to the home server wasn't successful");
            }
            else
            {
                Log.Information("Connectivity successfully established with home server");
            }
            StartTelemetry();
            ValidateSteamCMDInstall();
            ScanGameServerFolder();
            StartTimedJobs();
            StartListeners();
            StartCommandListener();
            Log.Information("Finished starting companion jobs");
        }

        private static void ScanGameServerFolder()
        {
            Log.Debug("Starting ScanGameServerFolder()");
            GameServerScan(Config.PathGameServerLocation);
            ReportFileChanges();
        }

        private static void StartListeners()
        {
            Log.Debug("Starting StartListeners()");
            StartDirectoryWatcher(Config.PathGameServerLocation);
        }

        private static void StartTimedJobs()
        {
            Log.Debug("Starting StartTimedJobs()");
            InitializeHangFire();
            //RecurringJob.AddOrUpdate(() => Jobs.GameAndModUpdater15Min(), Jobs.GetCronString(CronTime.MinFifteen));
            RecurringJob.AddOrUpdate(() => Jobs.GameServerFileUpdater15Min(), Jobs.GetCronString(CronTime.MinFifteen));
            Log.Information("Finished starting timed jobs");
        }

        private static void InitializeHangFire()
        {
            Log.Debug("Starting InitializeHangFire()");
            GlobalConfiguration.Configuration.UseMemoryStorage();
            Log.Information("Hangfire successfully initialized");
        }

        private static void ValidateSteamCMDInstall()
        {
            Log.Debug("Starting ValidateSteamCMDInstall()");
            if (!OSDynamic.FileExists(OSDynamic.GetSteamCMDPath()))
            {
                Log.Debug($"SteamCMD source not found, downloading, extracting and cleaning up | " +
                    $"{OSDynamic.GetSteamCMDPath()}");
                SteamCMD.DownloadSteamCMD();
                SteamCMD.ExtractSteamCMD();
                SteamCMD.CleanupCache();
            }
            else
            {
                Log.Information($"SteamCMD source found: {OSDynamic.GetSteamCMDPath()}");
            }
            Log.Information("SteamCMD source validation finished");
            SteamCMD.UpdateSteamCMD();
        }

        private static void StartCommandListener()
        {
            Log.Debug("Starting StartCommandListener()");
            NatConn.SubscribeAsync($"{Config.CompanionID}_CMD", Events.CommandListener);
            Log.Information($"Command listener started: {Config.CompanionID}_CMD");
        }

        private static void StartTelemetry()
        {
            Log.Debug("Starting StartTelemetry()");
            if (TelemetryWorker == null)
            {
                Log.Debug("Telemetry worker was null, instantiating a new one");
                TelemetryWorker = new BackgroundWorker();
                TelemetryWorker.ProgressChanged += Events.TelemetryWorker_ProgressChanged;
                TelemetryWorker.WorkerReportsProgress = true;
                TelemetryWorker.DoWork += Events.TelemetryWorker_DoWork;
                TelemetryWorker.RunWorkerCompleted += Events.TelemetryWorker_RunWorkerCompleted;
                WorkerTelemetryRunning = true;
                TelemetryWorker.RunWorkerAsync();
                Log.Information("Telemetry worker started");
            }
            else
            {
                Log.Warning("Telemetry worker not null, not instantiating another");
            }
        }

        private static void StopTelemetry()
        {
            Log.Debug("Starting StopTelemetry()");
            if (WorkerTelemetryRunning)
            {
                WorkerTelemetryRunning = false;
                TelemetryWorker.Dispose();
                Log.Information("Stopped telemetry worker");
            }
            else
            {
                Log.Warning("Telemetry worker already stopped");
            }
        }

        internal static void PresentMenu()
        {
            Log.Debug("Presenting Menu");
            while (!CloseApp)
            {
                Console.WriteLine(
                    $"|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{Environment.NewLine}" +
                    $"|  Enter the corresponding menu number for the action you want to perform:  |{Environment.NewLine}" +
                    $"|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{Environment.NewLine}" +
                    $"|  1. Change Home Server URL                                                |{Environment.NewLine}" +
                    $"|  2. Open Config & Log root directory                                      |{Environment.NewLine}" +
                    $"|  3. Change GameServer Folder Location                                     |{Environment.NewLine}" +
                    $"|  4. Finish Configuration and Run                                          |{Environment.NewLine}" +
                    $"|  5. Close Companion and Don't Run                                         |{Environment.NewLine}" +
                    $"|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{Environment.NewLine}" +
                    $"{Environment.NewLine}Option: ");
                var answer = Console.ReadLine();
                Log.Debug($"Menu answer was: {answer}");
                if (!int.TryParse(answer, out int intAnswer))
                {
                    Log.Debug("Menu answer entered was an invalid response");
                    Console.WriteLine("Answer wasn't invalid, please try again");
                    Thread.Sleep(3000);
                }
                else
                {
                    Log.Debug($"Valid menu option {intAnswer} was entered");
                    switch (intAnswer)
                    {
                        case 1:
                            Log.Debug($"Menu option {intAnswer} was selected, calling SetupHomeServer()");
                            SetupHomeServer();
                            break;
                        case 2:
                            Log.Debug($"Menu option {intAnswer} was selected, calling OpenAppStorage()");
                            OpenAppStorage();
                            break;
                        case 3:
                            Log.Debug($"Menu option {intAnswer} was selected, calling ChangeServerDirectory()");
                            ChangeGameServerDirectory();
                            break;
                        case 4:
                            Log.Debug($"Menu option {intAnswer} was selected, calling FinishConfig()");
                            FinishConfig();
                            break;
                        case 5:
                            Log.Debug($"Menu option {intAnswer} was selected, calling CloseCompanion()");
                            CloseCompanion();
                            break;
                        default:
                            Log.Debug("Answer entered wasn't a valid presented option");
                            Console.WriteLine("Answer entered isn't one of the options, please try again");
                            Thread.Sleep(3000);
                            break;
                    }
                }
                Console.Clear();
            }
            Log.Information("Exited menu");
        }

        private static void FinishConfig()
        {
            Log.Debug("Starting FinishConfig()");
            SaveConfig();
            Console.WriteLine("Configuration saved, console will now close and run in the background!");
            Console.WriteLine($"{Environment.NewLine}Press any key to continue...");
            Console.Read();
            HideConsole();
        }

        internal static void InitializeFirstRun()
        {
            Log.Debug("Calling InitializeFirstRun()");
            CreateNewConfig();
            ChangeGameServerDirectory();
            SetupHomeServer();
            if (CloseApp)
            {
                return;
            }
            SaveConfig();
            Log.Information("Finished InitializeFirstRun()");
        }

        public static void Cleanup()
        {
            Log.Information("Closing Companion and Cleaning Up Now");
        }

        public static void TriggerSettingsByConfig()
        {
            Log.Debug("Updating settings based on config file");
            ChangeLoggingLevel(Config.LoggingLevel);
            Log.Information("Finished updating settings based on config");
        }
        public static StatusReturn LoadConfig()
        {
            Log.Debug("Starting LoadConfig()");
            string configFile = $@"{PathConfigDefault}\config.json";
            Log.Debug($"configFile = {configFile}");
            if (File.Exists(configFile))
            {
                Log.Debug("Attempting to load config file");
                var configLoaded = File.ReadAllText(configFile);
                Config = JsonConvert.DeserializeObject<Shared.Config>(configLoaded);
                Log.Information("Successfully deserialized config file");
                return StatusReturn.Success;
            }
            else
            {
                Log.Information("Config file wasn't found");
                return StatusReturn.NotFound;
            }
        }

        public static StatusReturn SaveConfig()
        {
            Log.Debug("Starting SaveConfig()");
            if (!Directory.Exists(PathConfigDefault))
            {
                Log.Debug($"Config path doesn't exist, attempting to create dir: {PathConfigDefault}");
                Directory.CreateDirectory(PathConfigDefault);
                Log.Information($"Created missing config dir: {PathConfigDefault}");
            }
            string configFile = $@"{PathConfigDefault}\config.json";
            Log.Debug($"configFile = {configFile}");
            if (File.Exists(configFile))
            {
                Log.Information("Attempting to save over current config file");
            }
            else
            {
                Log.Information("Attempting to save a new config file");
            }
            File.WriteAllText(configFile, JsonConvert.SerializeObject(Config));
            Log.Information("Successfully serialized config file");
            return StatusReturn.Success;
        }

        public static void DisplayHostInfo()
        {
            Log.Debug("Displaying host info");
            Console.WriteLine(
                $"Hostname:                {Environment.MachineName}{Environment.NewLine}" +
                $"Current OS Platform:     {OSDynamic.GetCurrentOS()}{Environment.NewLine}" +
                $"Current OS Architecture: {RuntimeInformation.OSArchitecture}{Environment.NewLine}" +
                $"Current OS Description:  {RuntimeInformation.OSDescription}{Environment.NewLine}" +
                $"Current Process Arch:    {RuntimeInformation.ProcessArchitecture}{Environment.NewLine}" +
                $"Current Framework:       {RuntimeInformation.FrameworkDescription}{Environment.NewLine}" +
                $"Logging Path:            {PathLogs}{Environment.NewLine}" +
                $"Config Path:             {PathConfigDefault}{Environment.NewLine}" +
                $"GameServer Path:         {Config.PathGameServerLocation}{Environment.NewLine}");
            Log.Information("Host info Displayed");
        }

        private static void SendJoinRequestInitial()
        {
            Log.Debug("Starting SendJoinRequestInitial()");
            GenerateSecret();
            SendJoinRequest();
        }

        internal static void SetupConnection()
        {
            if (CloseApp)
            {
                return;
            }
            bool connected = VerifyHomeServerConnectivity();
            if (!connected)
            {
                Log.Debug($"Connection to the home server failed: {Config.HomeServerSocket} | Starting attempt loop");
                int counter = 5;
                while (!connected && counter > 0)
                {
                    Log.Error($"Connection to the home server failed: {Config.HomeServerSocket}");
                    Console.WriteLine($"Connection to the home server failed, will try again, tries left: {counter}");
                    Thread.Sleep(1000);
                    connected = VerifyHomeServerConnectivity();
                    counter--;
                }
                if (!connected)
                {
                    Console.WriteLine("Wasn't able to successfully connect to the home server, would you like to try to connect to another server?");
                    string answer = Console.ReadLine();
                    Log.Debug($"Answer: {answer}");
                    while (answer.ToLower() != "y" || answer.ToLower() != "n")
                    {
                        Console.WriteLine("Invalid answer, please try again");
                        answer = Console.ReadLine();
                        Log.Debug($"Answer: {answer}");
                    }
                    if (answer.ToLower() == "y")
                    {
                        Log.Information($"Answer was {answer}, calling SetupHomeServer()");
                        SetupHomeServer();
                    }
                    else
                    {
                        Console.WriteLine("Closing companion as a connection couldn't be established");
                        Log.Information($"Answer was {answer}, calling CloseCompanion()");
                        Console.ReadLine();
                        CloseCompanion();
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(Config.HomeServerSecret))
            {
                Log.Information("Home Server Secret not established yet, calling InitializeSecret()");
                SendJoinRequestInitial();
            }
            else
            {
                Log.Information("Secret is not null or empty");
            }
        }
    }
}
