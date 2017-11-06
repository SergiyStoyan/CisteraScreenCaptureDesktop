using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Cliver.CisteraScreenCapture;
using Zeroconf;

namespace Cliver.CisteraScreenCaptureTestServer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
                        
            HttpListener listener = new HttpListener();
            // URI prefixes are required,
            // for example "http://127.0.0.1:5800/screenCapture/" (does not work in LAN)
            //listener.Prefixes.Add("http://192.168.2.15:80/");
            listener.Prefixes.Add("http://127.0.0.1:80/");
            listener.Prefixes.Add("http://localhost:80/");
            listener.Start();
            listener.BeginGetContext(http_callback, listener);
            
            
            //Zeroconf..

            FormClosed += delegate
              {
                  try
                  {
                      listener.Stop();
                  }
                  catch
                  {

                  }
              };
        }
        Socket socket;

        private void start_Click(object sender, EventArgs e)
        { 
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = NetworkRoutines.GetLocalIpForDestination(IPAddress.Parse("127.0.0.1"));
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, int.Parse(localPort.Text));
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);

            socket.Connect(remoteHost.Text, int.Parse(remotePort.Text));

            TcpMessage m = new TcpMessage(TcpMessage.FfmpegStart, "test 123");
            TcpMessage m2 = m.SendAndReceiveReply(socket);

            socket.Disconnect(false);
            socket.Close();
            socket = null;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            socket.Connect(remoteHost.Text, int.Parse(remotePort.Text));

            TcpMessage m = new TcpMessage(TcpMessage.FfmpegStop, null);
            TcpMessage m2 = m.SendAndReceiveReply(socket);

            socket.Disconnect(true);
        }

        void http_callback(System.IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "OK";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }

    //public class ZeroConf: IZeroconfHost
    //{

    //}
}
