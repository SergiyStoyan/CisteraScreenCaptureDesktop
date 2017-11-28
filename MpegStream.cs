////********************************************************************************************
////Author: Sergey Stoyan, CliverSoft.com
////        http://cliversoft.com
////        stoyan@cliversoft.com
////        sergey.stoyan@gmail.com
////        27 February 2007
////Copyright: (C) 2007, Sergey Stoyan
////********************************************************************************************

//using System;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.IO;
//using System.Threading;
//using System.Text.RegularExpressions;
//using System.Web;
////using System.Web.Script.Serialization;
//using System.Collections.Generic;
//using Cliver;
//using System.Configuration;
//using System.Windows.Forms;
//using Microsoft.Win32;
//using System.Windows.Input;
//using System.Net.Http;
//using Zeroconf;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Security.AccessControl;
//using System.Security.Principal;

//namespace Cliver.CisteraScreenCapture
//{
//    public class MpegStream
//    {
//        static MpegStream()
//        {
//        }

//        public static void Start(string arguments)
//        {
//            if (mpeg_stream_process != null)
//                Log.Warning("The previous MpegStream was not stopped!");
//            Stop();

//            int x = 0, y = 0, w = 0, h = 0;
//            Win32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32.RECT lprcMonitor, IntPtr dwData) =>
//            {
//                Win32.MONITORINFOEX mi = new Win32.MONITORINFOEX();
//                mi.Size = Marshal.SizeOf(mi.GetType());
//                if (Win32.GetMonitorInfo(hMonitor, ref mi) && mi.DeviceName != Settings.General.CapturedMonitorDeviceName)
//                {
//                    x = lprcMonitor.Left;
//                    y = lprcMonitor.Top;
//                    w = lprcMonitor.Right - lprcMonitor.Left;
//                    h = lprcMonitor.Bottom - lprcMonitor.Top;
//                    return false;
//                }
//                return true;
//            };
//            Win32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
//            string source = " -offset_x " + x + " -offset_y " + y + " -video_size " + w + "x" + h + " -show_region 1 -i desktop ";
//            arguments = Regex.Replace(arguments, @"-framerate\s+\d+", "$0" + source);
//            commandLine = "ffmpeg.exe " + arguments;

//            Log.Inform("Launching:\r\n" + commandLine);

//            uint dwCreationFlags = 0;
//            if (!Settings.General.ShowMpegWindow)
//            {
//                dwCreationFlags |= Win32Process.dwCreationFlagValues.CREATE_NO_WINDOW;
//                //startupInfo.dwFlags |= Win32Process.STARTF_USESTDHANDLES;
//                //startupInfo.wShowWindow = Win32Process.SW_HIDE;
//            }
//            starter_p.StartInfo = new ProcessStartInfo("ProcessStarter.exe")
//            {
//                ErrorDialog = false,
//                UseShellExecute = false,
//                CreateNoWindow = true
//                //WindowStyle = ProcessWindowStyle.Hidden;
//            };
//            starter_p.StartInfo.Arguments = "\"" + arguments + "\"" + " \"" + Settings.General.ShowMpegWindow + " \"";
//            if (Settings.General.WriteMpegOutput2Log)
//            {
//                string file0 = Log.WorkDir + "\\ffmpeg_" + DateTime.Now.ToString("yyMMddHHmmss");
//                string file = file0;
//                for (int count = 1; File.Exists(file); count++)
//                    file = file0 + "_" + count.ToString();
//                file += ".log";

//                File.WriteAllText(file, @"STARTED: " + DateTime.Now.ToString() + @"
//>" + commandLine + @"

//");

//                FileSecurity fSecurity = File.GetAccessControl(file);
//                fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.AppendData, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
//                File.SetAccessControl(file, fSecurity);

//                starter_p.StartInfo.Arguments += " \"" + file + "\"";
//            }
//            starter_p.Start();
//            starter_p.WaitForExit();
//            string e = starter_p.StandardError.ReadToEnd();
//            if (!string.IsNullOrWhiteSpace(e))
//                throw new Exception(e);
//            string o = starter_p.StandardOutput.ReadToEnd();
//            int processId = int.Parse(o);
//            mpeg_stream_process = Process.GetProcessById(processId);
//            ProcessRoutines.AntiZombieTracker.This.Track(mpeg_stream_process);
//        }
//        static Process mpeg_stream_process = null;
//        static string commandLine = null;

//        public static void Stop()
//        {
//            if (mpeg_stream_process != null)
//            {
//                Log.Inform("Terminating:\r\n" + commandLine);
//                ProcessRoutines.KillProcessTree(mpeg_stream_process.Id);
//                mpeg_stream_process = null;
//            }
//            ProcessRoutines.AntiZombieTracker.This.KillTrackedProcesses();//to close the job object
//            commandLine = null;
//        }

//        public static bool Running
//        {
//            get
//            {
//                return mpeg_stream_process != null;
//            }
//        }

//        public static string CommandLine
//        {
//            get
//            {
//                return commandLine;
//            }
//        }
//    }
//}