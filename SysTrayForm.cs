//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Cliver.CisteraScreenCapture
{
    public partial class SysTray : Form 
    {
        SysTray()
        {
            InitializeComponent();

            icon = this.Icon.ToBitmap();
            Service.StateChanged += delegate
            {
                if (!IsHandleCreated)
                    CreateHandle();
                this.Invoke(() => { StartStop.Checked = Service.Running; });
                if (Service.Running)
                    notifyIcon.Icon = Icon.FromHandle(icon.GetHicon());
                else
                    notifyIcon.Icon = Icon.FromHandle(ImageRoutines.GetGreyScale(icon).GetHicon());
            };
        }
        readonly Bitmap icon;

        public static readonly SysTray This = new SysTray();

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            settingsToolStripMenuItem_Click(null, null);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow.Open();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm.Open();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Program.Exit();
            Environment.Exit(0);
        }

        private void SysTray_VisibleChanged(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void notifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void StartStop_CheckedChanged(object sender, EventArgs e)
        {
            Service.Running = StartStop.Checked;
        }

        private void workDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Log.WorkDir);
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
        }     
    }
}
