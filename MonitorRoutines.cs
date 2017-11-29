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
            Win32Monitor.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32Monitor.RECT lprcMonitor, IntPtr dwData) =>
            {
                Win32Monitor.MONITORINFOEX mi = new Win32Monitor.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (!Win32Monitor.GetMonitorInfo(hMonitor, ref mi))
                    return true;
                last_mn = mi.DeviceName;
                if (mi.Monitor.Left == 0 && mi.Monitor.Top == 0)
                {
                    default_mn = mi.DeviceName;
                    return false;
                }
                return true;
            };
            Win32Monitor.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return default_mn != null ? default_mn : last_mn;
        }

        public static Win32Monitor.RECT? GetMonitorAreaByMonitorName(string name)
        {
            Win32Monitor.RECT? a = null;
            Win32Monitor.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32Monitor.RECT lprcMonitor, IntPtr dwData) =>
            {
                Win32Monitor.MONITORINFOEX mi = new Win32Monitor.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (Win32Monitor.GetMonitorInfo(hMonitor, ref mi) && mi.DeviceName == name)
                {
                    a = mi.Monitor;
                    return false;
                }
                return true;
            };
            Win32Monitor.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return a;
        }

        public static List<MonitorInfo> GetMonitorInfos()
        {
            List<MonitorInfo> mis = new List<MonitorInfo>(); 
            Win32Monitor.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32Monitor.RECT lprcMonitor, IntPtr dwData) =>
            {
                Win32Monitor.MONITORINFOEX mi = new Win32Monitor.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (!Win32Monitor.GetMonitorInfo(hMonitor, ref mi))
                    return true;
                Win32Monitor.DISPLAY_DEVICE dd = new Win32Monitor.DISPLAY_DEVICE();
                dd.cb = Marshal.SizeOf(dd.GetType());
                Win32Monitor.EnumDisplayDevices(mi.DeviceName, 0, ref dd, 0);
                mis.Add(new MonitorInfo()
                {
                    DeviceString = dd.DeviceString,
                    DeviceName = mi.DeviceName,
                    Area = mi.Monitor,
                });
                return true;
            };
            Win32Monitor.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return mis;
        }
        public class MonitorInfo
        {
            public string DeviceString;
            public string DeviceName;
            public Win32Monitor.RECT Area;
        }
    }
}