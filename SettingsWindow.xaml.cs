using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cliver.CisteraScreenCapture
{
    public partial class SettingsWindow : Window
    {
        SettingsWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            Icon = AssemblyRoutines.GetAppIconImageSource();

            ContentRendered += delegate
            {
                this.MinHeight = this.ActualHeight;
                this.MaxHeight = this.ActualHeight;
                this.MinWidth = this.ActualWidth;
            };

            IsVisibleChanged += (object sender, DependencyPropertyChangedEventArgs e) =>
            {
                if (Visibility == Visibility.Visible)
                {
                    DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                    this.BeginAnimation(UIElement.OpacityProperty, da);
                }
            };

            Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                if (Opacity > 0)
                {
                    e.Cancel = true;
                    DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    da.Completed += delegate { Close(); };
                    this.BeginAnimation(UIElement.OpacityProperty, da);
                }
            };

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //DefaultServerIp.ValueDataType = typeof(IPAddress);

            set();
        }

        void set()
        { 
            ServerPort.Text = Settings.General.TcpClientPort.ToString();
            DefaultServerIp.Text = Settings.General.DefaultTcpClientIp.ToString();
            ClientPort.Text = Settings.General.TcpServerPort.ToString();
            Ssl.IsChecked = Settings.General.Ssl;
            ServiceName.Text = Settings.General.ServiceName;

            //using (ManagementObjectSearcher monitors = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
            //{
            //    foreach (ManagementObject monitor in monitors.Get())
            //    {
            //        MonitorName.Items.Add(monitor["Name"].ToString() + "|" + monitor["DeviceId"].ToString());// + "(" + monitor["ScreenHeight"].ToString() +"x"+ monitor["ScreenWidth"].ToString() + ")");
            //    }
            //}
            //foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    // For each screen, add the screen properties to a list box.
            //    MonitorName.Items.Add("Device Name: " + screen.DeviceName);
            //    MonitorName.Items.Add("Bounds: " + screen.Bounds.ToString());
            //    //MonitorName.Items.Add("Type: " + screen.GetType().ToString());
            //    //MonitorName.Items.Add("Working Area: " + screen.WorkingArea.ToString());
            //    MonitorName.Items.Add("Primary Screen: " + screen.Primary.ToString());
            //}
            Monitors.DisplayMemberPath = "Text";
            Monitors.SelectedValuePath = "Value";
            Win32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32.RECT lprcMonitor, IntPtr dwData) =>
              {
                  Win32.MONITORINFOEX mi = new Win32.MONITORINFOEX();
                  mi.Size = Marshal.SizeOf(mi.GetType());
                  if (Win32.GetMonitorInfo(hMonitor, ref mi))
                  {
                      Win32.DISPLAY_DEVICE dd = new Win32.DISPLAY_DEVICE();
                      dd.cb = Marshal.SizeOf(dd.GetType());
                      Win32.EnumDisplayDevices(mi.DeviceName, 0, ref dd, 0);
                      Monitors.Items.Add(new {
                          Text = dd.DeviceString + " (" + (lprcMonitor.Bottom - lprcMonitor.Top) + "x" + (lprcMonitor.Right - lprcMonitor.Left) + ")",
                          Value = dd.DeviceName
                      });
                  }
                  return true;
              };
            Win32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            if (Monitors.Items.Count > 0)
                if (Settings.General.CapturedMonitor != null)
                    Monitors.SelectedValue = Settings.General.CapturedMonitor;
                else
                    Monitors.SelectedIndex = 0;
            
            ShowMpegWindow.IsChecked = Settings.General.ShowMpegWindow;
            WriteMpegOutput2Log.IsChecked = Settings.General.WriteMpegOutput2Log;
        }

        static public void Open()
        {
            if (w == null)
            {
                w = new SettingsWindow();
                w.Closed += delegate 
                {
                    w = null;
                };
            }
            w.Show();
            w.Activate();
        }
        static SettingsWindow w = null;

        void close(object sender, EventArgs e)
        {
            Close();
        }

        void save(object sender, EventArgs e)
        {
            try
            {
                ushort v;

                if (!ushort.TryParse(ServerPort.Text, out v))
                    throw new Exception("Server port must be an integer between 0 and " + ushort.MaxValue);
                Settings.General.TcpClientPort = v;

                if (string.IsNullOrWhiteSpace(DefaultServerIp.Text))
                    throw new Exception("Default server ip is not specified.");
                IPAddress ia;
                if (!IPAddress.TryParse(DefaultServerIp.Text, out ia))
                    throw new Exception("Default server ip is not a valid value.");
                Settings.General.DefaultTcpClientIp = ia.ToString();

                if (!ushort.TryParse(ClientPort.Text, out v))
                    throw new Exception("Client port must be an between 0 and " + ushort.MaxValue);
                Settings.General.TcpServerPort = v;

                Settings.General.Ssl = Ssl.IsChecked ?? false;

                if (string.IsNullOrWhiteSpace(ServiceName.Text))
                    throw new Exception("Service name is not specified.");
                Settings.General.ServiceName = ServiceName.Text.Trim();

                if (Monitors.SelectedIndex < 0)
                    throw new Exception("Captured Video Source is not specified.");
                Settings.General.CapturedMonitor = (string)Monitors.SelectedValue;

                Settings.General.ShowMpegWindow = ShowMpegWindow.IsChecked ?? false;

                Settings.General.WriteMpegOutput2Log = WriteMpegOutput2Log.IsChecked ?? false;

                Settings.General.Save();
                Config.Reload();

                bool running = Service.Running;
                Service.Running = false;
                Service.Running = running;

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }

        void show_log(object sender, RoutedEventArgs e)
        {
            Process.Start(Log.WorkDir);
        }

        void reset_settings(object sender, RoutedEventArgs e)
        {
            if (!Message.YesNo("Do you want to reset settings to their initial state?"))
                return;
            Settings.General.Reset();
            set();
        }
    }
}
