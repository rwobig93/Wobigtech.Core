﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wobigtech.Companion.Dto;
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

                RunSteamCMD(new RunSteamDto()
                {
                    Command = command,
                    OutputHandler = outputHandler,
                    ExitHandler = exitHandler
                });

                Log.Information($"Ran SteamCMD Command: {command}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"SteamCMDCommand Error: {ex.Message}");
                return false;
            }
        }

        private static void RunSteamCMD(RunSteamDto runSteamDto)
        {
            Log.Debug("Starting RunSteamCMD");

            Process steamCMD = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = OSDynamic.GetSteamCMDPath(),
                    Arguments = runSteamDto.Command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            if (runSteamDto.OutputHandler != null)
            {
                Log.Debug("outputHandler wasn't null, using specified callback");
                steamCMD.OutputDataReceived += runSteamDto.OutputHandler;
            }
            else
            {
                Log.Debug("outputHanlder was null, using default event handler");
                steamCMD.OutputDataReceived += Events.SteamCMD_OutputDataReceived;
            }
            if (runSteamDto.ExitHandler != null)
            {
                Log.Debug("exitHandler wasn't null, using specified callback");
                steamCMD.Exited += runSteamDto.ExitHandler;
            }
            else
            {
                Log.Debug("exitHandler was null, using default event handler");
                steamCMD.Exited += Events.SteamCMD_Exited;
            }

            StreamWriter input = steamCMD.StandardInput;
            StreamReader output = steamCMD.StandardOutput;
            StreamReader errors = steamCMD.StandardError;

            steamCMD.Start();

            Task.Run(async () => await HandleSteamCMDOutputAsync(input, output, errors));
            //Task run = HandleSteamCMDOutputAsync(input, output, errors);
            //run.RunSynchronously();

            Log.Debug("Finished RunSteamCMD");
        }

        private static async Task HandleSteamCMDOutputAsync(StreamWriter input, StreamReader output, StreamReader errors)
        {
            try
            {
                string received = await output.ReadLineAsync();
                Log.Debug("STEAMCMD_OUTPUT: { SteamCMDOutput }", received);
                if (string.IsNullOrWhiteSpace(received))
                {
                    Log.Debug("Sending quit command to SteamCMD");
                    input.WriteLine("quit");
                    Log.Debug("Sent quit command to SteamCMD");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure occured during SteamCMD Output Handling");
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
            var steamCmdDir = OSDynamic.GetSteamCMDDirectory();
            if (Directory.Exists(steamCmdDir))
            {
                try
                {
                    Log.Warning($"Somehow steamcmd exists when we already checked for it so we'll delete it: {steamCmdDir}");
                    Log.Debug($"Deleting existing steamcmd directory: {steamCmdDir}");
                    Directory.Delete(steamCmdDir, true);
                    Log.Information($"Deleted existing steamcmd directory: {steamCmdDir}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to delete existing steamcmd directory before extraction");
                }
            }
            if (!Directory.Exists(steamCmdDir))
            {
                try
                {
                    Log.Debug($"Creating missing steam directory: {steamCmdDir}");
                    Directory.CreateDirectory(steamCmdDir);
                    Log.Information($"Created missing steam directory: {steamCmdDir}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to create missing steam directory for archive extraction");
                }
            }
            try
            {
                ZipFile.ExtractToDirectory(Path.Combine(Constants.PathCache, "steamcmd.zip"), steamCmdDir);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to extract steamcmd archive");
            }
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
