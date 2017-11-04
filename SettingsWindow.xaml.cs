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
                if(Visibility == Visibility.Visible)
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

            ServerPort.Text = Settings.General.TcpClientPort.ToString();
            DefaultServerIp.Text = Settings.General.DefaultTcpClientIp.ToString();
            ClientPort.Text = Settings.General.TcpServerPort.ToString();
            Ssl.IsChecked = Settings.General.Ssl;
            ServiceName.Text = Settings.General.ServiceName;
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
                if(!IPAddress.TryParse(DefaultServerIp.Text, out ia))
                    throw new Exception("Default server ip is not a valid value.");
                Settings.General.DefaultTcpClientIp = ia.ToString(); 

                if (!ushort.TryParse(ClientPort.Text, out v))
                    throw new Exception("Client port must be an between 0 and " + ushort.MaxValue);
                Settings.General.TcpServerPort = v;

                Settings.General.Ssl = Ssl.IsChecked ?? false;

                if (string.IsNullOrWhiteSpace(Settings.General.ServiceName))
                    throw new Exception("Service name is not specified.");
                Settings.General.ServiceName = ServiceName.Text.Trim();

                Settings.General.Save();
                Config.Reload();

                Close();

                bool running = Service.Running;
                Service.Running = false;
                Service.Running = running;
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
    }
}
