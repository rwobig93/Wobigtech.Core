using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Wobigtech.Core.Tools
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
                basePath = $@"{Path.Combine(userPath, companyName.Company)}";
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
            else if (OperatingSystem.IsMacOS())
            {
                return OSPlatform.OSX;
            }
            else
            {
                return OSPlatform.Linux;
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
