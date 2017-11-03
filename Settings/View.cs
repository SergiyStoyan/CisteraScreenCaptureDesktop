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
        public static readonly ViewSettings View;

        public class ViewSettings : Cliver.Settings
        {            
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
            }
        }
    }
}