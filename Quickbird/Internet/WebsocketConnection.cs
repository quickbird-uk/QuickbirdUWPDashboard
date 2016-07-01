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
    public class WebsocketConnection : INotifyPropertyChanged
    {
        public static WebsocketConnection Instance { get; } = new WebsocketConnection();
        private static MessageWebSocket _webSocket = new MessageWebSocket(); //it is laso the subject of lock
        private static DataWriter messageWriter;

        public event PropertyChangedEventHandler PropertyChanged;




        Task _task;

        uint reconnectionAttempt; //Used for exponenetial backoff timer 

        public WebsocketConnection(){

            Debug.WriteLine("WebsocketStarting"); 
            _task = Task.Run(Connect);
            _task.ContinueWith(async (Task it) =>
            { await SendAsync("Test 1!!"); }); 
        }


        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            Connected = false;
            CleanUp(); 
        }


        private async Task<bool> SendAsync(string toSend)
        {
            //string jsonData = JsonConvert.SerializeObject(toSend);
            messageWriter.WriteString(toSend);
            try
            {
                // Send the data as one complete message.
                await messageWriter.StoreAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true; 
        }

        private void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                try
                {
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    Debug.WriteLine(read);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async Task Connect()
        {
            if (_webSocket == null)
                _webSocket = new MessageWebSocket(); 

            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += MessageRecieved;
            _webSocket.Closed += SocketClosed;

            if (Settings.Instance.CredsSet)
            {
                try
                {
                    Uri uri = new Uri("wss://ghapi46azure.azurewebsites.net/api/Websocket?username=bob");
                    await _webSocket.ConnectAsync(uri);
                    messageWriter = new DataWriter(_webSocket.OutputStream);
                    Connected = true; 
                }
                catch
                {
                    CleanUp(); 
                }
            }            
        }

        public void Close()
        {
            Connected = false; 

            try
            {
                _webSocket?.Close(1000, "Closed due to user request.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            CleanUp(); 
        }

        /// <summary>
        /// Call after the connection is closed. Get ready for new connection
        /// </summary>
        private void CleanUp()
        {
            messageWriter?.DetachStream();
            messageWriter?.Dispose();
            messageWriter = null;

            _webSocket?.Dispose();
            _webSocket = null;
        }

        //Connected property, bindable directly from the UI
        private bool _connected = false;
        public bool Connected {
            get
            {
                return _connected;
            }
            set
            {
                _connected = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
            }
        }
    }
}
