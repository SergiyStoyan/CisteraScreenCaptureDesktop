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
    public class MpegStream
    {
        static MpegStream()
        {
        }

        public static bool Running
        {
            get
            {
                return mpeg_stream_process != null; 
            }
        }

        public static void Start(string arguments)
        {
            if (mpeg_stream_process != null)
                mpeg_stream_process.Kill();
            mpeg_stream_process = new Process();
            mpeg_stream_process.StartInfo = new ProcessStartInfo("ffmpeg", arguments);
        }
        static Process mpeg_stream_process = null;

        public  static void Stop()
        {
            if (mpeg_stream_process != null)
                mpeg_stream_process.Kill();
            mpeg_stream_process = null;
        }
    }
}