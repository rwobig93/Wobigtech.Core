using NATS.Client;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using Wobigtech.Companion.Handlers;
using Wobigtech.Companion.Shared;
using Wobigtech.Core.Comm;
using Wobigtech.Core.Crypto;
using Wobigtech.Core.Dto;
using Wobigtech.Core.Enums;
using static Wobigtech.Companion.Constants;

namespace Wobigtech.Companion.Local
{
    public static class Housekeeping
    {

        public static void ChangeLoggingLevel(LogEventLevel loggingLevel)
        {
            #if DEBUG
            loggingLevel = LogEventLevel.Debug;
            #endif
            Log.Debug($"Updating Logging level from {LevelSwitch.MinimumLevel} to {loggingLevel}");
            LevelSwitch.MinimumLevel = loggingLevel;
            Log.Information($"Updated logging level to {LevelSwitch.MinimumLevel}");
        }

        public static bool GenerateSecret()
        {
            Log.Debug("Starting GenerateSecret()");
            Constants.Config.CompanionSecret = CBase.GenerateString(128);
            Log.Debug("Generated Secret, hashing secret");
            Constants.Config.CompanionSecretHash = CBase.CalculateHash(Constants.Config.CompanionSecret);
            Log.Debug("Hash generated, verifying hash is valid");

            var hashValidated = CBase.CheckHashMatch(Constants.Config.CompanionSecretHash, Constants.Config.CompanionSecret);
            if (hashValidated)
            {
                Log.Information("Secret created, hashed and validated");
                return true;
            }
            else
            {
                Log.Error($"Secret generation failed secret[{Constants.Config.CompanionSecret}] hash[{Constants.Config.CompanionSecretHash}] valid[{hashValidated}]");
                return false;
            }
        }

        internal static void ReportFileChanges()
        {
            Log.Debug("Starting ReportFileChanges()");
            var serverChanges = Constants.RepoFileChanges.Count;
            var totalChanges = 0;
            foreach (var server in Constants.RepoFileChanges.ToList())
            {
                totalChanges = totalChanges +
                    server.Executables.Count +
                    server.FilesScripts.Count +
                    server.FilesLogs.Count;
            }
            if (Constants.RepoFileChanges.Count > 0)
            {
                Constants.NatConn.Publish(NatSubjects.FileUpdates, NatComm.NatMsgSend(NatCommType.FileUp, new NatDtoFileUpdates()
                {
                    CompanionID = Constants.Config.CompanionID,
                    FileUpdates = Constants.RepoFileChanges
                }));
                Log.Debug("Clearing RepoFileChanges");
                Constants.RepoFileChanges.Clear();
                Log.Information($"Sent FileChanges for {serverChanges} server(s): {totalChanges} changes detected");
            }
            else
            {
                Log.Debug("No Repo file changes, skipping update");
            }
        }

        public static void HideConsole()
        {
            Log.Debug("Starting HideConsole()");
            WINAPI.WindowStateChange(WINAPIWindowState.SW_HIDE);
            Log.Information("Console window hidden");
        }

        public static void ShowConsole()
        {
            Log.Debug("Starting ShowConsole()");
            WINAPI.WindowStateChange(WINAPIWindowState.SW_RESTORE);
            Log.Information("Console window restored");
        }

        public static void OpenAppStorage()
        {
            Log.Debug("Starting OpenAppStorage()");
            OSDynamic.MoveToAppDirectory();
        }

        internal static void GameServerScan(string path)
        {
            Log.Debug($"Starting GameServerScan({path})");
            if (!OSDynamic.DirectoryExists(path))
            {
                Log.Information($"GameServerPath doesn't exist: {path}");
                return;
            }
            Log.Debug($"GameServerPath exists, starting scan: {path}");
            
            OSDynamic.RecursiveFileScan(path);
            Log.Information("Finished Game Server Path Scan");
        }

        internal static void StartDirectoryWatcher(string watchPath)
        {
            Log.Debug("Starting StartDirectoryWatcher()");
            FileSystemWatcher watcher = new FileSystemWatcher()
            {
                Path = watchPath,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };
            watcher.Changed += Events.Watcher_Changed;
            watcher.Created += Events.Watcher_Created;
            Log.Information($@"Started Directory Watcher: {watchPath}");
        }

        public static void ChangeGameServerDirectory()
        {
            Log.Debug("Starting ChangeGameServerDirectory()");
            bool answered = false;
            while (!answered)
            {
                Console.WriteLine(
                    $"Where would you like all of your game servers to be located? (type q to quit){Environment.NewLine}" +
                    $"***NOTE: Please choose a directory that follows these guidelines:{Environment.NewLine}" +
                    $"         - A folder dedicated to only this application{Environment.NewLine}" +
                    $"         - A folder that doesn't require administrative rights (no Program Files or Windows folders){Environment.NewLine}" +
                    $"           ***If you use an admininistrative required folder this application will require admin rights everytime it's run{Environment.NewLine}" +
                    $"         - If you don't enter anything this will default to the recommended directory:{Environment.NewLine}" +
                    $"           {OSDynamic.GetDefaultGameServerPath()}{Environment.NewLine}");
                Console.Write($"Folder Path: ");
                string answer = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(answer))
                {
                    answer = OSDynamic.GetDefaultGameServerPath();
                }
                else if (answer.ToLower() == "q")
                {
                    Log.Debug("Server directory change was canceled");
                    return;
                }
                bool verifyLocation = Tools.PromptYesNo(
                    $"You chose: {answer}{Environment.NewLine}" +
                    $"Is this the location you want your game servers to be stored?");
                if (verifyLocation)
                {
                    if (!Directory.Exists(answer))
                    {
                        bool verifyDirCreate = Tools.PromptYesNo("This directory doesn't exist, would you like it to be created? (if not you will be prompted for a new path)");
                        if (verifyDirCreate)
                        {
                            Log.Debug($"Attempting to create directory: {answer}");
                            var newDir = Directory.CreateDirectory(answer);
                            Log.Debug($"Created directory: {answer}");
                            Constants.Config.PathGameServerLocation = newDir.FullName;
                            Log.Information($"Created new directory and updated gameServerLocation to: {Constants.Config.PathGameServerLocation}");
                            answered = true;
                        }
                        else
                        {
                            Console.Clear();
                        }
                    }
                    else
                    {
                        Log.Debug("Directory already exists, updating gameServerLocation");
                        Constants.Config.PathGameServerLocation = new DirectoryInfo(answer).FullName;
                        Log.Information($"Updated gameServerLocation to: {Constants.Config.PathGameServerLocation}");
                        answered = true;
                    }
                }
                else
                {
                    Console.Clear();
                }
            }
        }

        internal static void ValidateGameWatchCreate(FileSystemEventArgs e)
        {
            Log.Debug($"Starting ValidateGameWatchCreate({e.ChangeType}|{e.Name})");
            if (e.FullPath.StartsWith(PathSourceFolder))
            {
                Log.Debug($"Folder is source, ignoring: {e.FullPath}");
                return;
            }

            ParseGameServerFiles(e.FullPath);
        }

        internal static void ParseGameServerFiles(string path)
        {
            GameServerFileType fileType = GameServerFileType.None;
            GameServerFilesDto fileDto = new GameServerFilesDto();

            var prePath = $@"{path.Replace(Constants.Config.PathGameServerLocation, "")}".Split('\\');
            fileDto.Game = prePath[0];
            fileDto.Server = prePath[1];
            string pathLower = path.ToLower();

            // Executables
            if (pathLower.EndsWith(".exe"))
            {
                Log.Debug($"Adding executable to list: {pathLower}");
                fileDto.Executables.Add(path);
                fileType = GameServerFileType.Executable;
            }
            // Scripts
            else if (pathLower.EndsWith(".cmd") || pathLower.EndsWith(".bat"))
            {
                Log.Debug($"Adding script to list: {pathLower}");
                fileDto.FilesScripts.Add(path);
                fileType = GameServerFileType.Script;
            }
            // Logs
            else if (pathLower.EndsWith(".log") || pathLower.EndsWith(".logs"))
            {
                Log.Debug($"Adding log to list: {pathLower}");
                fileDto.FilesLogs.Add(path);
                fileType = GameServerFileType.Log;
            }
            // Catch-all
            else
            {
                Log.Verbose($"File hit catch-all and is being ignored: {pathLower}");
                return;
            }

            if (fileType != GameServerFileType.None)
            {
                var existingFound = RepoFileChanges.Find(x => x.Game == fileDto.Game && x.Server == fileDto.Server);
                if (null != existingFound)
                {
                    Log.Debug($"Existing Game/Server Entry, updating: [game/server]{existingFound.Game}/{existingFound.Server} " +
                        $"[type]{fileType} [path]{pathLower}");
                    switch (fileType)
                    {
                        case GameServerFileType.Executable:
                            existingFound.Executables.Add(fileDto.Executables[0]);
                            break;
                        case GameServerFileType.Script:
                            existingFound.FilesScripts.Add(fileDto.FilesScripts[0]);
                            break;
                        case GameServerFileType.Log:
                            existingFound.FilesLogs.Add(fileDto.FilesLogs[0]);
                            break;
                        case GameServerFileType.None:
                            break;
                    }
                }
                else
                {
                    Log.Debug($"New Game/Server Entry: [game/server]{fileDto.Game}/{fileDto.Server} " +
                        $"[type]{fileType} [path]{pathLower}");
                    RepoFileChanges.Add(fileDto);
                }
            }
        }

        internal static void ValidateGameWatchChange(FileSystemEventArgs e)
        {
            Log.Debug($"Starting ValidateGameWatchChange({e.ChangeType}|{e.Name})");
        }

        public static bool VerifyHomeServerConnectivity()
        {
            SetupCoreConnection();

            try
            {
                NatConn = NatFactory.CreateConnection(Constants.Config.HomeServerSocket);
            }
            catch (Exception ex)
            {
                Log.Error($"Nat Connection failed to connect to [{Constants.Config.HomeServerSocket}]: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }

            if (NatConn.State == ConnState.CONNECTED)
            {
                Log.Information($"Nats connection was successful! [{Constants.Config.HomeServerSocket}]");
                return true;
            }
            else
            {
                Log.Information($"Nats connection state on natConn isn't connected it is: {NatConn.State} | [{Constants.Config.HomeServerSocket}]");
                return false;
            }
        }

        public static void SetupCoreConnection()
        {
            Log.Debug("Starting SetupCoreConnection()");
            if (NatFactory == null)
            {
                Log.Debug("natFactory was null, calling InitializeNatFactory()");
                InitializeNatFactory();
            }
            if (NatConn != null)
            {
                Log.Debug("natConn isn't null, flushing and closing previous connection");
                NatConn.Flush();
                NatConn.Close();
                Log.Information("Flushed and closed previous natConn");
            }
            Log.Information("Core connection setup");
        }

        public static void SendJoinRequest()
        {
            Log.Debug("Starting SendJoinRequest()");
            OpenJoinListener();
            NatConn.Publish(NatSubjects.Join, NatComm.NatMsgSend(NatCommType.JoinReq, new NatDtoJoinReq()
            {
                HostName = Environment.MachineName,
                CompanionID = Constants.Config.CompanionID,
                CompanionSecret = Constants.Config.CompanionSecret
            }));
            Log.Information("Sent join request");
            Console.WriteLine($"Join request sent to {Constants.Config.HomeServerSocket}, now waiting on accept");
        }

        public static void OpenJoinListener()
        {
            Log.Debug("Starting join listener");
            Constants.Config.HomeServerSecret = "";
            Log.Debug("Cleared HomeServerSecret");
            NatConn.SubscribeAsync(Constants.Config.CompanionSecretHash, Events.NatJoinMsgHandler);
            Log.Information("Join listener started");
        }

        public static void InitializeNatFactory()
        {
            Log.Debug("Initializing new Nat Factory");
            NatFactory = new ConnectionFactory();
            Log.Information("Initialized Nat Factory");
        }

        public static void CreateNewConfig()
        {
            Log.Debug("Creating new config");
            Constants.Config = new Shared.Config();
            Log.Information("Instantiated new config file");
        }

        public static void SetupHomeServer()
        {
            Log.Information("Starting home server setup");
            Console.WriteLine("Please enter the home server in socket form | Example: games.wobigtech.net:9595 | [URL/Hostname]:[Port Number] | Type q to quit");
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
                    response = Console.ReadLine();
                }
                else
                {
                    Log.Information("Valid response entered, continuing");
                    Console.WriteLine("Socket entered verified to be reachable!");
                    moveOn = true;
                }
            }

            bool updateStatus = UpdateHomeServer();
            if (!updateStatus)
            {
                Log.Error("Nats connection was successful but home server address socket couldn't be updated");
                Console.WriteLine("Connection was successful to the server but updating the home server failed, please report this error!");
                Console.ReadLine();
                Function.CloseCompanion();
            }
        }

        public static bool UpdateHomeServer()
        {
            Log.Debug("Attempting to update HomeServerSocket");
            if (NatConn.ConnectedUrl != null)
            {
                Log.Debug("ConnectedUrl isn't null");
                Constants.Config.HomeServerSocket = NatConn.ConnectedUrl;
                Log.Information($"Successfully updated HomeServerSocket to: {Constants.Config.HomeServerSocket}");
                return true;
            }
            else
            {
                Log.Information("Failed updating HomeServerSocket");
                return false;
            }
        }

        public static bool ValidHomeSocket(string response)
        {
            Log.Debug("Validating entered home socket");
            string port;
            try
            {
                string hostURI = response.Split(':')[0];
                port = response.Split(':')[1];
                Log.Information($"Entered: Host[{hostURI}] Port[{port}]");
            }
            catch (Exception)
            {
                Log.Debug($"Entered: {response} which wasn't parsable");
                Console.WriteLine("You entered an invalid response, please try again");
                return false;
            }

            bool portIsNum = int.TryParse(port, out int outNum);
            if (!portIsNum)
            {
                Log.Information("Port entered isn't valid");
                Console.WriteLine("Port entered isn't valid, please validate and try again");
                return false;
            }
            else if (outNum > 65535)
            {
                Log.Information("Port entered is above port limit");
                Console.WriteLine("Port entered isn't valid, please validate and try again");
                return false;
            }

            Constants.Config.HomeServerSocket = $"nats://{response}";
            if (!VerifyHomeServerConnectivity())
            {
                Log.Information("Home Server Connectivity test failed");
                Console.WriteLine("Connectivity to the entered socket failed, please try again");
                return false;
            }
            else
            {
                Log.Information("Socket entered is valid and connnectivity succeeded");
                return true;
            }
        }
    }
}
