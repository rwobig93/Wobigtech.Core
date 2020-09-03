using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Wobigtech.Companion.Dto;

namespace Wobigtech.Companion.Handlers
{
    public class ThreadRunner
    {
        private static BackgroundWorker GameRunner { get; set; }
        private static List<Action> QueueGameRunner { get; set; }
        private static bool RunningGameRunner = false;

        public static bool SteamCMDRun(RunSteamDto runDto)
        {
            if (GameRunner == null)
            {
                InitializeThreadRunner();
            }
            if (QueueGameRunner == null)
            {
                InitializeQueueGameRunner();
            }

            return true;
        }

        private static void InitializeQueueGameRunner()
        {
            Log.Debug("Running InitializeQueueGameRunner()");
            QueueGameRunner = new List<Action>();
            Log.Information("Initialized new GameRunner Queue");
        }

        private static void InitializeThreadRunner()
        {
            Log.Debug("Running InitializeThreadRunner()");

            RunningGameRunner = true;
            GameRunner = new BackgroundWorker();

            GameRunner.WorkerReportsProgress = true;
            GameRunner.ProgressChanged += GameRunner_ProgressChanged;
            GameRunner.DoWork += GameRunner_DoWork;
            GameRunner.RunWorkerCompleted += GameRunner_RunWorkerCompleted;
            GameRunner.RunWorkerAsync();

            Log.Information("Initialized new GameRunner");
        }

        private static void GameRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Debug("Running GameRunner_DoWork");
            while (RunningGameRunner)
            {
                try
                {
                    if (QueueGameRunner.Count <= 0)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        QueueGameRunner[0]();
                        QueueGameRunner.RemoveAt(0);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failure occured on GameRunner Queue");
                }
            }
            Log.Debug("Finished GameRunner_DoWork");
        }

        private static void GameRunner_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Log.Debug($"GameRunner Progress Change: {e.ProgressPercentage}");
        }

        private static void GameRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Information("GameRunner Thread Finished");
        }
    }
}
