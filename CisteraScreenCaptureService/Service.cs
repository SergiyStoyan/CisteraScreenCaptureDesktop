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
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using System.Net.Http;
using Zeroconf;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.ServiceProcess;

namespace Cliver.CisteraScreenCaptureService
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Inform("Starting...");
            CisteraScreenCaptureService.Events.Started();
            
            try
            {
                Win32Process.CreateProcessInConsoleSession("cmd", false);
                //var p = new Process();
                //p.StartInfo.UseShellExecute = false;
                //const string file = "cmd.exe";
                ////const string file = @"psexec.exe";
                //p.StartInfo.WorkingDirectory = Path.GetDirectoryName(file);
                //p.StartInfo.FileName = Path.GetFileName(file);
                ////proc.StartInfo.Domain = "WIN08";
                ////p.StartInfo.Arguments = "-i -d -s cmd";
                ////p.StartInfo.UserName = "SYSTEM";
                ////var password = new System.Security.SecureString();
                ////foreach (var c in "123")
                ////    password.AppendChar(c);
                ////p.StartInfo.Password = password;
                //p.StartInfo.LoadUserProfile = false;
                //p.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }





            string user_name = GetUserName();
            //Log.Inform("TEST user by WindowsUserRoutines.GetUserName:" + WindowsUserRoutines.GetUserName());
            //Log.Inform("TEST user by WindowsUserRoutines.GetUserName2:" + WindowsUserRoutines.GetUserName2());
            //Log.Inform("TEST user by WindowsUserRoutines.GetUserName3:" + WindowsUserRoutines.GetUserName3());
            //Log.Inform("TEST user by WindowsUserRoutines.GetUserName4:" + WindowsUserRoutines.GetUserName4());
            if (!string.IsNullOrWhiteSpace(user_name))
                userLoggedOn();
            else
                Log.Warning("No user logged in.");
        }

        protected override void OnStop()
        {
            Log.Inform("Stopping...");
            CisteraScreenCaptureService.Events.Stopped();

            userLoggedOff();
        }

        static Service()
        {
            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public delegate void OnStateChanged();
        public static event OnStateChanged StateChanged = null;
        
        static public string GetUserName()
        {
            return WindowsUserRoutines.GetUserName3();
        }

        private static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                case SessionSwitchReason.SessionUnlock:
                    userLoggedOn();
                    break;
                default:
                    userLoggedOff();
                    break;
            }
        }

        static void userLoggedOn()
        {
            string user_name = GetUserName();
            if (currentUserName == user_name)
                return;
            stop_userLoggedOn_t();
            currentUserName = user_name;
            userLoggedOn_t = ThreadRoutines.StartTry(
                () =>
                {
                    try
                    {
                            //if (SysTray.This.IsOnlyTCP)
                            //{
                            //    Log.Warning("TEST MODE: IsOnlyTCP");
                            //    IPAddress ip1;
                            //    if (!IPAddress.TryParse(Settings.General.TcpClientDefaultIp, out ip1))
                            //        throw new Exception("Server IP is not valid: " + Settings.General.TcpClientDefaultIp);
                            //    TcpServer.Start(Settings.General.TcpServerPort, ip1);
                            //    return;
                            //}

                            if (string.IsNullOrWhiteSpace(user_name))
                        {
                            Log.Error("Session's user name is empty.");
                            return;
                        }
                        Log.Inform("User logged in: " + user_name);

                        string service = Settings.General.GetServiceName();
                        IReadOnlyList<IZeroconfHost> zhs = ZeroconfResolver.ResolveAsync(service, TimeSpan.FromSeconds(3), 1, 10).Result;
                        if (zhs.Count < 1)
                        {
                            currentServerIp = Settings.General.TcpClientDefaultIp;
                            string m = "Service '" + service + "' could not be resolved.\r\nUsing default ip: " + currentServerIp;
                            Log.Warning(m);
                            CisteraScreenCaptureService.Events.UiMessage.Warning(m);
                        }
                        else if (zhs.Where(x => x.IPAddress != null).FirstOrDefault() == null)
                        {
                            currentServerIp = Settings.General.TcpClientDefaultIp;
                            string m = "Resolution of service '" + service + "' has no IP defined.\r\nUsing default ip: " + currentServerIp;
                            Log.Error(m);
                            CisteraScreenCaptureService.Events.UiMessage.Error(m);
                        }
                        else
                        {
                            currentServerIp = zhs.Where(x => x.IPAddress != null).FirstOrDefault().IPAddress;
                            Log.Inform("Service: " + service + " has been resolved to: " + currentServerIp);
                        }

                        IPAddress ip;
                        if (!IPAddress.TryParse(currentServerIp, out ip))
                            throw new Exception("Server IP is not valid: " + currentServerIp);
                        TcpServer.Start(Settings.General.TcpServerPort, ip);

                        string url = "http://" + currentServerIp + "/screenCapture/register?username=" + user_name + "&ipaddress=" + TcpServer.LocalIp + "&port=" + TcpServer.LocalPort;
                        Log.Inform("GETing: " + url);

                        HttpClient hc = new HttpClient();
                        HttpResponseMessage rm = hc.GetAsync(url).Result;
                        if (!rm.IsSuccessStatusCode)
                            throw new Exception(rm.ReasonPhrase);
                        if (rm.Content == null)
                            throw new Exception("Response is empty");
                        string responseContent = rm.Content.ReadAsStringAsync().Result;
                        if (responseContent.Trim() != "OK")
                            throw new Exception("Response: " + responseContent);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        CisteraScreenCaptureService.Events.UiMessage.Error(Log.GetExceptionMessage(e));
                    }
                },
                null,
                () =>
                {
                    userLoggedOn_t = null;
                }
            );
        }
        static Thread userLoggedOn_t = null;
        static string currentUserName;
        public static string UserName
        {
            get
            {
                return currentUserName;
            }
        }
        static string currentServerIp;
        public static string ServerIp
        {
            get
            {
                return currentServerIp;
            }
        }

        static void stop_userLoggedOn_t()
        {
            if (userLoggedOn_t != null)
                while (userLoggedOn_t.IsAlive)
                {
                    userLoggedOn_t.Abort();
                    userLoggedOn_t.Join(200);
                }
        }

        static void userLoggedOff()
        {
            Log.Inform("User logged off");
            stop_userLoggedOn_t();
            TcpServer.Stop();
            MpegStream.Stop();
            currentUserName = null;
        }

        //static void userSessionEventHandler(int session_type)
        //{
        //    switch (session_type)
        //    {
        //        case Cliver.Win32.WtsEvents.WTS_SESSION_LOGON:
        //            userLoggedOn();
        //            break;
        //        case Cliver.Win32.WtsEvents.WTS_SESSION_LOGOFF:
        //            userLoggedOff();
        //            break;
        //    }
        //}
    }
}
