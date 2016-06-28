namespace Agronomist.LocalNetworking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using Util;

    /// <summary>
    ///     This class is in charge of all the local communication. It initiates MQTT and UDP messaging.
    ///     IF you try to instantiate this class twice, it will throw and exception!
    /// </summary>
    public class Manager : IDisposable
    {
        private static MqttBroker _mqttBroker;

        private static readonly object _lock = new object();

        private static UDPMessaging _udpMessaging;
        private static DatapointsSaver _datapointsSaver;
        private DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private bool disposedValue; // To detect redundant calls

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _resumeAction;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _suspendAction;

        public Manager()
        {
            lock (_lock)
            {
                if (_mqttBroker == null)
                {
                    _mqttBroker = new MqttBroker();
                    _mqttBroker.Start();
                    _udpMessaging = new UDPMessaging();
                    _datapointsSaver = new DatapointsSaver();
                    _mqttBroker.MessagePublished += MqttMessageRecieved;

                    _resumeAction = Resume;
                    _suspendAction = Suspend;
                    Messenger.Instance.Suspending.Subscribe(_suspendAction);
                    Messenger.Instance.Resuming.Subscribe(_resumeAction);
                }
                else
                {
                    throw new Exception("You should only instantiate this class once! ");
                }
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        private void MqttMessageRecieved(KeyValuePair<string, MqttMsgPublish> publishEvent)
        {
            //TODO move this GUID parsing code somewhere appropriate!
            Guid clientID;
            var rawClientID = publishEvent.Key.Replace(":", string.Empty);
            if (Guid.TryParse(rawClientID, out clientID) == false)
            {
                Debug.WriteLine("recieved message with invalid ClientID");
            }
            else
            {
                SensorMessage[] readings;
                var message = publishEvent.Value;

                if (message.Topic.Contains("reading"))
                {
                    var rawData = message.Message;
                    if (rawData.Length%SensorMessage.incomingLength != 0)
                    {
                        Debug.WriteLine("message recieved over MQTT has incorrect length!");
                    }
                    else
                    {
                        var numberOfReadings = rawData.Length/SensorMessage.incomingLength;
                        readings = new SensorMessage[numberOfReadings];
                        //process each reading
                        for (var i = 0; i < numberOfReadings; i++)
                        {
                            readings[i].value = BitConverter.ToSingle(rawData, i*SensorMessage.incomingLength);
                            readings[i].duration = BitConverter.ToInt32(rawData, i*SensorMessage.incomingLength + 4);
                            readings[i].SensorTypeID = rawData[i*SensorMessage.incomingLength + 8];
                        }
                        var toWrite =
                            new KeyValuePair<Guid, SensorMessage[]>(clientID, readings);

                        _datapointsSaver.BufferAndSendReadings(toWrite);
                    }
                }
            }
        }


        private void Suspend(TaskCompletionSource<object> taskCompletionSource)
        {
            Debug.WriteLine("suspending manager");
            _mqttBroker?.Stop();
            _udpMessaging?.Dispose();
            _udpMessaging = null;
            taskCompletionSource.SetResult(null);
        }

        private void Resume(TaskCompletionSource<object> taskCompletionSource)
        {
            Debug.WriteLine("resuming manager");
            _mqttBroker?.Start();
            if (_udpMessaging == null || _udpMessaging.Disposed)
                _udpMessaging = new UDPMessaging();
            taskCompletionSource.SetResult(null);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects). 
                    _udpMessaging?.Dispose();
                    _datapointsSaver?.Dispose();
                }
                if (_mqttBroker != null)
                {
                    _mqttBroker.MessagePublished -= MqttMessageRecieved;
                    _mqttBroker.Stop();
                }
                _mqttBroker = null;
                _udpMessaging = null;
                _datapointsSaver = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Manager()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public struct SensorMessage
        {
            public float value;
            public int duration;
            public byte SensorTypeID;
            public const int incomingLength = 9;
        }
    }
}