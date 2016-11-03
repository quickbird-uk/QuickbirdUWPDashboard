namespace Quickbird.Internet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    using Newtonsoft.Json;
    using Util;

    /// <summary>This class is static, the instance of this class needs to be accessed somewhere, somehow,
    /// for websocket communication to activate!</summary>
    public class WebSocketConnection : INotifyPropertyChanged
    {
        public enum ConnectionState
        {
            /// <summary>The websocket is not even started</summary>
            Stopped,

            /// <summary>Websocket is suspended because hte app is suspended</summary>
            Supended,

            /// <summary>it will try to reconnect shortly!</summary>
            WillTryConnect,

            /// <summary>It is actively attempting to connect</summary>
            Connecting,

            /// <summary>It is currently connected</summary>
            Connected,

            /// <summary>This state is used when suspend is called while the app is connecting. It will suspend as
            /// soon as it finishes connecting</summary>
            SuspendScheduled
        }

        private const string SocketCloseMessage = "AppIsSuspending";
#if LOCALSERVER
        private const string WebSocketStartUrl = "ws://localhost:53953/api/LiveData/Start";
#else
        private const string WebSocketStartUrl = "wss://.azurewebsites.net/api/LiveData/Start";
#endif
        private static MessageWebSocket _webSocket; //it is laso the subject of lock
        private static DataWriter _messageWriter;
        private static Timer _ReconnectTimer;
        private readonly object StoppingLock = new object();
        private long _connectionState;
        private int _reconnectionAttempt; //Used for exponenetial backoff timer


        public WebSocketConnection()
        {
            Debug.WriteLine("Websocket Starting");

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new SensorReadingsJsConverter());
                return settings;
            };
        }

        /// <summary>Property observable from the UI about state of the conection</summary>
        public ConnectionState Connected { get { return (ConnectionState) Interlocked.Read(ref _connectionState); } }

        /// NetWorking
        public static WebSocketConnection Instance { get; } = new WebSocketConnection();

        public event PropertyChangedEventHandler PropertyChanged;

#region StatefullMethod

        /// <summary>Returns true if the method is successfully started. Returns false if it's already started
        /// or it's not</summary>
        /// <returns></returns>
        public bool TryStart()
        {
            //only act if websocket is turned off
            if (
                (ConnectionState)
                Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.WillTryConnect,
                    (long) ConnectionState.Stopped) == ConnectionState.Stopped)
            {
                //Use is no signed in, return
                if (Settings.Instance.IsLoggedIn == false)
                    return false;

                _ReconnectTimer = new Timer(TimerTick, null, 1000, Timeout.Infinite);

                return true;
            }
            //it was already enabled!
            return false;
        }


        /// <summary>Not implemented</summary>
        /// <returns></returns>
        public void Stop()
        {
            lock (StoppingLock)
            {
                if ((ConnectionState) Interlocked.Read(ref _connectionState) == ConnectionState.Stopped)
                    return;
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Stopped,
                        (long) ConnectionState.Supended) != ConnectionState.Supended)
                {
                    var completion = new TaskCompletionSource<object>();

                    Suspend();

                    while (
                        (ConnectionState)
                        Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Stopped,
                            (long) ConnectionState.Supended) != ConnectionState.Supended)
                    {
                    }
                }
            }
        }

        /// <summary>A best-effor sender of data</summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(object toSend)
        {
            var jsonData = JsonConvert.SerializeObject(toSend);

            if ((ConnectionState) Interlocked.Read(ref _connectionState) == ConnectionState.Connected)
            {
                try
                {
                    // Send the data as one complete message.
                    _messageWriter.WriteString(jsonData);
                    await _messageWriter.StoreAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    CleanUp();
                    return false;
                }
            }
            return false;
        }

        private void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            if (SocketMessageType.Utf8 == args.MessageType)
            {
                try
                {
                    using (var reader = args.GetDataReader())
                    {
                        reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                        var read = reader.ReadString(reader.UnconsumedBufferLength);
                        var sensorReadings = JsonConvert.DeserializeObject<List<Messenger.SensorReading>>(read);
                        if (sensorReadings != null)
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues
                            Messenger.Instance.NewSensorDataPoint.Invoke(sensorReadings, true);
#pragma warning restore CS4014
                        }
                        //Debug.WriteLine(read);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    //If it's not inc connected state, then probably someone is already cleaning it up
                    if (
                        (ConnectionState)
                        Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.WillTryConnect,
                            (long) ConnectionState.Connected) == ConnectionState.Connected)
                    {
                        CleanUp();
                        ScheduleReconnection(false);
                    }
                }
            }
        }

        /// <summary>Overload for the Timer</summary>
        /// <param name="state">is null</param>
        private async void TimerTick(object state)
        {
            if (
                (ConnectionState)
                Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Connecting,
                    (long) ConnectionState.WillTryConnect) == ConnectionState.WillTryConnect)
                _ReconnectTimer.Dispose();

            var success = await TryConnect();
            if (success)
            {
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Connected,
                        (long) ConnectionState.Connecting) == ConnectionState.Connecting)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
                }
            }
            else
            {
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.WillTryConnect,
                        (long) ConnectionState.Connecting) == ConnectionState.Connecting)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
                    ScheduleReconnection(false);
                }
            }

            //This should be the very last line in the method, to check if suspend was scheduled while we were performing connection
            if (
                (ConnectionState)
                Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Supended,
                    (long) ConnectionState.SuspendScheduled) == ConnectionState.SuspendScheduled)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
                CleanUp();
            }
        }

        /// <summary>These methids are linked to the messenger</summary>
        /// <param name="taskCompletionSource"></param>
        public void Resume()
        {
            do
            {
                //Only resume if the previous state was suspended
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Supended,
                        (long) ConnectionState.WillTryConnect) == ConnectionState.Supended)
                {
                    ScheduleReconnection(true);
                }
            } while ((ConnectionState) Interlocked.Read(ref _connectionState) == ConnectionState.SuspendScheduled);
        }

        public void Suspend()
        {
            while (true)
            {
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Supended,
                        (long) ConnectionState.WillTryConnect) == ConnectionState.WillTryConnect)
                {
                    _ReconnectTimer.Dispose();
                    break;
                }
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.Supended,
                        (long) ConnectionState.Connected) == ConnectionState.Connected)
                {
                    //TODO: cleanup Connection
                    CleanUp();
                    break;
                }
                if (
                    (ConnectionState)
                    Interlocked.CompareExchange(ref _connectionState, (long) ConnectionState.SuspendScheduled,
                        (long) ConnectionState.Connecting) == ConnectionState.Connecting)
                {
                    //Socket is currently connecting, set the flag and it will cleanup
                    break;
                }
                if ((ConnectionState) Interlocked.Read(ref _connectionState) == ConnectionState.Stopped)
                {
                    break;
                }
            }
        }

#endregion

#region State-Ignoring methods

        private async Task<bool> TryConnect()
        {
            if (Settings.Instance.IsLoggedIn == false)
                return false;

            _webSocket = new MessageWebSocket();
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.Closed += SocketClosed;
            _webSocket.MessageReceived += MessageRecieved;

            var tokenHeader = "Authorization";
            var token = Settings.Instance.Token;
            _webSocket.SetRequestHeader(tokenHeader, "Bearer " + token);
            try
            {
                var uri = new Uri(WebSocketStartUrl);
                await _webSocket.ConnectAsync(uri);
                _messageWriter = new DataWriter(_webSocket.OutputStream);
                _reconnectionAttempt = 0;
                Debug.WriteLine("Websocket connected");
                return true;
            }
            catch
            {
                Debug.WriteLine("Websocket connection failed");
                _reconnectionAttempt++;
                CleanUp();
                return false;
            }
        }

        /// <summary>Unclear what hte hell this does</summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            if (args.Reason == SocketCloseMessage)
            {
                //means the app closed it, but so what?
            }

            //SocketClosed();
        }


        private void ScheduleReconnection(bool immediately)
        {
            if (immediately)
                _ReconnectTimer = new Timer(TimerTick, null, 1000, Timeout.Infinite);
            else
            {
                //Multiple should not exceed value of 6
                var timeMultiple = _reconnectionAttempt > 6 ? _reconnectionAttempt : 6;

                var reconDelay = (int) Math.Round(Math.Pow(_reconnectionAttempt, 2)*1000);

                _ReconnectTimer = new Timer(TimerTick, null, reconDelay, Timeout.Infinite);

                Interlocked.Exchange(ref _connectionState, (long) ConnectionState.WillTryConnect);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
            }
        }

        /// <summary>Call after the connection is closed. Get ready for new connection</summary>
        private void CleanUp()
        {
            _ReconnectTimer?.Dispose();

            try
            {
                _webSocket?.Close(1000, SocketCloseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            _messageWriter?.DetachStream();
            _messageWriter?.Dispose();
            _messageWriter = null;

            _webSocket?.Dispose();
            _webSocket = null;
        }

#endregion
    }
}
