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
                running = value;
                //set_hot_keys(value);
                //set_reboot_notificator(value);          
                //UserSessionRoutines.SessionEventHandler = value ? userSessionEventHandler : (UserSessionRoutines.SessionEventDelegate)null;

                if(value)
                    Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                else
                    Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

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
            string user_name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string g = System.Windows.Forms.SystemInformation.UserName;
            string h = System.Windows.Forms.SystemInformation.UserDomainName;
            if (string.IsNullOrWhiteSpace(user_name))
                userLoggedOff();
            else
                userLoggedOn();
        }

        static void userLoggedOn()
        {
            ThreadRoutines.StartTry(async () =>
            {
                string user_name = UserSessionRoutines.GetWindowsUserName();
                if (user_name == null)
                {
                    Log.Error("Session's user name is NULL.");
                    return;
                }

                //IReadOnlyList<IZeroconfHost> results = await ZeroconfResolver.ResolveAsync("_printer._tcp.local.");
                IReadOnlyList<IZeroconfHost> zhs = await ZeroconfResolver.ResolveAsync(Settings.General.ServiceName);
                //IObservable<IZeroconfHost> zhs = ZeroconfResolver.Resolve(Settings.General.ServiceName);
                if (zhs.Count < 1)
                    throw new Exception("Service could not be resolved: " + Settings.General.ServiceName);
                string server_ip = zhs[0].IPAddress;

                HttpClient hc = new HttpClient();
                string url = "http://" + server_ip + "/screenCapture/register?username=" + user_name + "&ipaddress=" + Cliver.NetworkRoutines.GetLocalIpAsString(IPAddress.Parse(server_ip)) + "&port=" + Settings.General.ClientPort;
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
            });
        }

        static void userLoggedOff()
        {
        }

        static void userSessionEventHandler(int session_type)
        {
                switch (session_type)
            {
                case Cliver.Win32.WtsEvents.WTS_SESSION_LOGON:
                    userLoggedOn();
                    break;
                case Cliver.Win32.WtsEvents.WTS_SESSION_LOGOFF:
                    userLoggedOn();
                    break;
            }
        }
    }
}