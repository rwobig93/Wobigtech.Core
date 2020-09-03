using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Wobigtech.Companion.Local;

namespace Wobigtech.Companion
{
    public static class OSDynamic
    {
        public static class OperatingSystem
        {
            public static bool IsWindows() =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            public static bool IsMacOS() =>
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            public static bool IsLinux() =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static string GetStoragePath()
        {
            string basePath = "";
            bool isWindows = OperatingSystem.IsWindows();
            if (isWindows)
            {
                var userPath = Environment.GetEnvironmentVariable(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                    "LOCALAPPDATA" : "Home");
                var assy = System.Reflection.Assembly.GetEntryAssembly();
                var companyName = assy.GetCustomAttributes<AssemblyCompanyAttribute>()
                  .FirstOrDefault();
                var productName = assy.GetCustomAttribute<AssemblyProductAttribute>().Product;
                basePath = $@"{Path.Combine(userPath, companyName.Company)}\{productName}";
            }
            return basePath;
        }

        internal static string GetLoggingPath()
        {
            return Path.Combine(GetStoragePath(), "Logs");
        }

        public static OSPlatform GetCurrentOS()
        {
            if (OperatingSystem.IsWindows())
            {
                return OSPlatform.Windows;
            }
            else if (OperatingSystem.IsLinux())
            {
                return OSPlatform.Linux;
            }
            else if (OperatingSystem.IsMacOS())
            {
                return OSPlatform.OSX;
            }
            else
            {
                return OSPlatform.FreeBSD;
            }
        }

        public static string GetCurrentDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                return Directory.GetCurrentDirectory();
            }
            else
            {
                return Directory.GetCurrentDirectory();
            }
        }

        internal static string GetConfigPath()
        {
            return Path.Combine(GetStoragePath(), "Config");
        }

        internal static bool DirectoryExists(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Debug($"OSDynamic.DirectoryExists Failure: {ex.Message}");
                return false;
            }
        }

        internal static string GetDefaultGameServerPath()
        {
            return Path.Combine(GetStoragePath(), "GameServers");
        }

        internal static void RecursiveFileScan(string path)
        {
            Log.Verbose($"Starting RecursiveFileScan({path})");
            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    Housekeeping.ParseGameServerFiles(file);
                }
                catch (Exception ex)
                {
                    Log.Debug($"RecursiveFileScan Failure: [file]{ex.Message}");
                }
            }
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                try
                {
                    RecursiveFileScan(dir);
                }
                catch (Exception ex)
                {
                    Log.Debug($"RecursiveFileScan Failure: [dir]{ex.Message}");
                }
            }
        }

        internal static void MoveToAppDirectory()
        {
            Log.Debug("Starting MoveToAppDirectory()");
            string storagePath = GetStoragePath();
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = storagePath
            });
            Log.Information($"Opened explorer.exe to: {storagePath}");
        }

        internal static string GetSteamCMDPath()
        {
            return Path.Combine(Constants.PathSourceFolder, "steamcmd\\steamcmd.exe");
        }

        internal static string GetSteamCMDDirectory()
        {
            return Path.Combine(Constants.PathSourceFolder, "steamcmd");
        }

        internal static bool FileExists(string filePath)
        {
            if (OperatingSystem.IsWindows())
            {
                return File.Exists(filePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
