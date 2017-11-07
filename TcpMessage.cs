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
}