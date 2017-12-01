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

namespace Cliver.CisteraScreenCaptureService
{
    public class MpegStream
    {
        public static void Start(uint sessionId, string arguments)
        {
            if(sessionId < 1)
                throw new Exception("sessionId == " + sessionId);

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
            commandLine = "\"" + Log.AppDir + "\\ffmpeg.exe\" " + arguments;
            
            WinApi.Advapi32.CreationFlags dwCreationFlags = 0;
            if (!Settings.General.ShowMpegWindow)
            {
                dwCreationFlags |= WinApi.Advapi32.CreationFlags.CREATE_NO_WINDOW;
                //startupInfo.dwFlags |= Win32Process.STARTF_USESTDHANDLES;
                //startupInfo.wShowWindow = Win32Process.SW_HIDE;
            }

            WinApi.Advapi32.STARTUPINFO startupInfo = new WinApi.Advapi32.STARTUPINFO();
            if (Settings.General.WriteMpegOutput2Log)
            {
                string file0 = Log.WorkDir + "\\ffmpeg_" + DateTime.Now.ToString("yyMMddHHmmss");
                string file = file0;
                for (int count = 1; File.Exists(file); count++)
                    file = file0 + "_" + count.ToString();
                file += ".log";

                File.WriteAllText(file, @"STARTED AT " + DateTime.Now.ToString() + @":
>" + commandLine + @"

", Encoding.UTF8);
                FileSecurity fileSecurity = File.GetAccessControl(file);
                FileSystemAccessRule fileSystemAccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.AppendData, AccessControlType.Allow);
                fileSecurity.AddAccessRule(fileSystemAccessRule);
                File.SetAccessControl(file, fileSecurity);
                
                commandLine = Environment.SystemDirectory + "\\cmd.exe /c \"" + commandLine + " 1>>\"" + file + "\",2>&1\"";
            }

            uint processId = createProcessInSession(sessionId, commandLine, dwCreationFlags, startupInfo);
            mpeg_stream_process = Process.GetProcessById((int)processId);
            if (mpeg_stream_process == null)
                throw new Exception("Could not find process #" + processId);
            if (mpeg_stream_process.HasExited)
                throw new Exception("Process #" + processId + " exited with code: " + mpeg_stream_process.ExitCode);
            if (antiZombieTracker != null)
                antiZombieTracker.KillTrackedProcesses();
            antiZombieTracker = new ProcessRoutines.AntiZombieTracker();
            antiZombieTracker.Track(mpeg_stream_process);
        }
        static Process mpeg_stream_process = null;
        static string commandLine = null;
        static FileStream fileStream = null;
        static ProcessRoutines.AntiZombieTracker antiZombieTracker = null;
        static uint createProcessInSession(uint dwSessionId, String commandLine, WinApi.Advapi32.CreationFlags dwCreationFlags = 0, WinApi.Advapi32.STARTUPINFO? startupInfo = null, bool bElevate = false)
        {
            Log.Main.Inform("Launching (in session " + dwSessionId + "):\r\n" + commandLine);
            
            IntPtr hNewProcessToken = IntPtr.Zero;
            IntPtr hProcessToken = IntPtr.Zero;
            try
            {                
                WinApi.Advapi32.STARTUPINFO si;
                if (startupInfo != null)
                    si = (WinApi.Advapi32.STARTUPINFO)startupInfo;
                else
                    si = new WinApi.Advapi32.STARTUPINFO();
                si.cb = Marshal.SizeOf(si);
                si.lpDesktop = "winsta0\\default";

                if (!WinApi.Advapi32.OpenProcessToken(Process.GetCurrentProcess().Handle, WinApi.Advapi32.DesiredAccess.MAXIMUM_ALLOWED, out hProcessToken))
                    throw new Exception("!OpenProcessToken. " + ErrorRoutines.GetLastError());
                
                var sa = new WinApi.Advapi32.SECURITY_ATTRIBUTES();
                sa.Length = Marshal.SizeOf(sa);
                if (!WinApi.Advapi32.DuplicateTokenEx(hProcessToken, WinApi.Advapi32.DesiredAccess.MAXIMUM_ALLOWED, ref sa, WinApi.Advapi32.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, WinApi.Advapi32.TOKEN_TYPE.TokenPrimary, ref hNewProcessToken))
                    throw new Exception("!DuplicateTokenEx. " + ErrorRoutines.GetLastError());

                if (!WinApi.Advapi32.SetTokenInformation(hNewProcessToken, WinApi.Advapi32.TOKEN_INFORMATION_CLASS.TokenSessionId, ref dwSessionId, (uint)IntPtr.Size))
                    throw new Exception("!SetTokenInformation. " + ErrorRoutines.GetLastError());
             
                WinApi.Advapi32.PROCESS_INFORMATION pi;
                if (!WinApi.Advapi32.CreateProcessAsUser(hNewProcessToken, // client's access token
                    null, // file to execute
                    commandLine, // command line
                    ref sa, // pointer to process SECURITY_ATTRIBUTES
                    ref sa, // pointer to thread SECURITY_ATTRIBUTES
                    false, // handles are not inheritable
                    dwCreationFlags, // creation flags
                    IntPtr.Zero,//pEnv, // pointer to new environment block 
                    null, // name of current directory 
                    ref si, // pointer to STARTUPINFO structure
                    out pi // receives information about new process
                    ))
                    throw new Exception("!CreateProcessAsUser. " + ErrorRoutines.GetLastError());
                return pi.dwProcessId;
            }
            //catch(Exception e)
            //{

            //}
            finally
            {
                if (hProcessToken != IntPtr.Zero)
                    WinApi.Kernel32.CloseHandle(hProcessToken);
                if (hNewProcessToken != IntPtr.Zero)
                    WinApi.Kernel32.CloseHandle(hNewProcessToken);
            }
        }

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
            if (antiZombieTracker != null)
            {
                antiZombieTracker.KillTrackedProcesses();
                antiZombieTracker = null;
            }
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