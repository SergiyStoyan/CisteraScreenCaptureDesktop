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
            public ushort ServerPort = 5900;
            public string DefaultServerIp = (new IPAddress(new byte[] {127, 0, 0, 1})).ToString();
            public ushort ClientPort = 5700;
            public bool Ssl = false;
            public string ServiceName = "_cisterascreencapturecontroller._tcp";
            public string CapturedVideoSource = "";
            
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "alert.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;

            //[Newtonsoft.Json.JsonIgnore]
            //public System.Text.Encoding Encoding = System.Text.Encoding.Unicode;

            public override void Loaded()
            {
            }

            public override void Saving()
            {
                //UserEmail = string.IsNullOrWhiteSpace(UserEmail) ? null : UserEmail.Trim();
            }
        }
    }
}