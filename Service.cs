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
                    Log.Inform("Starting...");
                    
                    Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                    string user_name = WindowsUserRoutines.GetUserName();
                    if (!string.IsNullOrWhiteSpace(user_name))
                        userLoggedOn();
                }
                else
                {
                    Log.Inform("Stopping...");

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
            string user_name = WindowsUserRoutines.GetUserName();
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
                    string user_name = WindowsUserRoutines.GetUserName();
                    if (user_name == null)
                    {
                        Log.Error("Session's user name is NULL.");
                        return;
                    }
                    Log.Inform("User logged in: " + user_name);

                    string service = Settings.General.GetServiceName();
                    IReadOnlyList<IZeroconfHost> zhs = await ZeroconfResolver.ResolveAsync(service, TimeSpan.FromSeconds(3), 1, 10);
                    string server_ip;
                    if (zhs.Count < 1)
                    {
                        server_ip = Settings.General.TcpClientDefaultIp.ToString();
                        string m = "Service could not be resolved: " + service + ". Using default ip: " + server_ip;
                        Log.Warning(m);
                        InfoWindow.Create(m, null, "OK", null, Settings.View.ErrorSoundFile, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Yellow);
                    }
                    else
                    {
                        server_ip = zhs[0].IPAddress;
                        Log.Inform("Service: " + service + " has been resolved to: " + server_ip);
                    }

                    TcpServer.Start(Settings.General.TcpServerPort, IPAddress.Parse(server_ip));

                    HttpClient hc = new HttpClient();
                    string url = "http://" + server_ip + "/screenCapture/register?username=" + user_name + "&ipaddress=" + TcpServer.LocalIp + "&port=" + TcpServer.LocalPort;
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