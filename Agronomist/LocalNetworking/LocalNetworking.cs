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

namespace Agronomist.LocalNetworking
{

    public class Manager
    {
        private static uPLibrary.Networking.M2Mqtt.MqttBroker _mqttBroker = null;
        private DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static object _lock = new object();

        private static UDPMessaging udpMessaging= null; 




        public Manager()
        {
            lock (_lock)
            {
                if (_mqttBroker == null)
                {               
                    _mqttBroker = new uPLibrary.Networking.M2Mqtt.MqttBroker();
                    _mqttBroker.Start(); 
                    _mqttBroker.MessagePublished += MqttMessageRecieved;
                    udpMessaging = new UDPMessaging(); 
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
                    if (rawData.Length % SensorMessage.length != 0)
                    {
                        Debug.WriteLine("message recieved over MQTT has incorrect length!");
                    }
                    else
                    {
                        int numberOfReadings = rawData.Length / SensorMessage.length;
                        readings = new SensorMessage[numberOfReadings];
                        //process each reading
                        for (int i = 0; i < numberOfReadings; i++)
                        {
                            readings[i].value = BitConverter.ToSingle(rawData, i * SensorMessage.length);
                            readings[i].duration = BitConverter.ToInt32(rawData, i * SensorMessage.length + 4);
                            readings[i].SensorTypeID = rawData[i * SensorMessage.length + 8];
                        }
                        Debug.WriteLine(readings.FirstOrDefault(r => r.SensorTypeID == 16).value.ToString());
                    }
                    Debug.WriteLine("we got a message!");

                }
            }
        }

     


        private struct SensorMessage
        {
            public float value;
            public Int32 duration;
            public byte SensorTypeID;
            public const int length = 9; 
        }


    }
}
