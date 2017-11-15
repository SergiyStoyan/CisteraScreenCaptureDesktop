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
    static public class TcpServer
    {
        //static Dictionary<ushort, TcpServer> servers = new Dictionary<ushort, TcpServer>(); 

        static public void Start(int local_port, IPAddress destination_ip)
        {
            if (!NetworkRoutines.IsNetworkAvailable())
                throw new Exception("No network available.");
            IPAddress ipAddress = NetworkRoutines.GetLocalIpForDestination(destination_ip);

            if (local_port == LocalPort && ipAddress.Equals(LocalIp))
                return;
            Stop();

            Log.Inform("Starting TCP listener on " + local_port + " for " + destination_ip);

            //listeningSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //listeningSocket.Bind(localEndPoint);
            ////listeningSocket.Listen(100);
            server = new TcpListener(ipAddress, local_port);
            server.Start();

            thread = ThreadRoutines.StartTry(run, (Exception e) =>
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
                Stop();
            });
        }
        static Thread thread = null;

        static public void Stop()
        {
            if (server == null)
                return;

            Log.Inform("Stopping TCP listener on " + ((IPEndPoint)server.LocalEndpoint).Port);

            //listeningSocket.Close(0);
            if (server != null)
            {
                server?.Stop();
                server = null;
            }
            if (thread != null)
            {
                while (thread.IsAlive)
                    thread.Abort();
                thread = null;
            }
        }

        static void run()
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
                connection = new TcpServerConnection(socket);
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

        public static IPAddress LocalIp
        {
            get
            {
                if (server == null)
                    return null;
                return ((IPEndPoint)server.LocalEndpoint).Address;
            }
        }

        public static ushort LocalPort
        {
            get
            {
                if (server == null)
                    return 0;
                return (ushort)((IPEndPoint)server.LocalEndpoint).Port;
            }
        }

        public static bool Running
        {
            get
            {
                return server != null;
            }
        }

        public static TcpServerConnection Connection
        {
            get
            {
                return connection;
            }
        }      
    }
}