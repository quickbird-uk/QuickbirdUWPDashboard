using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Agronomist.LocalNetworking; 

namespace Agronomist.LocalNetworking
{
    /// <summary>
    /// This class is in charge of all the local communication. It initiates MQTT and UDP messaging. 
    /// IF you try to instantiate this class twice, it will throw and exception! 
    /// </summary>
    public class Manager
    {

        private static uPLibrary.Networking.M2Mqtt.MqttBroker _mqttBroker = null;
        private DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static object _lock = new object();

        private static UDPMessaging _udpMessaging= null;
        private static DatapointsSaver _datapointsSaver = null; 




        public Manager()
        {
            lock (_lock)
            {
                if (_mqttBroker == null)
                {               
                    _mqttBroker = new uPLibrary.Networking.M2Mqtt.MqttBroker();
                    _mqttBroker.Start();
                    _udpMessaging = new UDPMessaging();
                    _datapointsSaver = new DatapointsSaver();
                    _mqttBroker.MessagePublished += MqttMessageRecieved;

                }
                else
                {
                    throw new Exception("You should onl;y instantiate this class once! "); 
                }
            }

        }

        private void MqttMessageRecieved(KeyValuePair<string, MqttMsgPublish> publishEvent)
        {
            //TODO move this GUID parsing code somewhere appropriate!
            Guid clientID;
            string rawClientID = publishEvent.Key.Replace(":", string.Empty);
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
                    byte[] rawData = message.Message;
                    if (rawData.Length % SensorMessage.incomingLength != 0)
                    {
                        Debug.WriteLine("message recieved over MQTT has incorrect length!");
                    }
                    else
                    {
                        int numberOfReadings = rawData.Length / SensorMessage.incomingLength;
                        readings = new SensorMessage[numberOfReadings];
                        //process each reading
                        for (int i = 0; i < numberOfReadings; i++)
                        {
                            readings[i].value = BitConverter.ToSingle(rawData, i * SensorMessage.incomingLength);
                            readings[i].duration = BitConverter.ToInt32(rawData, i * SensorMessage.incomingLength + 4);
                            readings[i].SensorTypeID = rawData[i * SensorMessage.incomingLength + 8];
                        }
                        KeyValuePair<Guid, SensorMessage[]> toWrite = 
                            new KeyValuePair<Guid, SensorMessage[]>(clientID, readings);

                        _datapointsSaver.BufferReadings(toWrite); 
                    }
                }
            }
        }

        public struct SensorMessage
        {
            public float value;
            public Int32 duration;
            public byte SensorTypeID;
            public const int incomingLength = 9;
        }




    }
}
