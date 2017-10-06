namespace Quickbird.LocalNetworking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Networking;
    using Windows.Networking.Connectivity;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Util;

    /// <summary>This class broadcasts UDP messages.  It should be instantiated by the Manager. The sensor
    /// boxees listen for those UDP messages, and connect to the server that sent them. That way the server
    /// does not need a static IP Here i use a System.Net api which is different to Windows.Networking, and
    /// is somewhat lower level. If you try to instantiate this class twice, you will get an exception!</summary>
    public class UDPMessaging : IDisposable
    {
        public const int BroadcastIntervalSeconds = 3;
        private static UDPMessaging _instance;


        private readonly Socket _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private readonly IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 44000);
        private List<IPAddress> _localEndPoints = new List<IPAddress>();
        private DispatcherTimer UdpBroadcastTimer;

        public UDPMessaging()
        {
            if (_instance != null)
            {
                throw new Exception("Tried to create more than one UDPMessaging Class!");
            }
            Task.Run(() => ((App) Application.Current).Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UdpBroadcastTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(BroadcastIntervalSeconds)};
                UdpBroadcastTimer.Tick += UDPBroadcast;
                UdpBroadcastTimer.Start();
            }));

            _udpSocket.Bind(remoteEndPoint);
            //this event is used to give all the parameters to WSystem.net api
            var e = new SocketAsyncEventArgs();
            var buffer = new byte[1024];
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

        public bool Disposed { get; private set; }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                var cleanUpTimer = new Action(() =>
                {
                    UdpBroadcastTimer.Stop();
                    UdpBroadcastTimer.Tick -= UDPBroadcast;
                });

                BlockingDispatcher.Run(cleanUpTimer);

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                _udpSocket.Dispose();

                // TODO: set large fields to null.
                _instance = null;

                Disposed = true;
            }
        }

        /// <summary>This is a call-back function triggered when we receive a new message, otehrwise known as
        /// Async for C</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveFromCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Util.LoggingService.LogInfo($" UDP Socket recieve error {e.SocketError}", Windows.Foundation.Diagnostics.LoggingLevel.Error);
                return;
            }

            var udpSocket = sender as Socket;
            var recievedFrom = e.RemoteEndPoint as IPEndPoint;
            var buffer = e.Buffer;


            if (_localEndPoints.Contains(recievedFrom.Address) == false)
            {
                try
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, e.BytesTransferred);
                    if (message == "sekret")
                    {
                        Task.Run(() => BroadcasterService.Instance.LocalNetworkConflict.Invoke(e.RemoteEndPoint.ToString()));
                        Util.LoggingService.LogInfo($"lan Conflict {e.RemoteEndPoint.ToString()}", Windows.Foundation.Diagnostics.LoggingLevel.Warning);
                    }
                }
                catch
                {
                    Util.LoggingService.LogInfo("Got a weired broadcast from " + recievedFrom.Address, Windows.Foundation.Diagnostics.LoggingLevel.Warning);
                }
            }

            if (!udpSocket.ReceiveFromAsync(e))
            {
                ReceiveFromCallback(udpSocket, e);
            }
        }

        private void UDPBroadcast(object sender, object o)
        {
            var localEndPoints = new List<IPAddress>();

            foreach (var localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null && localHostName.Type == HostNameType.Ipv4 &&
                    localHostName.IPInformation.PrefixLength.HasValue)
                    //by making sure that there is a prefix, we will probably hit a local network
                {
                    var ipString = localHostName.CanonicalName;
                    var stringBytes = ipString.Split('.');
                    var localIPBytes = new byte[stringBytes.Length];
                    var broadcastIpBytes = new byte[stringBytes.Length];


                    for (var i = 0; i < stringBytes.Length; i++)
                    {
                        localIPBytes[i] = byte.Parse(stringBytes[i]);
                        broadcastIpBytes[i] = localIPBytes[i];

                        var maskLength = localHostName.IPInformation.PrefixLength.Value - 8*i;

                        if (maskLength > 0)
                        {
                            var bynaryMask = (byte) (255 >> maskLength);
                            broadcastIpBytes[i] = (byte) (broadcastIpBytes[i] | bynaryMask);
                        }
                        else
                            broadcastIpBytes[i] = 255;
                    }

                    localEndPoints.Add(new IPAddress(localIPBytes));
                    var broadcast = new IPAddress(broadcastIpBytes);

                    var sds = new SocketAsyncEventArgs {RemoteEndPoint = new IPEndPoint(broadcast, 44000)};
                    sds.SetBuffer(new byte[] {115, 101, 107, 114, 101, 116}, 0, 6);
                    //sekret - the message arduino reacts to 

                    _udpSocket.SendToAsync(sds);
                }
            }

            Interlocked.Exchange(ref _localEndPoints, localEndPoints);
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~UDPMessaging()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
    }
}
