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
//using Mono.Zeroconf;
//using Bonjour;
//using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Cliver.CisteraScreenCaptureTestServer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            mpegCommandLine.Text = "-f gdigrab -framerate 10 -f rtp_mpegts -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -srtp_out_params aMg7BqN047lFN72szkezmPyN1qSMilYCXbqP/sCt srtp://127.0.0.1:5920";

            CreateHandle();
            stateText = "";
            startEnabled = false;
            stopEnabled = false;
            
            HttpListener listener = new HttpListener();
            try
            {
                // URI prefixes are required,
                // for example "http://127.0.0.1:5800/screenCapture/" (does not work in LAN)
                //listener.Prefixes.Add("http://192.168.2.15:80/");
                //listener.Prefixes.Add("http://127.0.0.1:80/");
                //listener.Prefixes.Add("http://localhost:80/");
                listener.Prefixes.Add("http://*:80/");
                listener.Start();
                listener.BeginGetContext(http_callback, listener);

                stateText = "Wating for HTTP request...";
            }
            catch(Exception e)
            {
                Message.Error(e);
            }

            //ServiceBrowser browser = new ServiceBrowser();
            //browser.ServiceAdded += delegate (object o, ServiceBrowseEventArgs args) {
            //    Console.WriteLine("Found Service: {0}", args.Service.Name);
            //    args.Service.Resolved += delegate (object o2, ServiceResolvedEventArgs args2) {
            //        IResolvableService s = (IResolvableService)args2.Service;
            //        Console.WriteLine("Resolved Service: {0} - {1}:{2} ({3} TXT record entries)",
            //            s.FullName, s.HostEntry.AddressList[0], s.Port, s.TxtRecord.Count);
            //    };
            //    args.Service.Resolve();
            //};
            //browser.Browse("_daap._tcp", "local");

            //RegisterService service = new RegisterService();
            //service.Name = "Aaron's DAAP Share";
            //service.RegType = "_daap._tcp";
            //service.ReplyDomain = "local.";
            //service.Port = 3689;
            //// TxtRecords are optional
            //TxtRecord txt_record = new TxtRecord();
            //txt_record.Add("Password", "false");
            //service.TxtRecord = txt_record;
            //service.Register();
            
            // int t = DNSServiceRegister(ref IntPtr.Zero, 0, 0, "test",, "_cisterascreencapturecontroller._tcp", null, null, 5353, 0, null, IntPtr.Zero, IntPtr.Zero);
            
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

        //     [DllImport("dnssd.dll", SetLastError = true)]
        //     public static extern Int32 DNSServiceRegister(
        //   ref  HandleRef sdRef, //DNSServiceRef* sdRef,
        //Int32 flags, //DNSServiceFlags flags,
        //UInt32 interfaceIndex, //uint32_t interfaceIndex,
        //string name, //const char* name,         /* may be NULL */
        //string regtype, //const char* regtype,
        //string domain, //const char* domain,       /* may be NULL */
        //string host, //const char* host,         /* may be NULL */
        //UInt16 port, //uint16_t                            port,          /* In network byte order */
        //UInt16 txtLen, //uint16_t txtLen,
        //string txtRecord, //const void* txtRecord,    /* may be NULL */
        // IntPtr callBack, //DNSServiceRegisterReply             callBack,      /* may be NULL */
        //IntPtr context //void* context       /* may be NULL */
        // );


        //       typedef void (DNSSD_API* DNSServiceRegisterReply)
        //   (
        //   DNSServiceRef sdRef,
        //   DNSServiceFlags flags,
        //   DNSServiceErrorType errorCode,
        //   const char* name,
        //   const char* regtype,
        //   const char* domain,
        //   void* context
        //   );

        //       static void DNSSD_API serviceRegisterReply(
        //       DNSServiceRef sdRef,
        //       DNSServiceFlags flags,
        //       DNSServiceErrorType errorCode,

        //       const char* name,

        //       const char* regtype,

        //       const char* domain,

        //       void* context
        //)
        //{

        //       TCHAR name_[255];

        //       mbstowcs(name_, name, sizeof(name_));
        //	if (errorCode == kDNSServiceErr_NoError)
        //	{
        //		if (flags & kDNSServiceFlagsAdd)
        //			log->info(_T("BonjourService: Service %s is registered and active."), name_);
        //		else
        //			log->info(_T("BonjourService: Service %s is unregistered."), name_);
        //	}
        //	else if (errorCode == kDNSServiceErr_NameConflict)
        //		log->error(_T("BonjourService: Service name %s is in use, please choose another."), name_);
        //	else
        //		log->error(_T("BonjourService: Error: %d"), errorCode);
        //}

        void connect_socket()
        {
            if (socket == null)
            {
                IPAddress ipAddress = NetworkRoutines.GetLocalIpForDestination(IPAddress.Parse("127.0.0.1"));
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, int.Parse(localTcpPort.Text));
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(localEndPoint);
            }
            if(!socket.Connected || !socket.Poll(1000, SelectMode.SelectWrite))
                socket.Connect(remoteHost, int.Parse(remotePort));
        }

        void disconnect_socket()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                socket.Disconnect(true);
            }
            catch { }
            try
            {
                socket.Close();
            }
            finally
            {
                socket = null;
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                stateText = "Starting MPEG...";

                connect_socket();
                TcpMessage m = new TcpMessage(TcpMessage.FfmpegStart, mpegCommandLine.Text);
                TcpMessage m2 = m.SendAndReceiveReply(socket);

                //Message.Inform("Response: " + m2.BodyAsText);
                stateText = "MPEG started";
                startEnabled = false;
                stopEnabled = true;
            }
            catch (Exception ex)
            {
                Message.Error(ex);
            }
            finally
            {
                //disconnect_socket();
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            try
            {
                stateText = "Stopping MPEG...";

                connect_socket();
                TcpMessage m = new TcpMessage(TcpMessage.FfmpegStop, null);
                TcpMessage m2 = m.SendAndReceiveReply(socket);

                //Message.Inform("Response: " + m2.BodyAsText);
                stateText = "MPEG stopped";
                startEnabled = true;
                stopEnabled = false;
            }
            catch (Exception ex)
            {
                Message.Error(ex);
            }
            finally
            {
                //disconnect_socket();
            }
        }

        void http_callback(System.IAsyncResult result)
        {
            string responseString;
            string username = null;
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            try
            {
                Match m = Regex.Match(request.Url.Query, @"username=(.+?)(&|$)");
                if (!m.Success)
                    throw new Exception("No username in http request.");
                username = m.Groups[1].Value;

                m = Regex.Match(request.Url.Query, @"ipaddress=(.+?)(&|$)");
                if (!m.Success)
                    throw new Exception("No ipaddress in http request.");
                remoteHost = m.Groups[1].Value;

                m = Regex.Match(request.Url.Query, @"port=(.+?)(&|$)");
                if (!m.Success)
                    throw new Exception("No port in http request.");
                remotePort = m.Groups[1].Value;

                responseString = "OK";
            }
            catch (Exception e)
            {
                responseString = Message.GetExceptionDetails(e);
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

            List<string> ss = new List<string>();
            ss.Add("Received HTTP request: " + request.Url);
            ss.Add("username: " + username);
            ss.Add("remoteHost: " + remoteHost);
            ss.Add("remotePort: " + remotePort);
            ss.Add("Sent HTTP response: " + responseString);
            //Message.Inform(string.Join("\r\n", ss));

            stateText = string.Join("\r\n", ss);
            startEnabled = true;
        }
        string remoteHost;
        string remotePort;

        string stateText
        {
            set
            {
                state.BeginInvoke(() =>
                {
                    state.Text = value;
                });
            }
        }

        bool startEnabled
        {
            set
            {
                start.BeginInvoke(() =>
                {
                    start.Enabled = value;
                });
            }
        }

        bool stopEnabled
        {
            set
            {
                stop.BeginInvoke(() =>
                {
                    stop.Enabled = value;
                });
            }
        }
    }
}
