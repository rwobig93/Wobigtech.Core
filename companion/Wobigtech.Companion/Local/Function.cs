using Hangfire;
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
using Wobigtech.Core.Comm;
using System.Collections.Generic;

namespace Wobigtech.Companion
{
    public static class Function
    {
        public static void InitializeLogger()
        { 
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .WriteTo.Async(c => c.File($"{PathLogs}\\Wobigtech_Companion.log", rollingInterval: RollingInterval.Day, 
                  outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .WriteTo.Async(c => c.Seq("http://192.168.1.249:5341", apiKey: "bJFq4TA6KTIENPM1YQpE"))
                .Enrich.WithCaller()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProperty("Application", "Companion App")
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
                if (arg.ToLower() == "-test")
                {
                    Constants.TestMode = true;
                    Log.Debug("Launch Arg is -test, enabling test mode");
                }
            }
            //Constants.TestMode = true;
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
            StartCommandListener();
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
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}" +
                    "|  Enter the corresponding menu number for the action you want to perform:  |{0}" +
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}" +
                    "|  1. Change Home Server URL                                                |{0}" +
                    "|  2. Open Config & Log root directory                                      |{0}" +
                    "|  3. Change GameServer Folder Location                                     |{0}" +
                    "|  4. Finish Configuration and Run                                          |{0}" +
                    "|  5. Close Companion and Don't Run                                         |{0}" +
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}" +
                    "{0}Option: ", Environment.NewLine);
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
            ValidateAllFilePaths();
            Log.Information("Finished updating settings based on config");
        }

        private static void ValidateAllFilePaths()
        {
            Log.Debug("Calling ValidateAllFilePaths()");
            List<string> directories = new List<string>
            {
                PathLogs,
                PathSourceFolder,
                PathCache,
                Config.PathGameServerLocation
            };

            foreach (var dir in directories)
            {
                try
                {
                    if (!Directory.Exists(dir))
                    {
                        Log.Debug($"Creating missing directory: {dir}");
                        Directory.CreateDirectory(dir);
                        Log.Information($"Created missing directory: {dir}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unable to create required directory");
                }
            }
            Log.Information("Finished validating required file paths");
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
                    bool answer = Shared.Tools.PromptYesNo(
                        "Wasn't able to successfully connect to the home server, would you like to try to connect to another server?");
                    Log.Debug($"Answer: {answer}");
                    if (answer)
                    {
                        Log.Debug($"Answer was {answer}, calling SetupHomeServer()");
                        SetupHomeServer();
                    }
                    else
                    {
                        Console.WriteLine("Closing companion as a connection couldn't be established, press any key to close");
                        Log.Information($"Answer was {answer}, calling CloseCompanion()");
                        Console.Read();
                        CloseCompanion();
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(Config.HomeServerSecret))
            {
                Log.Information("Home Server Secret not established yet, calling SendJoinRequestInitial()");
                SendJoinRequestInitial();
            }
            else
            {
                Log.Information("Secret is not null or empty");
            }
        }

        internal static void TestMenu()
        {
            if (!Constants.TestMode)
            {
                Log.Debug($"Test Mode is {Constants.TestMode}, skipping Test Menu");
                return;
            }
            else
            {
                Log.Information($"Test Mode is {Constants.TestMode}, enabling Test Menu");
            }
            PresentTestMenu();
            Console.Write("Home Server Socket: ");
            string response = Console.ReadLine();
            bool moveOn = false;
            while (!moveOn)
            {
                if (response.ToLower() == "q")
                {
                    Log.Debug($"Exiting Home Server Setup, answer: {response}");
                    Function.CloseCompanion();
                    return;
                }
                else if (!(ValidHomeSocket(response)))
                {
                    Console.WriteLine("Invalid host or response entered, please enter a valid socket:");
                }
                response = Console.ReadLine();
            }
        }

        private static void PresentTestMenu()
        {
            Log.Debug("Presenting Test Menu");
            while (!CloseApp)
            {
                Console.WriteLine(
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}" +
                    "|  Enter the corresponding menu number for the action you want to perform:  |{0}" +
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}" +
                    "|  1. Change Home Server URL                                                |{0}" +
                    "|  2. Open Config & Log root directory                                      |{0}" +
                    "|  3. Change GameServer Folder Location                                     |{0}" +
                    "|  4. Finish Configuration and Run                                          |{0}" +
                    "|  5. Close Companion and Don't Run                                         |{0}" +
                    "|  6. Send Messages to Nat Server                                           |{0}" +
                    "|  7. Subscribe to Nat Channel                                              |{0}" +
                    "|  8. Install/Update Test Game                                              |{0}" +
                    "|~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|{0}", Environment.NewLine);
                Console.Write($"{Environment.NewLine}Test Option: ");
                var answer = Console.ReadLine();
                Log.Debug($"Test Menu answer was: {answer}");
                if (!int.TryParse(answer, out int intAnswer))
                {
                    Log.Debug("Test Menu answer entered was an invalid response");
                    Console.WriteLine("Test Answer wasn't invalid, please try again");
                    Thread.Sleep(3000);
                }
                else
                {
                    Log.Debug($"Valid test menu option {intAnswer} was entered");
                    switch (intAnswer)
                    {
                        case 1:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling SetupHomeServer()");
                            SetupHomeServer();
                            break;
                        case 2:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling OpenAppStorage()");
                            OpenAppStorage();
                            break;
                        case 3:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling ChangeServerDirectory()");
                            ChangeGameServerDirectory();
                            break;
                        case 4:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling FinishConfig()");
                            FinishConfig();
                            break;
                        case 5:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling CloseCompanion()");
                            CloseCompanion();
                            break;
                        case 6:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling TestNatMessages()");
                            TestNatMessages();
                            break;
                        case 7:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling TestNatListener()");
                            TestNatListener();
                            break;
                        case 8:
                            Log.Debug($"Test Menu option {intAnswer} was selected, calling TestSteamCMDInstall()");
                            TestSteamCMDInstall();
                            break;
                        default:
                            Log.Debug("Test Answer entered wasn't a valid presented option");
                            Console.WriteLine("Test Answer entered isn't one of the options, please try again");
                            Thread.Sleep(3000);
                            break;
                    }
                }
                Console.Clear();
            }
            Log.Information("Exited test menu");
        }

        private static void TestSteamCMDInstall()
        {
            Log.Debug("Running test SteamCMD Game Install for app ID 443030");
            string command = $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login anonymous +force_install_dir " +
                $"\"{Constants.PathSourceFolder}\\ConanExiles\" +app_info_update 1 +app_update 443030 +app_status 443030";
            //string command = $"+login anonymous +force_install_dir \"{Constants.PathSourceFolder}\\ConanExiles\" +app_update 443030 validate";
            SteamCMD.SteamCMDCommand(command);
            Log.Debug("Finished running test SteamCMD Game install for app ID 443030");
        }

        private static void TestNatListener()
        {
            Console.WriteLine("Enter 'q' to exit this menu");
            bool moveOn = false;
            while (!moveOn)
            {
                try
                {
                    Console.Write("Enter Nat Subject to Listen to: ");
                    string response = Console.ReadLine();
                    if (response.ToLower() == "q")
                    {
                        Log.Debug($"Exiting Test Nat Listener, answer: {response}");
                        moveOn = true;
                    }
                    else
                    {
                        NatConn.SubscribeAsync(response, (s, a) =>
                        {
                            Log.Debug($"NAT-LISTEN: [sub]{response} [msg]{a.Message}");
                            Console.WriteLine($"NAT-LISTEN: [sub]{response} [msg]{a.Message}");
                        });
                        Console.WriteLine($"Nat Listener started: {response}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"NAT-LISTEN EXCEPTION: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    Console.WriteLine($"NAT-LISTEN EXCEPTION: {ex.Message}");
                }
            }
        }

        private static void TestNatMessages()
        {
            Console.WriteLine("Enter 'q' to exit this menu");
            Console.WriteLine("Enter 'c' to change Nat Subjects");
            string subject = "";
            bool moveOn = false;
            while (!moveOn)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(subject))
                    {
                        Console.Write("Enter Nat Subject to publish to: ");
                        subject = Console.ReadLine();
                    }
                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        Console.WriteLine($"Enter Nat Message to publish to [{subject}]: ");
                        string response = Console.ReadLine();
                        if (response.ToLower() == "q")
                        {
                            Log.Debug($"Exiting Test Nat Publisher, answer: {response}");
                            moveOn = true;
                        }
                        else if (response.ToLower() == "c")
                        {
                            Console.Write("Enter Nat Subject to publish to: ");
                            subject = Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine($"Publishing [{subject}]: {response}");
                            NatConn.Publish(subject, NatComm.NatMsgSend(NatCommType.CmdReq, response));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"NAT Subject [{subject}] is still null or empty, rolling back...");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"NAT-PUB EXCEPTION: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    Console.WriteLine($"NAT-PUB EXCEPTION: {ex.Message}");
                }
            }
        }
    }
}
