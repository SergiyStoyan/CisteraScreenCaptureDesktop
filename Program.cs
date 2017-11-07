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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net.Mail;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;

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