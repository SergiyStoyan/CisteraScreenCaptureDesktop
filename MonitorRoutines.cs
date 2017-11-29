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
using System.Drawing;

namespace Cliver.CisteraScreenCapture
{
    public static class MonitorRoutines
    {
        public static string GetDefaultMonitorName()
        {
            string last_mn = null;
            string default_mn = null;
            WinApi.User32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref WinApi.User32.RECT lprcMonitor, IntPtr dwData) =>
            {
                WinApi.User32.MONITORINFOEX mi = new WinApi.User32.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (!WinApi.User32.GetMonitorInfo(hMonitor, ref mi))
                    return true;
                last_mn = mi.DeviceName;
                if (mi.Monitor.Left == 0 && mi.Monitor.Top == 0)
                {
                    default_mn = mi.DeviceName;
                    return false;
                }
                return true;
            };
            WinApi.User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return default_mn != null ? default_mn : last_mn;
        }

        public static WinApi.User32.RECT? GetMonitorAreaByMonitorName(string name)
        {
            WinApi.User32.RECT? a = null;
            WinApi.User32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref WinApi.User32.RECT lprcMonitor, IntPtr dwData) =>
            {
                WinApi.User32.MONITORINFOEX mi = new WinApi.User32.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (WinApi.User32.GetMonitorInfo(hMonitor, ref mi) && mi.DeviceName == name)
                {
                    a = mi.Monitor;
                    return false;
                }
                return true;
            };
            WinApi.User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return a;
        }

        public static List<MonitorInfo> GetMonitorInfos()
        {
            List<MonitorInfo> mis = new List<MonitorInfo>(); 
            WinApi.User32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref WinApi.User32.RECT lprcMonitor, IntPtr dwData) =>
            {
                WinApi.User32.MONITORINFOEX mi = new WinApi.User32.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (!WinApi.User32.GetMonitorInfo(hMonitor, ref mi))
                    return true;
                WinApi.User32.DISPLAY_DEVICE dd = new WinApi.User32.DISPLAY_DEVICE();
                dd.cb = Marshal.SizeOf(dd.GetType());
                WinApi.User32.EnumDisplayDevices(mi.DeviceName, 0, ref dd, 0);
                mis.Add(new MonitorInfo()
                {
                    DeviceString = dd.DeviceString,
                    DeviceName = mi.DeviceName,
                    Area = mi.Monitor,
                });
                return true;
            };
            WinApi.User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return mis;
        }
        public class MonitorInfo
        {
            public string DeviceString;
            public string DeviceName;
            public WinApi.User32.RECT Area;
        }
    }
}