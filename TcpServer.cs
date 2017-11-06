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

namespace Cliver.CisteraScreenCapture
{
    public class TcpServerConnection : IDisposable
    {
        static readonly Dictionary<ushort, TcpServerConnection> connections = new Dictionary<ushort, TcpServerConnection>();

        static public TcpServerConnection Start(Socket socket)
        {
            lock (connections)
            {
                IPEndPoint iep = (IPEndPoint)socket.LocalEndPoint;
                if (connections.ContainsKey((ushort)iep.Port))
                    return null;
                return new TcpServerConnection(socket);
            }
        }

        static public void Stop(Socket socket)
        {
            lock (connections)
            {
                IPEndPoint iep = (IPEndPoint)socket.LocalEndPoint;
                TcpServerConnection tsc;
                if (!connections.TryGetValue((ushort)iep.Port, out tsc))
                    return;
                tsc.Dispose();
            }
        }

        TcpServerConnection(Socket socket)
        {
            Log.Inform("Starting connection from " + ((IPEndPoint)socket.RemoteEndPoint).Address + ":" + ((IPEndPoint)socket.RemoteEndPoint).Port);

            this.socket = socket;
            IPEndPoint iep = (IPEndPoint)socket.LocalEndPoint;
            connections[(ushort)iep.Port] = this;
            thread = ThreadRoutines.StartTry(
                run,
                (Exception e) => { Log.Error(e); },
                () => { Dispose(); }
                );
        }
        Socket socket = null;
        Thread thread = null;

        ~TcpServerConnection()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (socket == null)
                return;

            Log.Inform("Closing connection from " + ((IPEndPoint)socket.RemoteEndPoint).Address + ":" + ((IPEndPoint)socket.RemoteEndPoint).Port);

            lock (connections)
            {
                IPEndPoint iep = (IPEndPoint)socket.LocalEndPoint;
                connections.Remove((ushort)iep.Port);
            }
            socket.Disconnect(false);
            socket.Close();
            socket = null;
            thread.Abort();
            thread = null;
        }

        void run()
        {
            while (thread != null)
            {
                TcpMessage m = TcpMessage.Receive(socket);
                string reply = TcpMessage.Success;
                try
                {
                    switch (m.Name)
                    {
                        case TcpMessage.FfmpegStart:
                            MpegStream.Start(m.BodyAsText);
                            InfoWindow.Create("Mpeg stream started to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                            break;
                        case TcpMessage.FfmpegStop:
                            InfoWindow.Create("Stopping mpeg stream to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                            MpegStream.Stop();
                            break;
                        default:
                            throw new Exception("Unknown message: " + m.Name);
                    }
                }
                catch(Exception e)
                {
                    reply = e.Message;
                }
                m.Reply(socket, reply);
            }
        }
    }

    public class TcpMessage
    {
        public const string FfmpegStart = "FfmpegStart";
        public const string FfmpegStop = "FfmpegStop";
        public const string Success = "OK";

        public readonly ushort Size;
        public string Name
        {
            get
            {
                for (int i = 0; i < NameBodyAsBytes.Length; i++)
                    if (NameBodyAsBytes[i] == '\0')
                        return System.Text.Encoding.ASCII.GetString(NameBodyAsBytes, 0, i);
                return null;
            }
        }
        public string BodyAsText
        {
            get
            {
                for (int i = 0; i < NameBodyAsBytes.Length; i++)
                    if (NameBodyAsBytes[i] == '\0')
                    {
                        i++;
                        if (i < NameBodyAsBytes.Length)
                            return System.Text.Encoding.ASCII.GetString(NameBodyAsBytes, i, NameBodyAsBytes.Length - i);
                        return "";
                    }
                return null;
            }
        }
        public byte[] BodyAsBytes
        {
            get
            {
                for (int i = 2; i < NameBodyAsBytes.Length; i++)
                    if (NameBodyAsBytes[i] == '\0')
                    {
                        i++;
                        byte[] body = new byte[NameBodyAsBytes.Length - i];
                        if (i < NameBodyAsBytes.Length)
                            NameBodyAsBytes.CopyTo(body, i);
                        return body;
                    }
                return null;
            }
        }
        public readonly byte[] NameBodyAsBytes;

        public TcpMessage(byte[] name_body_as_bytes)
        {
            Size = (ushort)name_body_as_bytes.Length;
            NameBodyAsBytes = name_body_as_bytes;
        }

        public TcpMessage(string name, string body)
        {
            if (body == null)
                body = ""; 
            NameBodyAsBytes = new byte[name.Length + 1 + body.Length + 1];
            Size = (ushort)(NameBodyAsBytes.Length);
            int i = 0;
            Encoding.ASCII.GetBytes(name).CopyTo(NameBodyAsBytes, i);
            i += name.Length + 1;
            Encoding.ASCII.GetBytes(body).CopyTo(NameBodyAsBytes, i);
        }

        static public TcpMessage Receive(Socket socket)
        {
            byte[] message_size_buffer = new byte[2];
            if (socket.Receive(message_size_buffer, message_size_buffer.Length, SocketFlags.None) < message_size_buffer.Length)
                throw new Exception("Could not read from socket the required count of bytes: " + message_size_buffer.Length);
            ushort message_size = BitConverter.ToUInt16(message_size_buffer, 0);
            byte[] message_buffer = new byte[message_size];
            if (socket.Receive(message_buffer, message_buffer.Length, SocketFlags.None) < message_buffer.Length)
                throw new Exception("Could not read from socket the required count of bytes: " + message_buffer.Length);
            return new TcpMessage(message_buffer);
        }

        public void Reply(Socket socket, string body)
        {
            TcpMessage m = new TcpMessage(Name, body);
            m.send(socket);
        }

        void send(Socket socket)
        {
            byte[] sizeAsBytes = BitConverter.GetBytes(Size);
            if (socket.Send(sizeAsBytes) < sizeAsBytes.Length)
                throw new Exception("Could not send to socket the required count of bytes: " + sizeAsBytes.Length);
            if (socket.Send(NameBodyAsBytes) < NameBodyAsBytes.Length)
                throw new Exception("Could not send to socket the required count of bytes: " + NameBodyAsBytes.Length);
        }

        public TcpMessage SendAndReceiveReply(Socket socket)
        {
            send(socket);
            return Receive(socket);
        }
    }

    static public class TcpServer
    {
        //static Dictionary<ushort, TcpServer> servers = new Dictionary<ushort, TcpServer>(); 

        static public void Start(IPAddress destination_ip, int port)
        {
            if (thread != null && thread.IsAlive)
                return;

            if (!NetworkRoutines.IsNetworkAvailable())
                throw new Exception("No network available.");
            IPAddress ipAddress = NetworkRoutines.GetLocalIpForDestination(destination_ip);
            //listeningSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //listeningSocket.Bind(localEndPoint);
            ////listeningSocket.Listen(100);
            server = new TcpListener(ipAddress, port);
            server.Start();

            thread = ThreadRoutines.StartTry(() => { run(destination_ip, port); }, (Exception e) =>
            {
                if (e is SocketException)
                {
                    Log.Warning(e);
                }
                else
                {
                    Log.Error(e);
                    InfoWindow.Create(Log.GetExceptionMessage(e), null, "OK", null, Settings.View.ErrorSoundFile, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
                }
            });
        }
        static Thread thread = null;

        public  static IPAddress Ip
        {
            get
            {
                if (server == null)
                    return null;
                return ((IPEndPoint)server.LocalEndpoint).Address;
            }
        }

        static public void Stop()
        {
            if (thread == null)
                return;
            //listeningSocket.Close(0);
            server?.Stop();
            while (thread.IsAlive)
                thread.Abort();
            thread = null;
        }

        static void run(IPAddress destination_ip, int port)
        {
            //while (thread != null)
            //{
            //    //var r = listeningSocket.BeginAccept(accepted, listeningSocket);
            //    Socket socket = listeningSocket.Accept();
            //    if (connection != null)
            //        connection.Dispose();
            //    connection = TcpServerConnection.Start(socket);
            //}

            while (thread != null)
            {
                Socket socket = server.AcceptSocket();                
                if (connection != null)
                    connection.Dispose();
                connection = TcpServerConnection.Start(socket);
            }
        }
        static TcpListener server = null;
        //static Socket listeningSocket;
        static TcpServerConnection connection = null;

        //static void accepted(System.IAsyncResult result)
        //{            
        //    Socket socket = listeningSocket.EndAccept(result);
        //    TcpServerConnection.Start(socket);
        //}
    }
}