using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Agronomist
{

    public class Networking
    {
        private static uPLibrary.Networking.M2Mqtt.MqttBroker _mqttBroker = null;
        private static object _lock = new object(); 

        public Networking()
        {
            lock (_lock)
            {
                if (_mqttBroker == null)
                {               
                    _mqttBroker = new uPLibrary.Networking.M2Mqtt.MqttBroker();
                    _mqttBroker.Start(); 
                    _mqttBroker.MessagePublished += MqttMessageRecieved; 

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
