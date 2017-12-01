using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;

namespace Cliver.CisteraScreenCaptureService
{
    public partial class Settings
    {
        [Cliver.Settings.Obligatory]
        public static readonly GeneralSettings General;

        public class GeneralSettings : Cliver.Settings
        {
            public ushort TcpServerPort = 5900;//in general vision TcpServer runs on Client
            public string TcpClientDefaultIp = (new IPAddress(new byte[] {127, 0, 0, 1})).ToString();//in general vision TcpClient runs on Server
            //public ushort TcpClientDefaultPort = 5700;
            public bool Ssl = false;
            public string ServiceDomain = "cistera";
            public string ServiceType = "_cisterascreencapturecontroller._tcp";
            public string CapturedMonitorDeviceName = "";
            public bool ShowMpegWindow = true;
            public bool WriteMpegOutput2Log = false;

            public string GetServiceName()
            {
                return ServiceType.Trim().Trim('.') + "." + ServiceDomain.Trim().Trim('.') + ".";
            }

            //[Newtonsoft.Json.JsonIgnore]
            //public System.Text.Encoding Encoding = System.Text.Encoding.Unicode;

            public override void Loaded()
            {
            }

            public override void Saving()
            {
            }
        }
    }
}