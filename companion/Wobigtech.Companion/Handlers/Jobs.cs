using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wobigtech.Companion.Local;
using Wobigtech.Core.Comm;
using Wobigtech.Core.Enums;

namespace Wobigtech.Companion.Handlers
{
    public static class Jobs
    {
        public static string GetCronString(CronTime time)
        {
            return time switch
            {
                CronTime.MinFifteen => "*/15 * * * *",
                _ => "*/15 * * * *",
            };
        }
        public static void GameAndModUpdater15Min()
        {
            Log.Debug("Starting GameAndModUpdater15Min()");
            // Moved functionality to webservice, this removes reliance on
            // local storage
        }

        public static void GameServerFileUpdater15Min()
        {
            Log.Debug("Starting GameServerFileUpdater15Min()");
            Housekeeping.ReportFileChanges();
        }
    }
}
