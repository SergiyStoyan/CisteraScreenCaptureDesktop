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
using System.Security.AccessControl;
using System.Security.Principal;

namespace Cliver.CisteraScreenCapture
{
    public class MpegStream2
    {
        public static void Start(uint sessionId, string arguments)
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
            Win32Monitor.RECT? an = MonitorRoutines.GetMonitorAreaByMonitorName(Settings.General.CapturedMonitorDeviceName);
            if (an == null)
            {
                Settings.General.CapturedMonitorDeviceName = MonitorRoutines.GetDefaultMonitorName();
                Log.Main.Warning("Monitor '" + Settings.General.CapturedMonitorDeviceName + "' was not found. Using default one '" + Settings.General.CapturedMonitorDeviceName + "'");
                an = MonitorRoutines.GetMonitorAreaByMonitorName(Settings.General.CapturedMonitorDeviceName);
                if (an == null)
                    throw new Exception("Monitor '" + Settings.General.CapturedMonitorDeviceName + "' was not found.");
            }
            Win32Monitor.RECT a = (Win32Monitor.RECT)an;
            string source = " -offset_x " + a.Left + " -offset_y " + a.Top + " -video_size " + (a.Right - a.Left) + "x" + (a.Bottom - a.Top) + " -show_region 1 -i desktop ";

            arguments = Regex.Replace(arguments, @"-framerate\s+\d+", "$0" + source);
            commandLine = "ffmpeg.exe " + arguments;

            Log.Main.Inform("Launching:\r\n" + commandLine);

            uint dwCreationFlags = 0;
            if (!Settings.General.ShowMpegWindow)
            {
                dwCreationFlags |= Win32Process.dwCreationFlagValues.CREATE_NO_WINDOW;
                //startupInfo.dwFlags |= Win32Process.STARTF_USESTDHANDLES;
                //startupInfo.wShowWindow = Win32Process.SW_HIDE;
            }

            Win32Process3.STARTUPINFO startupInfo = new Win32Process3.STARTUPINFO();
            if (Settings.General.WriteMpegOutput2Log)
            {
                string file0 = Log.WorkDir + "\\ffmpeg_" + DateTime.Now.ToString("yyMMddHHmmss");
                string file = file0;
                for (int count = 1; File.Exists(file); count++)
                    file = file0 + "_" + count.ToString();
                file += ".log";

                fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write);
                FileSecurity fileSecurity = File.GetAccessControl(file);
                FileSystemAccessRule fileSystemAccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.AppendData, AccessControlType.Allow);
                fileSecurity.AddAccessRule(fileSystemAccessRule);
                File.SetAccessControl(file, fileSecurity);
                //fileStream.SetAccessControl(fileSecurity);

                string s = @"STARTED: " + DateTime.Now.ToString() + @"
>" + commandLine + @"

";
                byte[] bs = Encoding.UTF8.GetBytes(s);
                fileStream.Write(bs, 0, bs.Length);
                fileStream.Flush();

                //Win32File.SECURITY_ATTRIBUTES sa = new Win32File.SECURITY_ATTRIBUTES();
                //sa.nLength = Marshal.SizeOf(sa);
                //sa.lpSecurityDescriptor = IntPtr.Zero;
                //sa.bInheritHandle = true;
                //Microsoft.Win32.SafeHandles.SafeFileHandle sfh = Win32File.CreateFile(file, Win32File.dwDesiredAccess.GENERIC_WRITE, Win32File.dwShareMode.FILE_SHARE_WRITE | Win32File.dwShareMode.FILE_SHARE_READ, ref sa, Win32File.dwCreationDisposition.CREATE_ALWAYS, Win32File.dwFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                IntPtr fh = fileStream.SafeFileHandle.DangerousGetHandle();
                //IntPtr fh = sfh.DangerousGetHandle();
                startupInfo.hStdError = fh;
                startupInfo.hStdOutput = fh;
                startupInfo.dwFlags |= Win32Process.STARTF_USESTDHANDLES;
            }            
            //uint dwSessionId = Win32Process3.WTSGetActiveConsoleSessionId();
            //string active_user = Win32Process3.GetUsernameBySessionId(dwSessionId);
            //if (sessionUserName != active_user)
            //    throw new Exception("Active session user's name: '" + active_user + "' != '" + sessionUserName + "'");
            //uint processId = Win32Process.CreateProcessInConsoleSession("C:\\Windows\\System32\\cmd.exe /c " + commandLine, dwCreationFlags, startupInfo);
            uint processId = Win32Process3.CreateProcessInSession(sessionId, commandLine, dwCreationFlags, startupInfo);
            mpeg_stream_process = Process.GetProcessById((int)processId);
            ProcessRoutines.AntiZombieTracker.This.Track(mpeg_stream_process);
        }
        static Process mpeg_stream_process = null;
        static string commandLine = null;
        static FileStream fileStream = null;

        public static void Stop()
        {
            if (mpeg_stream_process != null)
            {
                Log.Main.Inform("Terminating:\r\n" + commandLine);
                ProcessRoutines.KillProcessTree(mpeg_stream_process.Id);
                mpeg_stream_process = null;
            }
            if (fileStream != null)
            {
                fileStream.Flush();
                fileStream.Dispose();
                fileStream = null;
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