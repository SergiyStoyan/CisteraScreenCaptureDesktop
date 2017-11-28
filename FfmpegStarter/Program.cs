using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace Cliver.CisteraScreenCapture.ProcessStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            string arguments = args[1];
            bool showMpegWindow = bool.Parse(args[2]);
            string outputLog = args.Length > 3 ? args[3] : null;

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("ffmpeg.exe", arguments)
            {
                ErrorDialog = false,
                UseShellExecute = false,
                CreateNoWindow = !showMpegWindow
                //WindowStyle = ProcessWindowStyle.Hidden;
            };
            if (outputLog != null)
            {
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                TextWriter tw = new StreamWriter(outputLog, true);
                p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                    tw.FlushAsync();
                };
                p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                    tw.FlushAsync();
                };
            }
            p.Start();

            Console.Write(p.Id);
        }
    }
}
