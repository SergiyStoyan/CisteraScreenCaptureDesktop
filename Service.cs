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
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using GlobalHotKey;
using System.Net.Http;
using Zeroconf;
using System.Diagnostics;

namespace Cliver.CisteraScreenCapture
{
    public class Service
    {
        static Service()
        {
        }

        public delegate void OnStateChanged();
        public static event OnStateChanged StateChanged = null;

        public static bool Running
        {
            set
            {
                if (running == value)
                    return;
                running = value;
                //UserSessionRoutines.SessionEventHandler = value ? userSessionEventHandler : (UserSessionRoutines.SessionEventDelegate)null;

                if (value)
                {
                    //SystemEvents_SessionSwitch(null, null);
                    //TcpServer.Start(Settings.General.TcpServerPort);
                    Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                    string user_name = SystemRoutines.GetWindowsUserName();
                    if (!string.IsNullOrWhiteSpace(user_name))
                        userLoggedOn();
                }
                else
                {
                    userLoggedOff();
                    Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                }

                StateChanged?.Invoke();
            }
            get
            {
                return running;
            }
        }
        static bool running = false;

        private static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            //string user_name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string user_name = SystemRoutines.GetWindowsUserName();
            if (!string.IsNullOrWhiteSpace(user_name))
                userLoggedOn();
            else
                userLoggedOff();
        }

        static void userLoggedOn()
        {
            ThreadRoutines.StartTry(async () =>
            {
                try
                {
                    string user_name = SystemRoutines.GetWindowsUserName();
                    if (user_name == null)
                    {
                        Log.Error("Session's user name is NULL.");
                        return;
                    }
                    Log.Inform("User logged in: " + user_name);

                    //IReadOnlyList<IZeroconfHost> results = await ZeroconfResolver.ResolveAsync("_printer._tcp.local.");
                    IReadOnlyList<IZeroconfHost> zhs = await ZeroconfResolver.ResolveAsync(Settings.General.ServiceName);
                    //IObservable<IZeroconfHost> zhs = ZeroconfResolver.Resolve(Settings.General.ServiceName);                    
                    string server_ip;
                    if (zhs.Count < 1)
                    {
                        string m = "Service could not be resolved: " + Settings.General.ServiceName + ". Using default ip: " + Settings.General.DefaultTcpClientIp;
                        Log.Warning(m);
                        InfoWindow.Create(m, null, "OK", null, Settings.View.ErrorSoundFile, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Yellow);
                        server_ip = Settings.General.DefaultTcpClientIp.ToString();
                    }
                    else
                        server_ip = zhs[0].IPAddress;

                    TcpServer.Start(IPAddress.Parse(server_ip), Settings.General.TcpServerPort);

                    HttpClient hc = new HttpClient();
                    string url = "http://" + server_ip + "/screenCapture/register?username=" + user_name + "&ipaddress=" + TcpServer.Ip + "&port=" + Settings.General.TcpServerPort;
                    //string url = "http://google.com";
                    Log.Inform("GETing: " + url);
                    HttpResponseMessage rm = await hc.GetAsync(url);
                    if (!rm.IsSuccessStatusCode)
                        throw new Exception(rm.ReasonPhrase);
                    if (rm.Content != null)
                    {
                        string responseContent = await rm.Content.ReadAsStringAsync();
                        if (responseContent.Trim() != "OK")
                            throw new Exception("Response: " + responseContent);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    InfoWindow.Create(Log.GetExceptionMessage(e), null, "OK", null, Settings.View.ErrorSoundFile, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
                }
            });
        }

        static void userLoggedOff()
        {
            Log.Inform("User logged off");
            TcpServer.Stop();
            MpegStream.Stop();
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