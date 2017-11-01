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
using System.Diagnostics;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using GlobalHotKey;

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
                running = true;
                //set_hot_keys(value);
                //set_reboot_notificator(value);
                StateChanged?.Invoke();
            }
            get
            {
                return running;
            }
        }
        static bool running = false;

        static ManualResetEvent stop = new ManualResetEvent(false);

        static void set_reboot_notificator(bool on)
        {
            if (!on)
            {
                if (reboot_notifier_t != null && reboot_notifier_t.IsAlive)
                {
                    stop.Set();
                    if (!reboot_notifier_t.Join(1000))
                        reboot_notifier_t.Abort();
                }
                return;
            }
            if (reboot_notifier_t != null && reboot_notifier_t.IsAlive)
                return;
            stop.Reset();
            //reboot_notifier_t = ThreadRoutines.StartTry(() =>
            //{
            //    DateTime next_notification_time = DateTime.MinValue;
            //    while (true)
            //    {
            //        TimeSpan ut = SystemInfo.GetUpTime();                  
            //        if (next_notification_time < DateTime.Now)
            //        {
            //            next_notification_time = DateTime.Now.AddSeconds(Settings.General.InfoToastLifeTimeInSecs * 2);
            //            InfoWindow.Create(ProgramRoutines.GetAppName(), "It's time to reboot the system...", null, "Reboot", ()=> {
            //                {
            //                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            //                    //StartShutDown("-f -r -t 5");
            //                }
            //            });
            //        }
            //        if (stop.WaitOne(1000))
            //            return;
            //    }
            //});
        }
        static Thread reboot_notifier_t = null;
        
        static void StartShutDown(string param)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Arguments = "/C shutdown " + param;
            Process.Start(psi);
        }

        //static void set_hot_keys(bool listen)
        //{
        //    if (key_manager != null)
        //    {
        //        key_manager.Dispose();
        //        key_manager = null;
        //    }
        //    if (!listen)
        //        return;
            //if (Settings.General.TicketKey != System.Windows.Input.Key.None)
            //{
            //    key_manager = new HotKeyManager();
            //    System.Windows.Input.ModifierKeys mks;
            //    if (Settings.General.TicketModifierKey1 != ModifierKeys.None)
            //    {
            //        mks = Settings.General.TicketModifierKey1;
            //        if (Settings.General.TicketModifierKey2 != ModifierKeys.None)
            //            mks |= Settings.General.TicketModifierKey2;
            //    }
            //    else
            //        mks = ModifierKeys.None;
            //    var hotKey = key_manager.Register(Settings.General.TicketKey, mks);
            //    key_manager.KeyPressed += delegate (object sender, KeyPressedEventArgs e)
            //    {
            //        CreateTicket();
            //    };
            //}
        //}
        //static HotKeyManager key_manager = null;
    }
}