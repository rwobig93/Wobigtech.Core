using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Wobigtech.Companion.Handlers;

namespace Wobigtech.Companion.Local
{
    public static class SteamCMD
    {
        public static bool SteamCMDCommand(string command, DataReceivedEventHandler outputHandler = null, EventHandler exitHandler = null)
        {
            // .\steamcmd.exe +login anonymous +force_install_dir "ConanExiles" +app_update 443030 validate +quit
            // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login ${SteamLoginName} +app_info_update 1 +app_status ${SteamAppID} +quit
            // https://steamapi.xpaw.me/
            Log.Debug($"Starting SteamCMDCommand({command}, {outputHandler}, {exitHandler})");
            try
            {
                if (command == "update")
                {
                    command = "";
                }
                Process steamCMD = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = OSDynamic.GetSteamCMDPath(),
                        Arguments = command
                    }
                };
                if (null != outputHandler)
                {
                    Log.Debug("outputHandler wasn't null, using specified callback");
                    steamCMD.OutputDataReceived += outputHandler;
                }
                else
                {
                    Log.Debug("outputHanlder was null, using default event handler");
                    steamCMD.OutputDataReceived += Events.SteamCMD_OutputDataReceived;
                }
                if (null != exitHandler)
                {
                    Log.Debug("exitHandler wasn't null, using specified callback");
                    steamCMD.Exited += exitHandler;
                }
                else
                {
                    Log.Debug("exitHandler was null, using default event handler");
                    steamCMD.Exited += Events.SteamCMD_Exited;
                }
                steamCMD.Start();
                Log.Information($"Ran SteamCMD Command: {command}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"SteamCMDCommand Error: {ex.Message}");
                return false;
            }
        }

        public static void ResetSteamCMDToFresh()
        {
            Log.Debug("Starting ResetSteamCMDToFresh()");
            DirectoryInfo steamDir = new DirectoryInfo(OSDynamic.GetSteamCMDDirectory());
            foreach (FileInfo file in steamDir.EnumerateFiles())
            {
                if (file.Name.ToLower() == "steamcmd.exe")
                {
                    Log.Debug($"Skipping steamcmd.exe: {file.FullName}");
                }
                else
                {
                    Log.Debug($"Deleting File: {file.FullName}");
                    file.Delete();
                }
            }
            foreach (DirectoryInfo dir in steamDir.EnumerateDirectories())
            {
                Log.Debug($"Deleting directory recursively: {dir.FullName}");
                dir.Delete(true);
            }
            Log.Information($"Finished resetting steam directory to fresh: {OSDynamic.GetSteamCMDDirectory()}");
        }

        public static void CleanupCache()
        {
            Log.Debug($"Starting CleanupCache() at {Constants.PathCache}");
            DirectoryInfo cacheFolder = new DirectoryInfo(Constants.PathCache);
            foreach (FileInfo file in cacheFolder.EnumerateFiles())
            {
                Log.Debug($"Deleting file: {file.FullName}");
                file.Delete();
            }
            foreach (DirectoryInfo dir in cacheFolder.EnumerateDirectories())
            {
                Log.Debug($"Deleting directory recursively: {dir.FullName}");
                dir.Delete(true);
            }
            Log.Information($"Finished cleaning up cache folder: {Constants.PathCache}");
        }

        public static void ExtractSteamCMD()
        {
            Log.Debug("Starting ExtractSteamCMD()");
            ZipFile.ExtractToDirectory(Path.Combine(Constants.PathCache, "steamcmd.zip"), Constants.PathSourceFolder);
            Log.Information("Finished extracting SteamCMD to source folder");
        }

        public static void DownloadSteamCMD()
        {
            Log.Debug("Starting DownloadSteamCMD()");
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += Events.WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += Events.WebClient_DownloadFileCompleted;
                webClient.DownloadFile(Constants.URLSteamCMDDownload, Path.Combine(Constants.PathCache, "steamcmd.zip"));
            }
            Log.Debug("Finished downloading SteamCMD");
        }

        public static void UpdateSteamCMD()
        {
            Log.Debug("Starting UpdateSteamCMD()");
            SteamCMD.SteamCMDCommand("update");
            Log.Information("Started SteamCMD Update");
        }

    }
}
