//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Cliver;
using System.Configuration;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security.Authentication;

namespace Cliver.CisteraScreenCaptureService
{
    public class TcpServerConnection : IDisposable
    {
        public TcpServerConnection(Socket socket)
        {
            this.socket = socket;
            stream = new NetworkStream(socket);

            Log.Main.Inform("Starting connection from " + RemoteIp + ":" + RemotePort);

            thread = ThreadRoutines.StartTry(
                run,
                (Exception e) =>
                {
                    if (socket != null && !socket.Connected)
                        Log.Main.Inform("Socket from " + RemoteIp + ":" + RemotePort + " has been disconnected.");
                    else
                        Log.Main.Error(e);
                },
                () =>
                {
                    ThreadRoutines.StartTry(Dispose);
                }
                );
        }
        Socket socket = null;
        Thread thread = null;
        Stream stream = null;

        ~TcpServerConnection()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (socket == null)
                    return;

                Log.Main.Inform("Closing connection from " + RemoteIp + ":" + RemotePort);

                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(true);
                    socket.Close();
                    socket = null;
                }
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }
            }
        }

        public bool IsAlive
        {
            get
            {
                return socket != null;
            }
        }

        public IPAddress LocalIp
        {
            get
            {
                if (!IsAlive)
                    return null;
                return ((IPEndPoint)socket.LocalEndPoint).Address;
            }
        }

        public ushort LocalPort
        {
            get
            {
                if (!IsAlive)
                    return 0;
                return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        public IPAddress RemoteIp
        {
            get
            {
                if (!IsAlive)
                    return null;
                return ((IPEndPoint)socket.RemoteEndPoint).Address;
            }
        }

        public ushort RemotePort
        {
            get
            {
                if (!IsAlive)
                    return 0;
                return (ushort)((IPEndPoint)socket.RemoteEndPoint).Port;
            }
        }

        void run()
        {
            while (thread != null)
            {
                TcpMessage m = TcpMessage.Receive(stream);

                Log.Main.Inform("Tcp message received: " + m.Name + "\r\n" + m.BodyAsText);
                CisteraScreenCaptureService.Events.UiMessage.Info("Tcp message received: " + m.Name + "\r\n" + m.BodyAsText);

                string reply = TcpMessage.Success;
                try
                {
                    switch (m.Name)
                    {
                        case TcpMessage.FfmpegStart:
                            MpegStream.Start(m.BodyAsText);
                            break;
                        case TcpMessage.FfmpegStop:
                            MpegStream.Stop();
                            break;
                        case TcpMessage.SslStart:
                            if (stream is SslStream)
                                throw new Exception("SSL is already started.");
                            break;
                        default:
                            throw new Exception("Unknown message: " + m.Name);
                    }
                }
                catch (Exception e)
                {
                    reply = e.Message;
                    Log.Main.Error("Tcp message processing: ", e);
                }
                Log.Main.Inform("Tcp message sending: " + m.Name + "\r\n" + reply);
                m.Reply(stream, reply);
                if (m.Name == TcpMessage.SslStart && reply == TcpMessage.Success)
                    startSsl();
            }
        }

        void startSsl()
        {
            SslStream sstream = new SslStream(stream, false, remoteCertificateValidationCallback);
            stream = sstream;
            //sstream.AuthenticateAsClient("", null, SslProtocols.Tls12, false);
            //sstream.AuthenticateAsClient(server, null, SslProtocols.Default, false);
            sstream.AuthenticateAsClient("");
        }

        bool remoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}