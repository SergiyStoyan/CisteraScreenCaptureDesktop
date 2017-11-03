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

namespace Cliver.CisteraScreenCaptureTestServer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, int.Parse(localPort.Text));
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            
            // URI prefixes are required,
            // for example "http://127.0.0.1:5800/screenCapture/".
            string[] prefixes = new string[] { "http://127.0.0.1:5800/screenCapture/" };
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");
            HttpListener listener = new HttpListener();
            foreach (string s in prefixes)
                listener.Prefixes.Add(s);
            listener.Start();
            listener.BeginGetContext(http_callback, listener);

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
        HttpListener listener;

        private void start_Click(object sender, EventArgs e)
        {
            socket.Connect(remoteHost.Text, int.Parse(remotePort.Text));

            TcpMessage m = new TcpMessage(TcpMessage.FfmpegStart, "test 123");
            TcpMessage m2 = m.SendAndReceiveReply(socket);
            
            socket.Disconnect(true);
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
}
