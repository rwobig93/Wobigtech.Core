using NATS.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Wobigtech.Companion.Local;
using Wobigtech.Core.Comm;
using Wobigtech.Core.Crypto;
using Wobigtech.Core.Enums;

namespace Wobigtech.Companion.Handlers
{
    public static class Events
    {
        public static void CommandListener(object sender, MsgHandlerEventArgs e)
        {
            try
            {
                NatDtoMsg natMsg = NatComm.NatMsgReceive(e.Message.Data);
                Log.Information($"NatComm Received: sub[{e.Message.Subject}] rep[{e.Message.Reply}] msg[{natMsg.NatType}]");
                switch (natMsg.NatType)
                {
                    case NatCommType.CmdReq:
                        NatDtoCmdReq cmdReq = (NatDtoCmdReq)natMsg.NatDto;
                        if (cmdReq.CmdType == CommandType.SteamCMD)
                        {
                            Log.Debug($"CmdType is {cmdReq.CmdType} | Starting SteamCMD Command: steamcmd.exe {cmdReq.Command}");
                            SteamCMD.SteamCMDCommand(cmdReq.Command);
                        }
                        else
                        {
                            Log.Warning($"Hit unexpected CmdType: {cmdReq.CmdType}");
                        }
                        break;
                    case NatCommType.CmdReqBulk:
                        NatDtoCmdReqBulk blkReq = (NatDtoCmdReqBulk)natMsg.NatDto;
                        if (blkReq.CmdType == CommandType.SteamCMD)
                        {
                            Log.Debug($"CmdType is {blkReq.CmdType}, starting command enumeration");
                            foreach (var cmd in blkReq.CommandList)
                            {
                                try
                                {
                                    Log.Debug($"Starting SteamCMD Command: steamcmd.exe {cmd}");
                                    SteamCMD.SteamCMDCommand(cmd);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"Error running a bulk command: {cmd} | {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                                }
                            }
                        }
                        else
                        {
                            Log.Warning($"Hit unexpected CmdType: {blkReq.CmdType}");
                        }
                        break;
                    case NatCommType.CmdResp:
                        NatDtoCmdResp cmdResp = (NatDtoCmdResp)natMsg.NatDto;
                        Log.Error($"Message received was of an unexpected type: [type]{natMsg.NatType} [cmd] {cmdResp.ActionPerformed}");
                        break;
                    case NatCommType.Health:
                        NatDtoTelemetryInfo telem = (NatDtoTelemetryInfo)natMsg.NatDto;
                        Log.Error($"Message received was of an unexpected type: [type]{natMsg.NatType} [cmd] {telem.CompanionID}");
                        break;
                    case NatCommType.JoinReq:
                        NatDtoJoinReq request = (NatDtoJoinReq)natMsg.NatDto;
                        Log.Error($"Message received was of an unexpected type: [type]{natMsg.NatType} [companion]({request.CompanionID}){request.HostName} [ver]{request.VersionNumber}");
                        break;
                    case NatCommType.JoinResp:
                        NatDtoJoinResp response = (NatDtoJoinResp)natMsg.NatDto;
                        Log.Error($"Message received was of an unexpected type: [type]{natMsg.NatType} [hash]{CBase.CalculateHash(response.HomeServerSecret)}");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured during CommandListener: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Log.Debug($@"File changed: [type]{e.ChangeType} [name]{e.Name} [path]{e.FullPath}");
            Housekeeping.ValidateGameWatchChange(e);
        }

        internal static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Log.Debug($@"File Created: [type]{e.ChangeType} [name]{e.Name} [path]{e.FullPath}");
            Housekeeping.ValidateGameWatchCreate(e);
        }

        public static void SteamCMD_Exited(object sender, EventArgs e)
        {
            Log.Information("SteamCMDCLI: Exited");
        }

        public static void SteamCMD_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.Information($"SteamCMDCLI: {e.Data}");
        }

        public static void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Information($"{sender} Download Finished");
        }

        public static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Log.Debug($"{sender} Download Progress: {e.ProgressPercentage}% {e.BytesReceived}\\{e.TotalBytesToReceive}");
        }
        public static void TelemetryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Information("Telemetry worker completed and stopped");
        }

        public static void TelemetryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Debug("Starting TelemetryWorker_DoWork()");
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            Log.Debug("Initialized cpuCounter");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            ramCounter.NextValue();
            Log.Debug("Initialized ramCounter");
            PerformanceCounter uptCounter = new PerformanceCounter("System", "System Up Time");
            uptCounter.NextValue();
            Log.Debug("Initialized uptCounter");
            int progress = 0;
            if (Constants.TelemetryBag == null || Constants.TelemetryBag.Count < 1)
            {
                Log.Debug("telemetryBag is null or empty, initializing and adding object");
                Constants.TelemetryBag = new System.Collections.Concurrent.ConcurrentBag<NatDtoTelemetryInfo>
                {
                    new NatDtoTelemetryInfo()
                    {
                        Cpu = cpuCounter.NextValue(),
                        Ram = ramCounter.NextValue(),
                        Uptime = uptCounter.NextValue()
                    }
                };
                Log.Debug("Finished initializing and adding to telemetryBag");
            }
            Log.Information("Finished initializing system health objects");
            while (Constants.WorkerTelemetryRunning)
            {
                Thread.Sleep(1000);
                Constants.TelemetryBag.First().Cpu = cpuCounter.NextValue();
                Constants.TelemetryBag.First().Ram = ramCounter.NextValue();
                Constants.TelemetryBag.First().Uptime = uptCounter.NextValue();
                if (progress > 60)
                {
                    Log.Debug($"Telemetry Worker Progress is {progress}, resetting");
                    progress = 0;
                }
                progress++;
                Constants.TelemetryWorker.ReportProgress(progress);
            }
            Log.Debug("Left TelemetryWorker_DoWork while loop");
            cpuCounter.Dispose();
            Log.Debug("Disposed cpuCounter");
            ramCounter.Dispose();
            Log.Debug("Disposed ramCounter");
            uptCounter.Dispose();
            Log.Debug("Finished disposing performance counters");
        }

        public static void TelemetryWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Log.Verbose($"Telemetry worker progress changed: {e.ProgressPercentage}");
            if (e.ProgressPercentage <= 61)
            {
                Log.Verbose("Telemetry worker progress value is send home server update");
                if (Constants.NatConn.State != ConnState.CLOSED ||
                    Constants.NatConn.State != ConnState.CONNECTING ||
                    Constants.NatConn.State != ConnState.DISCONNECTED ||
                    Constants.NatConn.State != ConnState.RECONNECTING)
                {
                    NatDtoTelemetryInfo telemDto = new NatDtoTelemetryInfo()
                    {
                        CompanionID = Constants.Config.CompanionID,
                        Cpu = Constants.TelemetryBag.First().Cpu,
                        Ram = Constants.TelemetryBag.First().Ram,
                        Uptime = Constants.TelemetryBag.First().Uptime
                    };
                    Log.Verbose("Sending telemetry");
                    Constants.NatConn.Publish(NatSubjects.Telemetry, NatComm.NatMsgSend(NatCommType.Health, telemDto));
                }
                else
                {
                    Log.Debug($"Not sending telemetry, connection is {Constants.NatConn.State}");
                }
            }
        }

        public static void NatJoinMsgHandler(object sender, MsgHandlerEventArgs e)
        {
            Log.Debug($"JoinMsgRecvd: {e.Message}");
            NatDtoMsg message = NatComm.NatMsgReceive(e.Message.Data);
            if (message.NatType != NatCommType.JoinResp)
            {
                Log.Debug($"Ignoring message, doesn't start w/ the correct response code | rec[{message.NatType}] exp[{NatCommType.JoinResp}]");
            }
            else
            {
                NatDtoJoinResp response = (NatDtoJoinResp)message.NatDto;
                Log.Debug($"Valid message received: [type]{message.NatType}");
                Constants.Config.HomeServerSecret = response.HomeServerSecret;
                Log.Debug($"Home Server Secret Saved: [hash]{CBase.CalculateHash(Constants.Config.HomeServerSecret)}");
                Log.Information("Finished home server join successfully");

                Console.WriteLine($"Successfully joined server {Constants.Config.HomeServerSocket}!");
                Log.Debug("Unsubscribing from JoinListener");
                e.Message.ArrivalSubscription.Unsubscribe();
                Log.Information("Unsubscribed from Join Listener");
            }
        }

    }
}
