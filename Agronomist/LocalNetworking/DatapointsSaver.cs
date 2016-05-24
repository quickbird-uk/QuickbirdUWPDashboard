using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabasePOCOs.User;
using DatabasePOCOs;
using Agronomist.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;


namespace Agronomist.LocalNetworking
{
    public class DatapointsSaver
    {
        private List<Device> _dbDevices;
        private List<SensorBuffer> _sensorBuffer = new List<SensorBuffer>();
        private List<KeyValuePair<Relay, List<RelayDatapoint>>> _relayBuffer = new List<KeyValuePair<Relay, List<RelayDatapoint>>>(); 

        private static DatapointsSaver _Instance = null; 

        private volatile bool _intitialised= false; 

        public DatapointsSaver()
        {
            if(_Instance == null)
            {
                _Instance = this;
                LoadData(); //deal with te fact that it's async !                
            }
        }

        //TODO register this with an event in messenger class
        private async Task LoadData()
        {
            var db = new MainDbContext();
            _dbDevices = await db.Devices.Include(dv => dv.Sensors).Include(dv => dv.Relays).AsNoTracking().ToListAsync();

            //Add missing sensors and relays 
            foreach (Sensor sensor in _dbDevices.SelectMany(dv => dv.Sensors))
            {
                if (_sensorBuffer.Any(sb => sb.sensor.ID == sensor.ID) == false)
                {
                    _sensorBuffer.Add(new SensorBuffer(sensor));
                }
            }
            foreach(Relay relay in _dbDevices.SelectMany(dv => dv.Relays))
            {
                if (_relayBuffer.Any(rb => rb.Key.ID == relay.ID) == false)
                {
                    _relayBuffer.Add(new KeyValuePair<Relay, List<RelayDatapoint>>(
                        relay, new List<RelayDatapoint>())); 
                }
            }
            _intitialised = true;
            db.Dispose(); 
        }

        public void BufferReadings(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        {
            if(_intitialised)
            {
                Device device = _dbDevices.FirstOrDefault(dv => dv.ID == values.Key); 
                if(device == null)
                {
                    DeviceNotInDB(); 
                }
                else
                {
                    foreach(Manager.SensorMessage message in values.Value)
                    {
                        try
                        {
                            SensorBuffer sensorBuffer = _sensorBuffer.First(sb => sb.sensor.SensorTypeID == message.SensorTypeID);
                            TimeSpan duration = TimeSpan.FromMilliseconds((double) message.duration / 1000);
                            DateTimeOffset timeStamp = DateTimeOffset.Now; 
                            SensorDatapoint datapoint = new SensorDatapoint(message.value, timeStamp, duration);
                            sensorBuffer.buffer.Add(datapoint);
                        }
                        catch(ArgumentNullException)
                        {
                            //TODO add a nhew sensor to the device! 
                        }

                    }
                }
            }
        }


        //TODO make usefull
        private void DeviceNotInDB()
        {

        }

        //Todo make usefull, run on a timer
        private void SaveBufferedReadings()
        {

        }

        private struct SensorBuffer
        {

            public SensorBuffer(Sensor assignSensor)
            {
                sensor = assignSensor;
                buffer = new List<SensorDatapoint>(); 
            }
            public Sensor sensor;
            public List<SensorDatapoint> buffer; 
        }
    }
}
