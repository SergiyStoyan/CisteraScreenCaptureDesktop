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
        public TcpServerConnection(Socket socket)
        {
            Log.Inform("Starting connection from " + RemoteIp + ":" + RemotePort);

            this.socket = socket;
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

            Log.Inform("Closing connection from " + RemoteIp + ":" + RemotePort);

            if (socket != null)
            {
                socket.Disconnect(false);
                socket.Close();
                socket = null;
            }
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        public bool IsAlive
        {
            get
            {
                return socket != null;
            }
        }

        public  IPAddress LocalIp
        {
            get
            {
                if (!IsAlive)
                    return null;
                return ((IPEndPoint)socket.LocalEndPoint).Address;
            }
        }

        public  ushort LocalPort
        {
            get
            {
                if (!IsAlive)
                    return 0;
                return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        public  IPAddress RemoteIp
        {
            get
            {
                if (!IsAlive)
                    return null;
                return ((IPEndPoint)socket.RemoteEndPoint).Address;
            }
        }

        public  ushort RemotePort
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
}