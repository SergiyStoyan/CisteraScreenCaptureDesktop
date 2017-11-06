using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Net;

namespace Cliver.CisteraScreenCapture
{
    public partial class Settings
    {
        [Cliver.Settings.Obligatory]
        public static readonly GeneralSettings General;

        public class GeneralSettings : Cliver.Settings
        {
            public ushort TcpServerPort = 5900;//in general design TcpServer runs on Client
            public string DefaultTcpClientIp = (new IPAddress(new byte[] {127, 0, 0, 1})).ToString();
            public ushort TcpClientPort = 5700;//in general design TcpClient runs on Server
            public bool Ssl = false;
            public string ServiceName = "_cisterascreencapturecontroller._tcp";
            public string CapturedMonitorDeviceName = "";
            public bool ShowMpegWindow = false;
            public bool WriteMpegOutput2Log = false;

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