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
using System.Collections.Generic;
using System.Net.Mail;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

/// <summary>
/// TBD:
/// - SSL;
/// - service;
/// - check logins;
/// - add display area;
/// </summary>
namespace Cliver.CisteraScreenCapture
{
    public class Program
    {
        static Program()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception e = (Exception)args.ExceptionObject;
                Message.Error(e);
                Application.Exit();
            };

            Message.TopMost = true;

            Log.Initialize(Log.Mode.ONLY_LOG);
            //Log.Initialize(Log.Mode.ONLY_LOG, Log.CliverSoftCommonDataDir);
            //Config.Initialize(new string[] { "General" });
            Cliver.Config.Reload();

            //ProcessRoutines.CurrentProcessProtection.On = true;
            //SystemEvents.SessionEnding += delegate
            //  {
            //      ProcessRoutines.CurrentProcessProtection.On = false;
            //  };
        }

        //public class CommandLineParameters : ProgramRoutines.CommandLineParameters
        //{
        //    public static readonly CommandLineParameters START = new CommandLineParameters("-start");
        //    public static readonly CommandLineParameters STOP = new CommandLineParameters("-stop");
        //    public static readonly CommandLineParameters EXIT = new CommandLineParameters("-exit");

        //    public CommandLineParameters(string value) : base(value) { }
        //}

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                uint dwSessionId = WinApi.Wts.WTSGetActiveConsoleSessionId();
                MpegStream.Start(dwSessionId, "-f gdigrab -framerate 10 -f rtp_mpegts -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -srtp_out_params aMg7BqN047lFN72szkezmPyN1qSMilYCXbqP/sCt srtp://127.0.0.1:5920");
                //Process mpeg_stream_process;
                //var processId = Win32Process.CreateProcessInConsoleSession("cmd");
                //mpeg_stream_process = Process.GetProcessById((int)processId);
                //ProcessRoutines.AntiZombieTracker.Track(mpeg_stream_process);

                //    try
                //    {
                //        var p = new Process();
                //        p.StartInfo.UseShellExecute = false;
                //        //const string file = "cmd.exe";
                //        const string file = @"psexec.exe";
                //        p.StartInfo.WorkingDirectory = Path.GetDirectoryName(file);
                //        p.StartInfo.FileName = Path.GetFileName(file);
                //        //proc.StartInfo.Domain = "WIN08";
                //        p.StartInfo.Arguments = "-i -d -s cmd";
                //        //p.StartInfo.UserName = "SYSTEM";
                //        //var password = new System.Security.SecureString();
                //        //foreach (var c in "123")
                //        //    password.AppendChar(c);
                //        //p.StartInfo.Password = password;
                //        p.StartInfo.LoadUserProfile = false;
                //        p.Start();
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e);
                //    }

                //MpegStream.Start("-f gdigrab -framerate 10 -f rtp_mpegts -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -srtp_out_params aMg7BqN047lFN72szkezmPyN1qSMilYCXbqP/sCt srtp://127.0.0.1:5920");
                //Thread.Sleep(2000);
                //MpegStream.Stop();

                Log.Main.Inform("Version: " + AssemblyRoutines.GetAppVersion());

                ProcessRoutines.RunSingleProcessOnly();

                Service.Running = true;

                Application.Run(SysTray.This);
            }
            catch (Exception e)
            {
                Message.Error(e);
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}