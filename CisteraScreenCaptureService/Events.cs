using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Cliver.CisteraScreenCaptureService
{
    static class Events
    {
        internal static class UiMessage
        {
            static internal void Info(string m)
            {
            }

            static internal void Warning(string m)
            {
            }

            static internal void Error(string m)
            {

            }
        }

        static internal void Started()
        {
        }

        static internal void Stopped()
        {
        }
    }
}
