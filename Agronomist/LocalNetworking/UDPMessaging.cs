using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.Networking.Connectivity;
using Windows.Networking;

namespace Agronomist.LocalNetworking
{
    /// <summary>
    /// This class broadcasts UDP messages.  It should be instantiated by the Manager. 
    /// The sensor boxees listen for those UDP messages, and connect to the server that sent them. 
    /// That way the server does not need a static IP 
    /// Here i use a System.Net api which is different to Windows.Networking, and is somewhat lower level. 
    /// If you try to instantiate this class twice, you will get an exception! 
    /// </summary>
    public class UDPMessaging
    {
        private static UDPMessaging _instance = null;

        private const int BroadcastIntervalSeconds = 3;
        private DispatcherTimer UdpBroadcastTimer;
        

        Socket _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 44000);


        public UDPMessaging()
        {
            if (_instance != null)
            {
                throw new Exception("Tried to create more than one UDPMessaging Class!");
            }
            else
            {
                UdpBroadcastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(BroadcastIntervalSeconds) };
                UdpBroadcastTimer.Tick += UDPBroadcast;
                UdpBroadcastTimer.Start();

                _udpSocket.Bind(remoteEndPoint);
                ///this event is used to give all the parameters to WSystem.net api
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                byte[] buffer = new byte[1024];
                e.SetBuffer(buffer, 0, buffer.Length);
                e.RemoteEndPoint = remoteEndPoint;
                e.Completed += ReceiveFromCallback;
                

                //_udpSocket.ConnectAsync(new SocketAsyncEventArgs()); 

                if (!_udpSocket.ReceiveFromAsync(e))
                {
                    ReceiveFromCallback(_udpSocket, e);
                }

                _instance = this; 
            }
        }

        private void UDPBroadcast(object sender, object o)
        {
            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null && localHostName.Type == HostNameType.Ipv4
                    && localHostName.IPInformation.PrefixLength.HasValue) //by making sure that there is a prefix, we will probably hit a local network
                {
                    string ipString = localHostName.CanonicalName;
                    string[] stringBytes = ipString.Split('.');
                    byte[] ipBytes = new byte[stringBytes.Length]; 
                                         
                    for (int i=0; i < stringBytes.Length; i++)
                    {
                        ipBytes[i] = Byte.Parse(stringBytes[i]);
                        int maskLength = localHostName.IPInformation.PrefixLength.Value - 8 * i;
                        if (maskLength > 0)
                        {
                            byte bynaryMask = (byte)(255 >> maskLength);
                            ipBytes[i] = (byte)(ipBytes[i] | bynaryMask);
                        }
                        else
                            ipBytes[i] = 255;
                    }

                    IPAddress broadcast = new IPAddress(ipBytes);

                    SocketAsyncEventArgs sds = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = new IPEndPoint(broadcast, 44000),
                    };
                    sds.SetBuffer(new byte[] { 115, 101, 107, 114, 101, 116 }, 0, 6); //sekret - the message arduino reacts to 

                    _udpSocket.SendToAsync(sds);
                }
            }

           
        }


        /// <summary>
        /// This is a call-back function triggered wehn we receive a new message, otehrwise known as Async for C 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveFromCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Debug.WriteLine(e.SocketError);
                return;
            }

            Socket udpSocket = sender as Socket;

            byte[] buffer = e.Buffer;
            //NOw we got hte data!!
            Debug.WriteLine(Encoding.UTF8.GetString(buffer, 0, e.BytesTransferred));

            if (!udpSocket.ReceiveFromAsync(e))
            {
                ReceiveFromCallback(udpSocket, e);
            }
        }
    }

}
