using Wobigtech.Core.Enums;

namespace Wobigtech.Companion
{
    class Program
    {
        static void Main(string[] args)
        {
            Function.InitializeLogger();

            Function.ParseLaunchArgs(args);

            if (Function.LoadConfig() == StatusReturn.NotFound)
            {
                Function.InitializeFirstRun();
            }

            Function.TestMenu();

            Function.TriggerSettingsByConfig();

            Function.DisplayHostInfo();

            Function.SetupConnection();

            Function.StartCompanionWork();

            Function.PresentMenu();

            Function.Cleanup();
        }
    }
}
