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
                _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_saveIntervalSeconds) };
                _saveTimer.Tick += SaveBufferedReadings;
                _saveTimer.Start();

                _localTask = factory.StartNew(() =>
               {
                   LoadData();
               }, TaskCreationOptions.LongRunning); 
            }
            else
            {
                throw new Exception("You can't initialise mroe than one datapoint Saver");
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
            List<SensorHistory> sensorsHistory = db.SensorHistory.Where(sh => sh.TimeStamp > Today).ToList(); // we will edit this
            _sensorTypes = db.SensorTypes.Include(st => st.Param).Include(st => st.Place).AsNoTracking().ToList(); 

            //Add missing sensors and relays 
            foreach (Sensor sensor in _dbDevices.SelectMany(dv => dv.Sensors))
            {
                if (_sensorBuffer.Any(sb => sb.sensor.ID == sensor.ID) == false)
                {
                    _sensorBuffer.Add(new SensorBuffer(sensor));
                }
            }            //TODO merge the datapoints! 
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
                Device device = _dbDevices.FirstOrDefault(dv => dv.SerialNumber == values.Key);
                if (device == null)
                {
                    CreateDevice(values); 
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

        /// <summary>
        /// To be used internaly
        /// </summary>
        /// <param name="values"></param>
        private bool CreateDevice(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        {
            Settings settings = new Settings();
            if (settings.CredToken != null)
            {
                MainDbContext db = new MainDbContext();

                Device device = new Device
                {
                    ID = Guid.NewGuid(),
                    SerialNumber = values.Key,
                    Deleted = false,
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now,
                    Name = string.Format("Box Number {0}", _dbDevices.Count),
                    Relays = new List<Relay>(),
                    Sensors = new List<Sensor>(),
                    Version = new byte[32],
                    Location = new Location
                    {
                        ID = Guid.NewGuid(),
                        Deleted = false,
                        Name = string.Format("Box Number {0}", _dbDevices.Count),
                        PersonId = Guid.Parse(settings.CredUserId),
                        Version = new byte[32],
                        CropCycles = new List<CropCycle>(),
                        Devices = new List<Device>(),
                        RelayHistory = new List<RelayHistory>(),
                        SensorHistory = new List<SensorHistory>(),
                        CreatedAt = DateTimeOffset.Now,
                        UpdatedAt = DateTimeOffset.Now,
                    }
                };

                db.Devices.Add(device);
                //Add sensors
                foreach (var inSensors in values.Value)
                {
                    //Todo check correctness of hte sensorType
                    Sensor newSensor = new Sensor
                    {
                        CreatedAt = DateTimeOffset.Now,
                        UpdatedAt = DateTimeOffset.Now,
                        ID = Guid.NewGuid(),
                        DeviceID = device.ID,
                        Deleted = false,
                        SensorTypeID = inSensors.SensorTypeID,
                        Enabled = true,
                        Multiplier = 1,
                        Offset = 0,
                        Version = new byte[32]
                    };
                    db.Sensors.Add(newSensor);
                }
                db.SaveChanges();
                return true;
            }
            else
                return false; 
        }



        //Closes sensor Histories that are no longer usefull
        private void SaveBufferedReadings(object sender, object e)
        {
            _localTask.ContinueWith((Task previous) =>
            {
                var db = new MainDbContext();

                //loop for each sensorBuffer
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

                    //check if corresponding dataDay is too old and needsto be closed
                    if(sbuffer.dataDay?.TimeStamp < sensorDatapoint?.TimeStamp)
                    {
                        sbuffer.dataDay = null; 
                    }
                    
                    //if there is no suitable dat day, create it, but oinly if we have data to put there 
                    if (sbuffer.dataDay == null && sensorDatapoint != null)
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

                    //Make changes to the database
                    if(sensorDatapoint != null)
                    {
                        db.SensorHistory.Attach(sbuffer.dataDay);
                        sbuffer.dataDay.Data.Add(sensorDatapoint);
                        sbuffer.dataDay.SerialiseData();
                    }
                }
                //Once we are done here, mark changes to the db
                db.SaveChanges(); 
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
