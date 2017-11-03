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
    public class TcpConnection
    {
        public TcpConnection(Socket socket)
        {
            this.socket = socket;
            thread = ThreadRoutines.StartTry(run);
        }
        Socket socket = null;
        Thread thread = null;

        public void Close()
        {
            socket.Close();
            thread.Abort();
            thread = null;
        }

        void run()
        {
            while (thread != null)
            {
                TcpMessage m = TcpMessage.Receive(socket);
                switch (m.Name)
                {
                    case TcpMessage.FfmpegStart:
                        MpegStream.Start(m.Body);
                        InfoWindow.Create("Mpeg Stream", "Mpeg stream started to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                        break;
                    case TcpMessage.FfmpegStop:
                        InfoWindow.Create("Mpeg Stream", "Stopping mpeg stream to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                        MpegStream.Stop();
                        break;
                    default:
                        throw new Exception("Unknown message: " + m.Name);
                }
                m.Reply(socket, TcpMessage.Success);
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
                for (int i = 2; i < NameBodyAsBytes.Length; i++)
                    if (NameBodyAsBytes[i] == '\0')
                        return System.Text.Encoding.ASCII.GetString(NameBodyAsBytes, 0, i);
                return null;
            }
        }
        public string Body
        {
            get
            {
                for (int i = 2; i < NameBodyAsBytes.Length; i++)
                    if (NameBodyAsBytes[i] == '\0')
                        return System.Text.Encoding.ASCII.GetString(NameBodyAsBytes, i + 1, NameBodyAsBytes.Length - i);
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

    public class TcpServer
    {
        //static Dictionary<ushort, TcpServer> servers = new Dictionary<ushort, TcpServer>(); 

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        public TcpServer()
        {
        }

        public void Start(int port)
        {
            if (thread != null && thread.IsAlive)
                return;
            thread = ThreadRoutines.StartTry(() => { start(port); });
        }
        Thread thread = null;

        public void Stop()
        {
            if (thread == null)
                return;
            listeningSocket.Close();
            while (thread.IsAlive)
                thread.Abort();
            thread = null;
        }

        void start(int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listeningSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(localEndPoint);
            listeningSocket.Listen(100);

            while (thread != null)
            {
                Socket socket = listeningSocket.Accept();
                if (tcpConnection != null)
                    tcpConnection.Close();
                tcpConnection = new TcpConnection(socket);
            }
        }
        Socket listeningSocket;
        static TcpConnection tcpConnection = null;
    }
}