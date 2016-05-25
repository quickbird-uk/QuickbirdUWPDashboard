using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabasePOCOs.User;
using DatabasePOCOs;
using DatabasePOCOs.Global;
using Agronomist.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Windows.UI.Xaml;
using System.Threading;

namespace Agronomist.LocalNetworking
{
    public class DatapointsSaver
    {



        //Local Cache
        private List<SensorType> _sensorTypes = null; 
        private List<Device> _dbDevices;
        private List<SensorBuffer> _sensorBuffer = new List<SensorBuffer>();
        private List<SensorHistory> _sensorDays = new List<SensorHistory>();

        private List<KeyValuePair<Relay, List<RelayDatapoint>>> _relayBuffer = new List<KeyValuePair<Relay, List<RelayDatapoint>>>();
        
        //Flow management
        //private volatile int _pendingLoads= 1;
        private Task _localTask = null;
        private DispatcherTimer _saveTimer;
        private const int _saveIntervalSeconds = 120;
        private static DatapointsSaver _Instance = null;

        /// <summary>
        /// The date of yesturday with hours, minutes and seconds set to zero 
        /// </summary>
        public static DateTimeOffset Yesturday { get
            {
                DateTimeOffset yesturday = DateTimeOffset.Now.AddDays(-1);
                return yesturday.Subtract(yesturday.TimeOfDay);
            }
        }

        /// <summary>
        /// The date with hours, minutes and seconds set to zero 
        /// </summary>
        public static DateTimeOffset Today
        {
            get
            {
                DateTimeOffset today = DateTimeOffset.Now; 
                return today.Subtract(today.TimeOfDay);
            }
        }

        public static DateTimeOffset Tomorrow
        {
            get
            {
                DateTimeOffset tomorrow = DateTimeOffset.Now.AddDays(1);
                return tomorrow.Subtract(tomorrow.TimeOfDay);
            }
        }

        public DatapointsSaver()
        {
            if(_Instance == null)
            {
                _Instance = this;
                var factory = new TaskFactory(TaskCreationOptions.LongRunning,
                   TaskContinuationOptions.LongRunning);

               _localTask = factory.StartNew(() =>
               {
                   LoadData();
                   _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_saveIntervalSeconds) };
                   _saveTimer.Tick += SaveBufferedReadings;
                   _saveTimer.Start();
               }, TaskCreationOptions.LongRunning); 
            }

        }



        //TODO register this with an event in messenger class
        /// <summary>
        /// Don't make publich or call directly! always push onto the task! 
        /// </summary>
        /// <returns>true if it loaded something, false otherwise</returns>
        private void LoadData()
        {
            var db = new MainDbContext();
            _dbDevices = db.Devices.Include(dv => dv.Sensors).Include(dv => dv.Relays).AsNoTracking().ToList();
            List<SensorHistory> sensorsHistory = db.SensorHistory.Where(sh => sh.TimeStamp > Today).ToList();
            _sensorTypes = db.SensorTypes.Include(st => st.Param).Include(st => st.Place).ToList(); 

            //Add missing sensors and relays 
            foreach (Sensor sensor in _dbDevices.SelectMany(dv => dv.Sensors))
            {
                if (_sensorBuffer.Any(sb => sb.sensor.ID == sensor.ID) == false)
                {
                    _sensorBuffer.Add(new SensorBuffer(sensor));
                }
            }            
            foreach(SensorHistory sHistory in sensorsHistory)
            {
                var sensorBuffered = _sensorBuffer.First(sb => sb.sensor.ID == sHistory.SensorID); 
                if (sensorBuffered.dataDay == null || sensorBuffered.dataDay.TimeStamp < sHistory.TimeStamp)
                {
                    sHistory.DeserialiseData(); 
                    sensorBuffered.dataDay = sHistory; 
                }
            }


            foreach (Relay relay in _dbDevices.SelectMany(dv => dv.Relays))
            {
                if (_relayBuffer.Any(rb => rb.Key.ID == relay.ID) == false)
                {
                    _relayBuffer.Add(new KeyValuePair<Relay, List<RelayDatapoint>>(
                        relay, new List<RelayDatapoint>()));
                }
            }
            db.Dispose();
        }

        public void BufferReadings(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        {
            //Purposefull fire and forget
            _localTask.ContinueWith((Task previous) =>
            {
                LoadData();
                Device device = _dbDevices.FirstOrDefault(dv => dv.ID == values.Key);
                if (device == null)
                {
                    DeviceNotInDB();
                }
                else
                {
                    foreach (Manager.SensorMessage message in values.Value)
                    {
                        try
                        {
                            SensorBuffer sensorBuffer = _sensorBuffer.First(sb => sb.sensor.SensorTypeID == message.SensorTypeID);
                            TimeSpan duration = TimeSpan.FromMilliseconds((double)message.duration / 1000);
                            DateTimeOffset timeStamp = DateTimeOffset.Now;
                            SensorDatapoint datapoint = new SensorDatapoint(message.value, timeStamp, duration);
                            sensorBuffer.freshBuffer.Add(datapoint);
                        }
                        catch (ArgumentNullException)
                        {
                            //TODO add a nhew sensor to the device! 
                        }

                    }
                }

            }); 
        }

        //public void CreateDevice(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        //{
        //    MainDbContext db; 
        //    Device device = new Device
        //    {
        //        ID = Guid.NewGuid(),
        //        SerialNumber = values.Key,
        //        CreatedAt =DateTimeOffset.Now,
        //        UpdatedAt = DateTimeOffset.Now, 
        //        Deleted = false,
        //        l
        //    }

        //}

        //Closes sensor Histories that are no longer usefull
        private void SaveBufferedReadings(object sender, object e)
        {
            _localTask.ContinueWith((Task previous) =>
            {
                var db = new MainDbContext();
                Guid[] sensorIDs = _sensorBuffer.Select(sb => sb.sensor.ID).ToArray();

                for (int i = 0; i < _sensorBuffer.Count; i++)
                {
                    var sbuffer = _sensorBuffer[i];


                    SensorDatapoint sensorDatapoint = null; 

                    if (sbuffer.freshBuffer.Count > 0)
                    {
                        DateTimeOffset startTime = sbuffer.freshBuffer[0].TimeStamp;                     
                        DateTimeOffset endTime = sbuffer.freshBuffer.Last().TimeStamp;
                        TimeSpan duration = (endTime - startTime).Subtract(sbuffer.freshBuffer[0].Duration); 

                        TimeSpan cumulativeDuration = TimeSpan.Zero;
                        double cumulativeValue = 0; 

                        for (int b = 0; b < sbuffer.freshBuffer.Count; b++)
                        {
                            cumulativeDuration += sbuffer.freshBuffer[b].Duration;
                            cumulativeValue += sbuffer.freshBuffer[b].Value; 
                        }

                        SensorType sensorType = _sensorTypes.First(st => st.ID == sbuffer.sensor.SensorTypeID);
                        double value = cumulativeValue / sbuffer.freshBuffer.Count;

                        if (sensorType.ParamID == 5) // Level
                        {
                            sensorDatapoint = new SensorDatapoint(Math.Round(value), endTime, duration);
                        }
                        else if(sensorType.ParamID == 9) //water flow
                        {
                            sensorDatapoint = new SensorDatapoint(value, endTime, cumulativeDuration);
                        } 
                        else
                        {
                            sensorDatapoint = new SensorDatapoint(value, endTime, duration);
                        }

                        sbuffer.freshBuffer.RemoveRange(0, sbuffer.freshBuffer.Count); 
                    }

                    
                    //{ //save taht length of data
                    //    List<SensorDatapoint> dpBuffer = sbuffer.freshBuffer; 
                    //    DateTimeOffset startTime = dpBuffer[0].TimeStamp;

                    //    for(int t =0; startTime.AddSeconds(_saveIntervalSeconds) > dpBuffer[t]?.TimeStamp; t++) 
                    //}



                    //for(int b =0; b< sbuffer.freshBuffer.Count; b++)
                    //{

                        //}
                        //SensorDatapoint dataPoint = new SensorDatapoint()


                        ////Check if dataday need to be closed
                    if (sbuffer.dataDay != null && sbuffer.dataDay?.TimeStamp < Tomorrow)
                    {
                        sbuffer.dataDay = null; 
                    }
                    //Chekc if new data needs to be written
                    if (sbuffer.dataDay == null)
                    {
                        sbuffer.dataDay = new SensorHistory
                        {
                            LocationID = sbuffer.sensor.Device.LocationID,
                            SensorID = sbuffer.sensor.ID,
                            Sensor = sbuffer.sensor,
                            TimeStamp = Tomorrow,
                            Data = new List<SensorDatapoint>(),
                        };
                    }


                }

                    ////removes DataDays that don;t need to be there
                    //_sensorDays.RemoveAll(sh => sh.TimeStamp < Tomorrow);
                    ////Check if data day is no longer is same location as the device
                    //for(int i =0; i< _sensorBuffer.Count; i++)
                    //{

                    //}


                    ////Check if dataday needs to be created





                    ///foreach 
                    // Sensor sensors 
                    //SHIT! 
            }); 
        }



        //TODO make usefull
        private void DeviceNotInDB()
        {

        }

        private void HardwareChanged()
        {
            _localTask.ContinueWith((Task previous) =>
            {
                LoadData();
            });
         }



        private struct SensorBuffer
        {

            public SensorBuffer(Sensor assignSensor)
            {
                sensor = assignSensor;
                freshBuffer = new List<SensorDatapoint>();
                dataDay = null; 
            }
            public readonly Sensor sensor;
            public readonly List<SensorDatapoint> freshBuffer;
            public SensorHistory dataDay; 
        }

    }
}
