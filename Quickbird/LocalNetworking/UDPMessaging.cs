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
using System.Threading;

namespace Quickbird.LocalNetworking
{
    using Util;

    /// <summary>
    /// This class broadcasts UDP messages.  It should be instantiated by the Manager. 
    /// The sensor boxees listen for those UDP messages, and connect to the server that sent them. 
    /// That way the server does not need a static IP 
    /// Here i use a System.Net api which is different to Windows.Networking, and is somewhat lower level. 
    /// If you try to instantiate this class twice, you will get an exception! 
    /// </summary>
    public class UDPMessaging : IDisposable
    {
        private static UDPMessaging _instance = null;

        public const int BroadcastIntervalSeconds = 3;
        private DispatcherTimer UdpBroadcastTimer;
        

        private Socket _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 44000);
        private List<IPAddress> _localEndPoints = new List<IPAddress>(); 

        private bool disposedValue = false; // To detect redundant calls
        public bool Disposed { get { return disposedValue; } }

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
            List<IPAddress> localEndPoints = new List<IPAddress>(); 

            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null && localHostName.Type == HostNameType.Ipv4
                    && localHostName.IPInformation.PrefixLength.HasValue) //by making sure that there is a prefix, we will probably hit a local network
                {
                    
                    string ipString = localHostName.CanonicalName;
                    string[] stringBytes = ipString.Split('.');
                    byte[] localIPBytes = new byte[stringBytes.Length];
                    byte[] broadcastIpBytes = new byte[stringBytes.Length];

                    
                                         
                    for (int i=0; i < stringBytes.Length; i++)
                    {
                        localIPBytes[i] = Byte.Parse(stringBytes[i]);
                        broadcastIpBytes[i] = localIPBytes[i]; 

                        int maskLength = localHostName.IPInformation.PrefixLength.Value - 8 * i;

                        if (maskLength > 0)
                        {
                            byte bynaryMask = (byte)(255 >> maskLength);
                            broadcastIpBytes[i] = (byte)(broadcastIpBytes[i] | bynaryMask);
                        }
                        else
                            broadcastIpBytes[i] = 255;
                    }

                    localEndPoints.Add(new IPAddress(localIPBytes));
                    IPAddress broadcast = new IPAddress(broadcastIpBytes);

                    SocketAsyncEventArgs sds = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = new IPEndPoint(broadcast, 44000),
                    };
                    sds.SetBuffer(new byte[] { 115, 101, 107, 114, 101, 116 }, 0, 6); //sekret - the message arduino reacts to 

                    _udpSocket.SendToAsync(sds);
                }
            }

            Interlocked.Exchange(ref _localEndPoints, localEndPoints); 
        }


        /// <summary>
        /// This is a call-back function triggered when we receive a new message, otehrwise known as Async for C 
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
            IPEndPoint recievedFrom = e.RemoteEndPoint as IPEndPoint;
            byte[] buffer = e.Buffer;

            
            if (_localEndPoints.Contains(recievedFrom.Address) == false)
            {
                try
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, e.BytesTransferred);
                    if (message == "sekret")
                    {
                        Task.Run(()=>Messenger.Instance.LocalNetworkConflict.Invoke(e.RemoteEndPoint.ToString()));
                        Debug.WriteLine(e.RemoteEndPoint.ToString());
                    }
                }
                catch(Exception exception)
                {
                    Debug.WriteLine("Got a weired broadcast from " + recievedFrom.Address); 
                }
                
            }

            if (!udpSocket.ReceiveFromAsync(e))
            {
                ReceiveFromCallback(udpSocket, e);
            }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                UdpBroadcastTimer.Stop();
                UdpBroadcastTimer.Tick -= UDPBroadcast; 
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                _udpSocket.Dispose();

                // TODO: set large fields to null.
                _instance = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~UDPMessaging()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }

}
