using Quickbird.LocalNetworking;
using Quickbird.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quickbird.Services
{

    /// <summary>
    /// Thread-safe clas that can start and stop publishing of virtual data.
    /// Static - only one virtual device from each machine. 
    /// </summary>
    public static class VirtualDeviceService
    {
        private static Timer _PublishTimer = new Timer(Publish, null, publishInterval, publishInterval);

        public const int publishInterval = 300; //in milliseconds
        private static volatile bool _Active = false; 
        private static Guid _DeviceID = Guid.Empty;

        private static object _Lock = new object();

        private static SensorMessage[] _PrevDatapoints = null;

        private static Random _random = new Random(); 

        public static bool Active => _Active;

        public static bool UpdateBasedONSettings()
        {
            if (SettingsService.Instance.VirtualDeviceEnabled == false)
            {
                Stop();
                return false;
            }
            else
            {
                Guid deviceID = SettingsService.Instance.MachineID;
                Start(deviceID);
                return true; 
            }

        }

        /// <summary>
        /// Starts the service with the spesified deviceID. 
        /// </summary>
        /// <param name="deviceId">Guid of the device that will </param>
        public static void Start(Guid deviceId)
        {
            lock(_Lock)
            {
                _Active = true; 
                _DeviceID = deviceId; 
            }
        }

        public static void Stop()
        {
            lock (_Lock)
            {
                _Active = false;
                _DeviceID = Guid.Empty; 
            }
        }

        private static void Publish(object state)
        {
            bool active;
            Guid deviceGuid; 
            lock (_Lock)
            {
                active = _Active;
                deviceGuid = _DeviceID;
            }

            if (active == false)
                return;

            int NumOfSensors = 8; 
            if (_PrevDatapoints == null)
            { _PrevDatapoints = new SensorMessage[NumOfSensors]; }

            SensorMessage[] datapoints =  new SensorMessage[NumOfSensors];
            datapoints[0].SensorTypeID = 5; //Air Humidity
            datapoints[1].SensorTypeID = 6; //Air Temperature
            datapoints[2].SensorTypeID = 16; //WaterFlow
            datapoints[3].SensorTypeID = 19; //WaterLevel
            datapoints[4].SensorTypeID = 8; // WaterTemperature
            datapoints[5].SensorTypeID = 4; // Water PH
            datapoints[6].SensorTypeID = 13; // Water Conductivity
            datapoints[7].SensorTypeID = 11; // Ambient Light

            

            datapoints[0].value = RandomFluctuation(_PrevDatapoints[0].value, 3, 100, 0);
            datapoints[1].value = RandomFluctuation(_PrevDatapoints[1].value, 3, 50, -30);
            datapoints[2].value = RandomFluctuation(_PrevDatapoints[2].value, 1, 10, 0);
            datapoints[3].value = RandomFluctuation(_PrevDatapoints[3].value, 0.5f, 100, 0);
            datapoints[4].value = RandomFluctuation(_PrevDatapoints[4].value, 0.5f, 100, 0);
            datapoints[5].value = RandomFluctuation(_PrevDatapoints[5].value, 0.1f, 12, 2);
            datapoints[6].value = RandomFluctuation(_PrevDatapoints[6].value, 0.1f, 6, 0);
            datapoints[7].value = RandomFluctuation(_PrevDatapoints[7].value, 10, 100, 0);

            for (int i = 0; i < datapoints.Length; i++)
            {
                datapoints[i].duration = publishInterval;
                _PrevDatapoints[i].value = datapoints[i].value;
            }

            var KeyValue = new KeyValuePair<Guid, SensorMessage[]>(deviceGuid, datapoints);
            DatapointService.Instance.BufferAndSendReadings(KeyValue, $"Virtual device"); 
        }

        public static float RandomFluctuation(float currentvalue, float fluctuationSize, float upperLimit, float lowerLimit)
        {
            if (lowerLimit >= upperLimit)
                throw new Exception($"Error in {nameof(VirtualDeviceService)}:{nameof(RandomFluctuation)} - Upper and Lower limit are set incorrectly");

            if (currentvalue > upperLimit || currentvalue < lowerLimit)
                currentvalue = (upperLimit + lowerLimit) / 2;

            float change = (float)((_random.NextDouble() - 0.5) * fluctuationSize);
            float nextValue = currentvalue + change;

            if (nextValue > upperLimit || nextValue < lowerLimit)
                nextValue = currentvalue - change;

            return nextValue;
        }
    }
}
