//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
//using System.Web.Script.Serialization;
using System.Collections.Generic;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using System.Net.Http;
using Zeroconf;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cliver.CisteraScreenCaptureService
{
    public class MpegStream
    {
        public static void Start(string arguments)
        {
            if (mpeg_stream_process != null)
                Log.Main.Warning("The previous MpegStream was not stopped!");
            Stop();

            if (string.IsNullOrWhiteSpace(Settings.General.CapturedMonitorDeviceName))
            {
                Settings.General.CapturedMonitorDeviceName = MonitorRoutines.GetDefaultMonitorName();
                if (string.IsNullOrWhiteSpace(Settings.General.CapturedMonitorDeviceName))
                    throw new Exception("No monitor was found.");
            }
            WinApi.User32.RECT? an = MonitorRoutines.GetMonitorAreaByMonitorName(Settings.General.CapturedMonitorDeviceName);
            if (an == null)
            {
                Settings.General.CapturedMonitorDeviceName = MonitorRoutines.GetDefaultMonitorName();
                Log.Main.Warning("Monitor '" + Settings.General.CapturedMonitorDeviceName + "' was not found. Using default one '" + Settings.General.CapturedMonitorDeviceName + "'");
                an = MonitorRoutines.GetMonitorAreaByMonitorName(Settings.General.CapturedMonitorDeviceName);
                if (an == null)
                    throw new Exception("Monitor '" + Settings.General.CapturedMonitorDeviceName + "' was not found.");
            }
            WinApi.User32.RECT a = (WinApi.User32.RECT)an;
            string source = " -offset_x " + a.Left + " -offset_y " + a.Top + " -video_size " + (a.Right - a.Left) + "x" + (a.Bottom - a.Top) + " -show_region 1 -i desktop ";

            arguments = Regex.Replace(arguments, @"-framerate\s+\d+", "$0" + source);
            commandLine = "ffmpeg " + arguments;

            Log.Main.Inform("Launching:\r\n" + commandLine);

            mpeg_stream_process = new Process();
            mpeg_stream_process.StartInfo = new ProcessStartInfo("ffmpeg.exe", arguments)
            {
                ErrorDialog = false,
                UseShellExecute = false,
                CreateNoWindow = !Settings.General.ShowMpegWindow
                //WindowStyle = ProcessWindowStyle.Hidden;
            };
            if (Settings.General.WriteMpegOutput2Log)
            {
                mpeg_stream_process.StartInfo.RedirectStandardOutput = true;
                mpeg_stream_process.StartInfo.RedirectStandardError = true;

                string file0 = Log.WorkDir + "\\ffmpeg_" + DateTime.Now.ToString("yyMMddHHmmss");
                string file = file0;
                for (int count = 1; File.Exists(file); count++)
                    file = file0 + "_" + count.ToString();
                TextWriter tw = new StreamWriter(file, false);
                tw.WriteLine("STARTED: " + DateTime.Now.ToString());
                tw.WriteLine(">" + commandLine);
                tw.WriteLine("\r\n");
                tw.FlushAsync();
                mpeg_stream_process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                    tw.FlushAsync();
                };
                mpeg_stream_process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                    tw.FlushAsync();
                };
            }
            mpeg_stream_process.Start();
            ProcessRoutines.AntiZombieTracker.This.Track(mpeg_stream_process);
        }
        static Process mpeg_stream_process = null;
        static string commandLine = null;

        public static void Stop()
        {
            if (mpeg_stream_process != null)
            {
                Log.Main.Inform("Terminating:\r\n" + commandLine);
                ProcessRoutines.KillProcessTree(mpeg_stream_process.Id);
                mpeg_stream_process = null;
            }
            ProcessRoutines.AntiZombieTracker.This.KillTrackedProcesses();//to close the job object
            commandLine = null;
        }

        public static bool Running
        {
            get
            {
                return mpeg_stream_process != null;
            }
        }

        public static string CommandLine
        {
            get
            {
                return commandLine;
            }
        }
    }
}