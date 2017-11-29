using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cliver.CisteraScreenCapture
{
    public class UserSessionRoutines
    {
        public delegate void SessionEventDelegate(int session_type);
        static public SessionEventDelegate SessionEventHandler
        {
            get
            {
                return sessionEventHandler;
            }
            set
            {
                sessionEventHandler = value;
                if (value != null)
                {
                    if (f == null)
                        f = new WtsEventsListeningForm();
                    return;
                }
                if (f == null)
                    return;
                f.Dispose();
            }
        }
        static SessionEventDelegate sessionEventHandler = null;
        static WtsEventsListeningForm f = null;

        class WtsEventsListeningForm : Form
        {
            internal WtsEventsListeningForm()
            {
                //InitializeComponent();
                if (!WinApi.Wts.WTSRegisterSessionNotification(this.Handle, WinApi.Wts.WTSRegisterSessionNotificationFlags.NOTIFY_FOR_ALL_SESSIONS))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                if (!WinApi.Wts.WTSUnRegisterSessionNotification(this.Handle))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                base.OnClosing(e);
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == Cliver.WinApi.Messages.WM_WTSSESSION_CHANGE)
                    //SessionEventHandler?.BeginInvoke(m.WParam.ToInt32(), null, null);
                    SessionEventHandler?.Invoke(m.WParam.ToInt32());
                base.WndProc(ref m);
            }
        }
    }
}