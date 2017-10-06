namespace Quickbird.LocalNetworking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using Newtonsoft;

    public struct SensorMessage
    {
        public float value;
        public int duration;
        public byte SensorTypeID;

        ///Binarry Message Length. 
        public const int incomingLength = 9;
    }

    /// <summary>This class is in charge of all the local communication. It initiates MQTT and UDP
    /// messaging. IF you try to instantiate this class twice, it will throw and exception!</summary>
    public class LANManager : IDisposable
    {
        private static MqttBroker _mqttBroker;

        private static readonly object _lock = new object();

        private static UDPMessaging _udpMessaging;
        private DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private bool disposedValue; // To detect redundant calls

        public LANManager()
        {
            lock (_lock)
            {
                if (_mqttBroker == null)
                {
                    _mqttBroker = new MqttBroker();
                    _mqttBroker.Start();
                    _udpMessaging = new UDPMessaging();
                    _mqttBroker.MessagePublished += MqttMessageRecieved;
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

        public void Resume()
        {
            Debug.WriteLine("resuming manager");
            _mqttBroker?.Start();
            if (_udpMessaging == null || _udpMessaging.Disposed)
                _udpMessaging = new UDPMessaging();
        }


        public void Suspend()
        {
            Debug.WriteLine("suspending manager");
            _mqttBroker?.Stop();
            _udpMessaging?.Dispose();
            _udpMessaging = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects). 
                    _udpMessaging?.Dispose();
                }
                if (_mqttBroker != null)
                {
                    _mqttBroker.MessagePublished -= MqttMessageRecieved;
                    _mqttBroker.Stop();
                }
                _mqttBroker = null;
                _udpMessaging = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        private void MqttMessageRecieved(KeyValuePair<string, MqttMsgPublish> publishEvent)
        {
            //TODO move this GUID parsing code somewhere appropriate!
            Guid clientID;
            var rawClientID = publishEvent.Key.Replace(":", string.Empty);

            if (Guid.TryParse(rawClientID, out clientID) == false)
            {
                Util.LoggingService.LogInfo($"Recieved message with invalid ClientID, {clientID} is not a valid ID", Windows.Foundation.Diagnostics.LoggingLevel.Error);
                return;
            }

            SensorMessage[] readings = null;
            var message = publishEvent.Value;
            string prefix = message.Topic.Substring(0, 8); 

            if (prefix.CompareTo("readings") != 0)
                return;

            else if (message.Topic.CompareTo("readings/v1/binary") == 0)
                readings = DecodeBinaryMessage(message.Message);
            else if (message.Topic.CompareTo("readings/v1/JSON") == 0)
                readings = DecodeJSONMessage(message.Message); 

            var toWrite = new KeyValuePair<Guid, SensorMessage[]>(clientID, readings);

            if (readings != null)
                DatapointService.Instance.BufferAndSendReadings(toWrite, "Box");
        }

        private static SensorMessage[] DecodeBinaryMessage(byte[] rawData)
        {
            SensorMessage[] readings = null;

            if (rawData.Length % SensorMessage.incomingLength != 0)
            {
                Util.LoggingService.LogInfo($"Binary message recieved over MQTT is invalid. it must consist of sensor readings, " +
                    $"{SensorMessage.incomingLength} bytes long each. It's length is {rawData.Length}, not divisble by {SensorMessage.incomingLength}"
                    , Windows.Foundation.Diagnostics.LoggingLevel.Error);
            }
            else
            {
                var numberOfReadings = rawData.Length / SensorMessage.incomingLength;
                readings = new SensorMessage[numberOfReadings];
                //process each reading
                for (var i = 0; i < numberOfReadings; i++)
                {
                    readings[i].value = BitConverter.ToSingle(rawData, i * SensorMessage.incomingLength);
                    readings[i].duration = BitConverter.ToInt32(rawData, i * SensorMessage.incomingLength + 4);
                    readings[i].SensorTypeID = rawData[i * SensorMessage.incomingLength + 8];
                }
            }

            return readings; 
        }

        private static SensorMessage[] DecodeJSONMessage(byte[] rawData)
        {
            SensorMessage[] message = null; 
            try
            {
                string messageSent = System.Text.Encoding.UTF8.GetString(rawData);
                message = Newtonsoft.Json.JsonConvert.DeserializeObject<SensorMessage[]>(messageSent);
            }
            catch
            {
                Util.LoggingService.LogInfo($"Message recieved over MQTT is not valid JSON. it must consist of sensor readings", Windows.Foundation.Diagnostics.LoggingLevel.Error);
            }
            return message;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~LANManager()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
    }
}
