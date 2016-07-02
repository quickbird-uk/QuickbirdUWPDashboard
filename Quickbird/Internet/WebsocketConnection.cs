using Quickbird.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ComponentModel;
using Newtonsoft.Json; 


namespace Quickbird.Internet
{

    /// <summary>
    /// This class is static, the instance of this class needs to be accessed somewhere, somehow, for websocket communication to activate! 
    /// </summary>
    public class WebSocketConnection : INotifyPropertyChanged
    {
        public static WebSocketConnection Instance { get; } = new WebSocketConnection();
        private static MessageWebSocket _webSocket = new MessageWebSocket(); //it is laso the subject of lock
        private static DataWriter _messageWriter;

        public event PropertyChangedEventHandler PropertyChanged;
        const string SocketCloseMessage = "AppIsSuspending"; 

        private static Timer _ReconnectTimer;
        int _reconnectionAttempt; //Used for exponenetial backoff timer 

        private static long _AppRunning = 1; //1 stands for running, 0 for suspended;  
 




        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _resumeAction;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _suspendAction;



        //Connected property, bindable directly from the UI
        private long _connected = 0;
        public bool Connected
        {
            get
            {
                return _connected == 1;
            }
        }


        public WebSocketConnection()
        {

            Debug.WriteLine("Websocket Starting");

            _resumeAction = Resume;
            _suspendAction = Suspend;
            Messenger.Instance.Suspending.Subscribe(_suspendAction);
            Messenger.Instance.Resuming.Subscribe(_resumeAction);

            //JsonConvert.DefaultSettings = () =>
            //{
            //    var settings = new JsonSerializerSettings();
            //    settings.Converters.Add(new SensorReadingsJsConverter());
            //    return settings;
            //};


            _ReconnectTimer = new Timer(Connect, null, 1000, Timeout.Infinite);
        }


        /// <summary>
        /// A best-effor sender of data
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(object toSend)
        {
            string jsonData = JsonConvert.SerializeObject(toSend);

            if (Interlocked.Read(ref _connected) == 1)
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
                    SocketClosed(); 
                    return false;
                }
            }
            else
                return false; 
        }

        private void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            
            if (SocketMessageType.Utf8 == args.MessageType)
            {
                try
                {
                    using (DataReader reader = args.GetDataReader())
                    {
                        reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                        string read = reader.ReadString(reader.UnconsumedBufferLength);
                        List<Messenger.SensorReading> sensorReadings = JsonConvert.DeserializeObject<List<Messenger.SensorReading>>(read); 
                        if(sensorReadings != null)
                        {
                            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues
                            Messenger.Instance.NewSensorDataPoint.Invoke(sensorReadings, true);
                            #pragma warning restore CS4014
                        }
                        Debug.WriteLine(read);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    SocketClosed(); 
                }                
            }
        }

        /// <summary>
        /// Overload for the Timer
        /// </summary>
        /// <param name="state">is null</param>
        private async void Connect(object state)
        {
            _ReconnectTimer.Dispose();
            bool success = await Connect();
        }

        private async Task<bool> Connect()
        {
            if (Settings.Instance.CredsSet == false)
                return false; 

            if (_webSocket == null)
                _webSocket = new MessageWebSocket(); 

            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.Closed += SocketClosed;
            _webSocket.MessageReceived += MessageRecieved;


            var tokenHeader = "X-ZUMO-AUTH";
            var creds = Creds.FromUserIdAndToken(Settings.Instance.CredUserId, Settings.Instance.CredToken);
            _webSocket.SetRequestHeader(tokenHeader, creds.Token);

            
            try
            {
                Uri uri = new Uri("wss://ghapi46azure.azurewebsites.net/api/Websocket");
                await _webSocket.ConnectAsync(uri);
                _messageWriter = new DataWriter(_webSocket.OutputStream);
            }
            catch
            {
                Debug.WriteLine("Websocket connection failed");
                SocketClosed();
                return false;
            }

            _reconnectionAttempt = 0;

            Interlocked.Exchange(ref _connected, 1);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));

            Debug.WriteLine("Websocket connected");
            return true;
        }



        /// <summary>
        /// These methids are linked to the messenger
        /// </summary>
        /// <param name="taskCompletionSource"></param>
        private void Resume(TaskCompletionSource<object> taskCompletionSource)
        {
            Interlocked.Exchange(ref _AppRunning, 1);
            _ReconnectTimer = new Timer(Connect, null, 0, Timeout.Infinite);
            taskCompletionSource.SetResult(null);
        }

        private void Suspend(TaskCompletionSource<object> taskCompletionSource)
        {
            Interlocked.Exchange(ref _AppRunning, 0);
            SocketClosed();
            taskCompletionSource.SetResult(null);
        }

        /// <summary>
        /// Unclear what hte hell this does
        /// </summary>
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

        /// <summary>
        /// Call after the connection is closed. Get ready for new connection
        /// </summary>
        private void SocketClosed()
        {
            _ReconnectTimer?.Dispose();
            Interlocked.Exchange(ref _connected, 0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));

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

            if(Interlocked.Read(ref _AppRunning) == 0)
            {
                //Don't need to do anything
            }
            else
            {              
                _reconnectionAttempt++;

                //Multiple should not exceed value of 6
                int timeMultiple = _reconnectionAttempt > 6 ? _reconnectionAttempt : 6;

                int reconDelay = (int)Math.Round(Math.Pow(_reconnectionAttempt, 2) * 1000);

                _ReconnectTimer = new Timer(Connect, null, reconDelay, Timeout.Infinite);
            }
        }

    }
}
