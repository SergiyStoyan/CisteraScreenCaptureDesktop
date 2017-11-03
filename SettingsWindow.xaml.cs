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

namespace Cliver.CisteraScreenCapture
{
    public partial class SettingsWindow : Window
    {
        SettingsWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            Icon = AssemblyRoutines.GetAppIconImageSource();

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            ServerPort.Text = Settings.General.ServerPort.ToString();
            DefaultServerIp.Text = Settings.General.DefaultServerIp.ToString();
            ClientPort.Text = Settings.General.ClientPort.ToString();
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
                    throw new Exception("Server port must be positive integer.");
                Settings.General.ServerPort = v;

                if (string.IsNullOrWhiteSpace(DefaultServerIp.Text))
                    throw new Exception("Default server ip is not specified.");
                IPAddress ia;
                if(!IPAddress.TryParse(DefaultServerIp.Text, out ia))
                    throw new Exception("Default server ip could not be parsed.");
                Settings.General.DefaultServerIp = ia.ToString(); ;

                if (!ushort.TryParse(ClientPort.Text, out v))
                    throw new Exception("Client port must be positive integer.");
                Settings.General.ClientPort = v;

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
