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

namespace Cliver.CisteraScreenCapture
{
    public class TcpServerConnection : IDisposable
    {
        readonly static string SslCertificateFileName = Log.AppDir + "\\server_certificate.pem";
        readonly static string SslPrivateKeyFileName = Log.AppDir + "\\server_key.pem";

        public TcpServerConnection(Socket socket)
        {
            this.socket = socket;
            stream = new NetworkStream(socket);

            Log.Inform("Starting connection from " + RemoteIp + ":" + RemotePort);

            thread = ThreadRoutines.StartTry(
                run,
                (Exception e) => { Log.Error(e); },
                () => { Dispose(); }
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
            if (socket == null)
                return;

            Log.Inform("Closing connection from " + RemoteIp + ":" + RemotePort);

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
                TcpMessage m = TcpMessage.Receive(stream);

                Log.Inform("Tcp message received: " + m.Name + "\r\n" + m.BodyAsText);

                string reply = TcpMessage.Success;
                try
                {
                    switch (m.Name)
                    {
                        case TcpMessage.FfmpegStart:
                            InfoWindow.Create("Starting mpeg stream to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                            MpegStream.Start(m.BodyAsText);
                            break;
                        case TcpMessage.FfmpegStop:
                            InfoWindow.Create("Stopping mpeg stream to " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                            MpegStream.Stop();
                            break;
                        //case TcpMessage.SslStart:
                        //    InfoWindow.Create("Starting ssl on the connection " + socket.RemoteEndPoint.ToString() + " by server request.", null, "OK", null);
                        //    SslStream sstream = new SslStream(stream, false);
                        //    stream = sstream;                           

                        //    //byte[] certificateBuffer = SslRoutines.GetBytesFromPEM(File.ReadAllText(), SslRoutines.PemStringType.Certificate);
                        //    //X509Certificate2 certificate = new X509Certificate2(certificateBuffer);
                        //    //byte[] keyBuffer = SslRoutines.GetBytesFromPEM(File.ReadAllText(), SslRoutines.PemStringType.PrivateKey);
                        //    //RSACryptoServiceProvider prov = System.Web.Helpers.Crypto.DecodeRsaPrivateKey(keyBuffer);
                        //    //certificate.PrivateKey = prov;

                        //    OpenSSL.X509Certificate2Provider.CertificateFromFileProvider cffp = new OpenSSL.X509Certificate2Provider.CertificateFromFileProvider(File.ReadAllText(SslCertificateFileName), File.ReadAllText(SslPrivateKeyFileName));
                        //    //X509Certificate certificate = new X509Certificate();
                        //    sstream.AuthenticateAsServer(cffp.Certificate, false, SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, false);
                        //    break;
                        default:
                            throw new Exception("Unknown message: " + m.Name);
                    }
                }
                catch(Exception e)
                {
                    reply = e.Message;
                }

                Log.Inform("Tcp message sending: " + m.Name + "\r\n" + reply);

                m.Reply(stream, reply);
            }
        }

        //public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //{
        //    //var coll = new X509Certificate2Collection();
        //    //var cert = new X509Certificate2("provided.pfx", "WhateverPasswordYouGaveIt");
        //    //coll.Add(cert);

        //    //// do TcpClient stuff
        //    //var sslStream = new SslStream(tcpClient.GetStream());
        //    //const SslProtocols allowedProtocols = SslProtocols.Tls12 | SslProtocols.Tls11;
        //    //sslStream.AuthenticateAsClient("127.0.0.1", coll, allowedProtocols, checkCertificateRevocation: true);
        //    return true;
        //}

        //public class SslRoutines
        //{
        //    /// <summary>
        //    /// This helper function parses an integer size from the reader using the ASN.1 format
        //    /// </summary>
        //    /// <param name="rd"></param>
        //    /// <returns></returns>
        //    public static int DecodeIntegerSize1(System.IO.BinaryReader rd)
        //    {
        //        byte byteValue;
        //        int count;

        //        byteValue = rd.ReadByte();
        //        if (byteValue != 0x02)        // indicates an ASN.1 integer value follows
        //            return 0;

        //        byteValue = rd.ReadByte();
        //        if (byteValue == 0x81)
        //        {
        //            count = rd.ReadByte();    // data size is the following byte
        //        }
        //        else if (byteValue == 0x82)
        //        {
        //            byte hi = rd.ReadByte();  // data size in next 2 bytes
        //            byte lo = rd.ReadByte();
        //            count = BitConverter.ToUInt16(new[] { lo, hi }, 0);
        //        }
        //        else
        //        {
        //            count = byteValue;        // we already have the data size
        //        }

        //        //remove high order zeros in data
        //        while (rd.ReadByte() == 0x00)
        //        {
        //            count -= 1;
        //        }
        //        rd.BaseStream.Seek(-1, System.IO.SeekOrigin.Current);

        //        return count;
        //    }

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="pemString"></param>
        //    /// <param name="type"></param>
        //    /// <returns></returns>
        //    public static byte[] GetBytesFromPEM(string pemString, PemStringType type)
        //    {
        //        string header; string footer;

        //        switch (type)
        //        {
        //            case PemStringType.Certificate:
        //                header = "-----BEGIN CERTIFICATE-----";
        //                footer = "-----END CERTIFICATE-----";
        //                break;
        //            case PemStringType.RsaPrivateKey:
        //                header = "-----BEGIN RSA PRIVATE KEY-----";
        //                footer = "-----END RSA PRIVATE KEY-----";
        //                break;
        //            case PemStringType.PrivateKey:
        //                header = "-----BEGIN PRIVATE KEY-----";
        //                footer = "-----END PRIVATE KEY-----";
        //                break;
        //            default:
        //                return null;
        //        }

        //        int start = pemString.IndexOf(header) + header.Length;
        //        int end = pemString.IndexOf(footer, start) - start;
        //        return Convert.FromBase64String(pemString.Substring(start, end));
        //    }

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="inputBytes"></param>
        //    /// <param name="alignSize"></param>
        //    /// <returns></returns>
        //    public static byte[] AlignBytes1(byte[] inputBytes, int alignSize)
        //    {
        //        int inputBytesSize = inputBytes.Length;

        //        if ((alignSize != -1) && (inputBytesSize < alignSize))
        //        {
        //            byte[] buf = new byte[alignSize];
        //            for (int i = 0; i < inputBytesSize; ++i)
        //            {
        //                buf[i + (alignSize - inputBytesSize)] = inputBytes[i];
        //            }
        //            return buf;
        //        }
        //        else
        //        {
        //            return inputBytes;      // Already aligned, or doesn't need alignment
        //        }
        //    }

        //    public enum PemStringType
        //    {
        //        Certificate,
        //        RsaPrivateKey,
        //        PrivateKey
        //    }
        //}
    }
}