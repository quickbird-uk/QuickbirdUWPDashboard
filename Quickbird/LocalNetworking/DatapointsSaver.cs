namespace Quickbird.LocalNetworking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using DbStructure;
    using DbStructure.Global;
    using DbStructure.User;
    using Internet;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;

    public class DatapointsSaver : IDisposable
    {
        private const int _saveIntervalSeconds = 60;
        private static DatapointsSaver _Instance;
        private readonly Action<string> _onHardwareChanged;

        private readonly List<SensorBuffer> _sensorBuffer = new List<SensorBuffer>();
        private List<Device> _dbDevices;

        //Flow management
        //private volatile int _pendingLoads= 1;
        private Task _localTask;

        private List<KeyValuePair<Relay, List<RelayDatapoint>>> _relayBuffer =
            new List<KeyValuePair<Relay, List<RelayDatapoint>>>();

        private DispatcherTimer _saveTimer;
        private List<SensorHistory> _sensorDays = new List<SensorHistory>();
        //Local Cache
        private List<SensorType> _sensorTypes;

        public DatapointsSaver()
        {
            if (_Instance == null)
            {
                _Instance = this;

                Task.Run(() => ((App) Application.Current).Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _saveTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(_saveIntervalSeconds)};
                    _saveTimer.Tick += SaveBufferedReadings;
                    _saveTimer.Start();
                }));

                _onHardwareChanged = HardwareChanged;
                Messenger.Instance.TablesChanged.Subscribe(_onHardwareChanged);

                _localTask = Task.Run(() => { LoadData(); });
            }
            else
            {
                throw new Exception("You can't initialise more than one datapoint Saver");
            }
        }

        /// <summary>The date with hours, minutes and seconds set to zero</summary>
        public static DateTimeOffset Today
        {
            get
            {
                var today = DateTimeOffset.Now;
                return today.Subtract(today.TimeOfDay);
            }
        }

        public static DateTimeOffset Tomorrow
        {
            get
            {
                var tomorrow = DateTimeOffset.Now.AddDays(1);
                return tomorrow.Subtract(tomorrow.TimeOfDay);
            }
        }

        /// <summary>The date of yesturday with hours, minutes and seconds set to zero</summary>
        public static DateTimeOffset Yesturday
        {
            get
            {
                var yesturday = DateTimeOffset.Now.AddDays(-1);
                return yesturday.Subtract(yesturday.TimeOfDay);
            }
        }

        public void Dispose()
        {
            BlockingDispatcher.Run(() => _saveTimer?.Stop());
            _Instance = null;
        }

        public void BufferAndSendReadings(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        {
            //Purposefull fire and forget
            _localTask = _localTask.ContinueWith(previous =>
            {
                var device = _dbDevices.FirstOrDefault(dv => dv.SerialNumber == values.Key);

                if (device == null)
                {
                    CreateDevice(values);
                }
                else
                {
                    var sensorReadings = new List<Messenger.SensorReading>();
                    foreach (var message in values.Value)
                    {
                        try
                        {
                            var sensorBuffer = _sensorBuffer.First(sb => sb.sensor.SensorTypeID == message.SensorTypeID);
                            var duration = TimeSpan.FromMilliseconds((double) message.duration/1000);
                            var timeStamp = DateTimeOffset.Now;
                            var datapoint = new SensorDatapoint(message.value, timeStamp, duration);
                            sensorBuffer.freshBuffer.Add(datapoint);
                            var sensorReading = new Messenger.SensorReading(sensorBuffer.sensor.ID, datapoint.Value,
                                datapoint.TimeStamp, datapoint.Duration);
                            sensorReadings.Add(sensorReading);
                        }
                        catch (ArgumentNullException)
                        {
                            //TODO add a new sensor to the device! 
                        }
                    }


                    //this is meant to be fire-forget, that's cool 
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    WebSocketConnection.Instance.SendAsync(sensorReadings);
                    Messenger.Instance.NewSensorDataPoint.Invoke(sensorReadings);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            });
        }

        public void Resume()
        {
            Debug.WriteLine("resuming datasaver");
            BlockingDispatcher.Run(() => _saveTimer?.Start());
        }


        public void Suspend()
        {
            Debug.WriteLine("suspending datasaver");
            BlockingDispatcher.Run(() => _saveTimer?.Stop());
        }

        /// <summary>To be used internaly</summary>
        /// <param name="values"></param>
        private bool CreateDevice(KeyValuePair<Guid, Manager.SensorMessage[]> values)
        {
            //Make sure that if fired several times, the constraints are maintained

            var settings = Settings.Instance;

            if (settings.CredsSet && settings.LastDatabaseDownload != default(DateTimeOffset) &&
                _dbDevices.Any(dev => dev.SerialNumber == values.Key) == false)
            {
                Debug.WriteLine("addingDevice");
                var db = new MainDbContext();

                var device = new Device
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
                    Location =
                        new Location
                        {
                            ID = Guid.NewGuid(),
                            Deleted = false,
                            Name = string.Format("Box Number {0}", _dbDevices.Count),
                            PersonId = settings.CredStableSid, //TODO use the thing from settings! 
                            Version = new byte[32],
                            CropCycles = new List<CropCycle>(),
                            Devices = new List<Device>(),
                            RelayHistory = new List<RelayHistory>(),
                            SensorHistory = new List<SensorHistory>(),
                            CreatedAt = DateTimeOffset.Now,
                            UpdatedAt = DateTimeOffset.Now
                        }
                };

                db.Devices.Add(device);
                db.Locations.Add(device.Location);
                //Add sensors
                foreach (var inSensors in values.Value)
                {
                    //Todo check correctness of hte sensorType
                    var newSensor = new Sensor
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
                    device.Sensors.Add(newSensor);
                }
                db.SaveChanges();

                //Add the device to the cached data? 
                _dbDevices.Add(device);
                foreach (var sensor in device.Sensors)
                {
                    _sensorBuffer.Add(new SensorBuffer(sensor));
                }
                db.Dispose();

                return true;
            }
            return false;
        }


        //TODO make usefull
        private void DeviceNotInDB() { }

        private void HardwareChanged(string value)
        {
            _localTask = _localTask.ContinueWith(previous => { LoadData(); });
        }


        //TODO register this with an event in messenger class
        /// <summary>Don't make publich or call directly! always push onto the task!</summary>
        /// <returns>true if it loaded something, false otherwise</returns>
        private void LoadData()
        {
            Debug.WriteLine("DatapointsSaver refreshing cache");
            var db = new MainDbContext();
            _dbDevices = db.Devices.Include(dv => dv.Sensors).Include(dv => dv.Relays).AsNoTracking().ToList();
            var sensorsHistory = db.SensorsHistory.Where(sh => sh.TimeStamp > Today).ToList(); // we will edit this
            _sensorTypes = db.SensorTypes.Include(st => st.Param).Include(st => st.Place).AsNoTracking().ToList();

            //Add missing sensors and relays 
            foreach (var sensor in _dbDevices.SelectMany(dv => dv.Sensors))
            {
                if (_sensorBuffer.Any(sb => sb.sensor.ID == sensor.ID) == false)
                {
                    _sensorBuffer.Add(new SensorBuffer(sensor));
                }
            } //TODO merge the datapoints! 
            foreach (var sHistory in sensorsHistory)
            {
                sHistory.DeserialiseData();
                var mIndex = _sensorBuffer.FindIndex(sb => sb.sensor.ID == sHistory.SensorID);
                if (_sensorBuffer[mIndex].dataDay == null ||
                    _sensorBuffer[mIndex].dataDay.TimeStamp < sHistory.TimeStamp)
                {
                    _sensorBuffer[mIndex] = new SensorBuffer(_sensorBuffer[mIndex].sensor, sHistory);
                }
                else if (_sensorBuffer[mIndex].dataDay.Data != null && sHistory.Data != null)
                {
                    var sHistMerged = SensorHistory.Merge(_sensorBuffer[mIndex].dataDay, sHistory);
                    _sensorBuffer[mIndex] = new SensorBuffer(_sensorBuffer[mIndex].sensor, sHistMerged);
                }
            }


            //foreach (Relay relay in _dbDevices.SelectMany(dv => dv.Relays))
            //{
            //    if (_relayBuffer.Any(rb => rb.Key.ID == relay.ID) == false)
            //    {
            //        _relayBuffer.Add(new KeyValuePair<Relay, List<RelayDatapoint>>(
            //            relay, new List<RelayDatapoint>()));
            //    }
            //}
            db.Dispose();
        }


        //Closes sensor Histories that are no longer usefull
        private void SaveBufferedReadings(object sender, object e)
        {
            Toast.Debug("SaveBufferedReadings", $"{DateTimeOffset.Now.DateTime} Datapointsaver");
            _localTask = _localTask.ContinueWith(previous =>
            {
                var settings = Settings.Instance;

                if (settings.LastDatabaseDownload > DateTimeOffset.Now - TimeSpan.FromMinutes(1))
                {
                    Debug.WriteLine("Datasaver started, recent update detected.");

                    var db = new MainDbContext();
                    // List<SensorHistory> updatedSensorHistories = new List<SensorHistory>(); 

                    //loop for each sensorBuffer
                    for (var i = 0; i < _sensorBuffer.Count; i++)
                    {
                        var sbuffer = _sensorBuffer[i];


                        SensorDatapoint sensorDatapoint = null;

                        if (sbuffer.freshBuffer.Count > 0)
                        {
                            var startTime = sbuffer.freshBuffer[0].TimeStamp;
                            var endTime = sbuffer.freshBuffer.Last().TimeStamp;
                            var duration = (endTime - startTime).Subtract(sbuffer.freshBuffer[0].Duration);

                            var cumulativeDuration = TimeSpan.Zero;
                            double cumulativeValue = 0;

                            for (var b = 0; b < sbuffer.freshBuffer.Count; b++)
                            {
                                cumulativeDuration += sbuffer.freshBuffer[b].Duration;
                                cumulativeValue += sbuffer.freshBuffer[b].Value;
                            }

                            var sensorType = _sensorTypes.First(st => st.ID == sbuffer.sensor.SensorTypeID);
                            var value = cumulativeValue/sbuffer.freshBuffer.Count;

                            if (sensorType.ParamID == 5) // Level
                            {
                                sensorDatapoint = new SensorDatapoint(Math.Round(value), endTime, duration);
                            }
                            else if (sensorType.ParamID == 9) //water flow
                            {
                                sensorDatapoint = new SensorDatapoint(value, endTime, cumulativeDuration);
                            }
                            else
                            {
                                sensorDatapoint = new SensorDatapoint(value, endTime, duration);
                            }

                            sbuffer.freshBuffer.RemoveRange(0, sbuffer.freshBuffer.Count);
                        }
                        //only if new data is present
                        if (sensorDatapoint != null)
                        {
                            //check if corresponding dataDay is too old or none exists at all 
                            if (sbuffer.dataDay?.TimeStamp < sensorDatapoint?.TimeStamp || sbuffer.dataDay == null)
                            {
                                var dataDay = new SensorHistory
                                {
                                    LocationID = sbuffer.sensor.Device.LocationID,
                                    SensorID = sbuffer.sensor.ID,
                                    Sensor = sbuffer.sensor,
                                    TimeStamp = Tomorrow,
                                    Data = new List<SensorDatapoint>()
                                };
                                _sensorBuffer[i] = new SensorBuffer(sbuffer.sensor, dataDay);
                                //Only uses this entity, and does not follow the references to stick related references in the DB  
                                db.Entry(dataDay).State = EntityState.Added;
                            }
                            else
                            {
                                //this will not attach related entities, which is good 
                                db.Entry(sbuffer.dataDay).State = EntityState.Unchanged;
                            }

                            _sensorBuffer[i].dataDay.Data.Add(sensorDatapoint);
                            _sensorBuffer[i].dataDay.SerialiseData();
                        }
                    } //for loop ends 
                    //Once we are done here, mark changes to the db
                    db.SaveChanges();
                    Debug.WriteLine("Saved Sensor Data");
                    db.Dispose();
                }
                else
                {
                    Debug.WriteLine("Skipped datasaver due to lack of recent update.");
                }
            });
        }


        private class SensorBuffer
        {
            public readonly SensorHistory dataDay;
            public readonly List<SensorDatapoint> freshBuffer;
            public readonly Sensor sensor;

            public SensorBuffer(Sensor assignSensor, SensorHistory inDataDay = null)
            {
                sensor = assignSensor;
                freshBuffer = new List<SensorDatapoint>();
                dataDay = inDataDay;
            }
        }
    }
}
