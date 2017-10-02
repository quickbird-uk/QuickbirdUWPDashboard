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

    /// <summary>Non-threadsafe singleton manager of the saving of datapoints.</summary>
    public class DatapointService : IDisposable
    {
        private const int SaveIntervalSeconds = 60;

        private static DatapointService _Instance = null; 
        public static DatapointService Instance
        {
            get
            {
                if (_Instance == null)
                { _Instance = new DatapointService(); }

                return _Instance; 
            }
        }


        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>Action for BroadcastMessaged, cannot be inlined due to weak ref use.</summary>
        private readonly Action<string> _onHardwareChanged;

        private readonly List<SensorBuffer> _sensorBuffer = new List<SensorBuffer>();
        private List<Device> _dbDevices;

        /// <summary>Most operations replace this task with a continuation on it to make the tasks sequential.</summary>
        private Task _localTask;

        private DispatcherTimer _saveTimer;
        private List<SensorType> _sensorTypes;

        private DatapointService()
        {
            Task.Run(() => ((App) Application.Current).Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _saveTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(SaveIntervalSeconds)};
                _saveTimer.Tick += SaveBufferedReadings;
                _saveTimer.Start();
            }));

            _onHardwareChanged = HardwareChanged;
            BroadcasterService.Instance.TablesChanged.Subscribe(_onHardwareChanged);

            _localTask = Task.Run(() => { LoadData(); });
        }

        /// <summary>The date with hours, minutes and seconds set to zero</summary>
        public static DateTimeOffset Today => DateTimeOffset.Now.Subtract(DateTimeOffset.Now.TimeOfDay);
    

        public static DateTimeOffset Tomorrow =>  DateTimeOffset.Now.AddDays(1).Subtract(DateTimeOffset.Now.TimeOfDay);
    

        /// <summary>The date of yesturday with hours, minutes and seconds set to zero</summary>
        public static DateTimeOffset Yesturday => DateTimeOffset.Now.AddDays(-1).Subtract(DateTimeOffset.Now.TimeOfDay);
        
        public void Dispose()
        {
            BlockingDispatcher.Run(() => _saveTimer?.Stop());
            _Instance = null;
        }

        public void BufferAndSendReadings(KeyValuePair<Guid, SensorMessage[]> values, string deviceName)
        {
            //Purposefull fire and forget
            _localTask = _localTask.ContinueWith(previous =>
            {
                var device = _dbDevices.FirstOrDefault(dv => dv.SerialNumber == values.Key);

                if (device == null)
                {
                    CreateDevice(values, deviceName);
                }
                else
                {
                    var sensorReadings = new List<BroadcasterService.SensorReading>();

                    foreach (var message in values.Value)
                    {
                        try
                        {
                            var sensorBuffer = _sensorBuffer.FirstOrDefault(sb => sb.Sensor.SensorTypeID == message.SensorTypeID);
                            if(sensorBuffer == null)
                            {
                                Util.ToastService.NotifyUserOfError($"Device has a new sensor with typeID {message.SensorTypeID}, however adding sensors is not supported yet.");
                                break; 
                            }
                            var duration = TimeSpan.FromMilliseconds((double) message.duration/1000);
                            var timeStamp = DateTimeOffset.Now;
                            var datapoint = new SensorDatapoint(message.value, timeStamp, duration);
                            sensorBuffer.FreshBuffer.Add(datapoint);
                            var sensorReading = new BroadcasterService.SensorReading(sensorBuffer.Sensor.ID, datapoint.Value,
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
                    BroadcasterService.Instance.NewSensorDataPoint.Invoke(sensorReadings);
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
        private bool CreateDevice(KeyValuePair<Guid, SensorMessage[]> values, string devicename)
        {
            var db = new MainDbContext();

            //Make sure that it is good and proper to create this device
            if (SettingsService.Instance.IsLoggedIn == false
                || SettingsService.Instance.LastSuccessfulGeneralDbGet == default(DateTimeOffset) 
                || _dbDevices.Any(dev => dev.SerialNumber == values.Key))
            {
                return false;
            }

            //Make sure this device is reporting valid sensor Id's
            var sensorTypes = db.SensorTypes.ToList();
            foreach (var sensor in values.Value)
            {
                if (sensorTypes.FirstOrDefault(st => st.ID == sensor.SensorTypeID) == null)
                {
                    Util.ToastService.NotifyUserOfError($"Receieved message with sensor ID {sensor.SensorTypeID}, but it is invalid - there is no such ID");
                    return false; //If any of the sensors are invalid, then return;
                }
            }
            
            Debug.WriteLine("addingDevice");

            var device = new Device
            {
                ID = Guid.NewGuid(),
                SerialNumber = values.Key,
                Deleted = false,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now,
                Name = $"{devicename} {_dbDevices.Count}",
                Relays = new List<Relay>(),
                Sensors = new List<Sensor>(),
                Version = new byte[32],
                Location =
                    new Location
                    {
                        ID = Guid.NewGuid(),
                        Deleted = false,
                        Name = string.Format("Box Number {0}", _dbDevices.Count),
                        PersonId = SettingsService.Instance.CredStableSid, //TODO use the thing from settings! 
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

        private void HardwareChanged(string value)
        {
            _localTask = _localTask.ContinueWith(previous => { LoadData(); });
        }


        //TODO register this with an event in messenger class
        /// <summary>Loads device and sensor info from the DB.</summary>
        /// <remarks>Don't make publish or call directly! always push onto the task!</remarks>
        /// <returns>True if it loaded something, false otherwise.</returns>
        private void LoadData()
        {
            Debug.WriteLine("DatapointsSaver refreshing cache");
            using (var db = new MainDbContext())
            {
                _dbDevices = db.Devices.Include(dv => dv.Sensors).Include(dv => dv.Relays).AsNoTracking().ToList();
                var sensorsHistory = db.SensorsHistory.Where(sh => sh.TimeStamp > Today).ToList(); // we will edit this
                _sensorTypes = db.SensorTypes.Include(st => st.Param).Include(st => st.Place).AsNoTracking().ToList();

                //Add missing sensors and relays 
                foreach (var sensor in _dbDevices.SelectMany(dv => dv.Sensors))
                {
                    if (_sensorBuffer.Any(sb => sb.Sensor.ID == sensor.ID) == false)
                    {
                        _sensorBuffer.Add(new SensorBuffer(sensor));
                    }
                } //TODO merge the datapoints! 
                foreach (var sHistory in sensorsHistory)
                {
                    sHistory.DeserialiseData();
                    var mIndex = _sensorBuffer.FindIndex(sb => sb.Sensor.ID == sHistory.SensorID);
                    if (_sensorBuffer[mIndex].DataDay == null ||
                        _sensorBuffer[mIndex].DataDay.TimeStamp < sHistory.TimeStamp)
                    {
                        _sensorBuffer[mIndex] = new SensorBuffer(_sensorBuffer[mIndex].Sensor, sHistory);
                    }
                    else if (_sensorBuffer[mIndex].DataDay.Data != null && sHistory.Data != null)
                    {
                        var sHistMerged = SensorHistory.Merge(_sensorBuffer[mIndex].DataDay, sHistory);
                        _sensorBuffer[mIndex] = new SensorBuffer(_sensorBuffer[mIndex].Sensor, sHistMerged);
                    }
                }
            }
        }


        //Closes sensor Histories that are no longer usefull
        private void SaveBufferedReadings(object sender, object e)
        {
            ToastService.Debug("SaveBufferedReadings", $"{DateTimeOffset.Now.DateTime} Datapointsaver");
            _localTask = _localTask.ContinueWith(previous =>
            {
                //if (Settings.Instance.LastSuccessfulGeneralDbGet > DateTimeOffset.Now - TimeSpan.FromMinutes(5))
                //{
                Debug.WriteLine("Datasaver started, did not bother detecting a recent update.");

                using (var db = new MainDbContext())
                {
                    for (var i = 0; i < _sensorBuffer.Count; i++)
                    {
                        var sbuffer = _sensorBuffer[i];

                        SensorDatapoint sensorDatapoint = null;

                        if (sbuffer.FreshBuffer.Count > 0)
                        {
                            var startTime = sbuffer.FreshBuffer[0].TimeStamp;
                            var endTime = sbuffer.FreshBuffer.Last().TimeStamp;
                            var duration = (endTime - startTime).Subtract(sbuffer.FreshBuffer[0].Duration);

                            var cumulativeDuration = TimeSpan.Zero;
                            double cumulativeValue = 0;

                            for (var b = 0; b < sbuffer.FreshBuffer.Count; b++)
                            {
                                cumulativeDuration += sbuffer.FreshBuffer[b].Duration;
                                cumulativeValue += sbuffer.FreshBuffer[b].Value;
                            }

                            var sensorType = _sensorTypes.First(st => st.ID == sbuffer.Sensor.SensorTypeID);
                            var value = cumulativeValue/sbuffer.FreshBuffer.Count;

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

                            sbuffer.FreshBuffer.RemoveRange(0, sbuffer.FreshBuffer.Count);
                        }

                        //only if new data is present
                        if (sensorDatapoint != null)
                        {
                            //check if corresponding dataDay is too old or none exists at all 
                            if (sbuffer.DataDay?.TimeStamp < sensorDatapoint.TimeStamp || sbuffer.DataDay == null)
                            {
                                var dataDay = new SensorHistory
                                {
                                    LocationID = sbuffer.Sensor.Device.LocationID,
                                    SensorID = sbuffer.Sensor.ID,
                                    Sensor = sbuffer.Sensor,
                                    TimeStamp = Tomorrow,
                                    Data = new List<SensorDatapoint>()
                                };
                                _sensorBuffer[i] = new SensorBuffer(sbuffer.Sensor, dataDay);
                                //Only uses this entity, and does not follow the references to stick related references in the DB  
                                db.Entry(dataDay).State = EntityState.Added;
                            }
                            else
                            {
                                //this will not attach related entities, which is good 
                                db.Entry(sbuffer.DataDay).State = EntityState.Unchanged;
                            }

                            _sensorBuffer[i].DataDay.Data.Add(sensorDatapoint);
                            _sensorBuffer[i].DataDay.SerialiseData();
                        }
                    } //for loop ends 
                    //Once we are done here, mark changes to the db
                    db.SaveChanges();
                    Debug.WriteLine("Saved Sensor Data");
                }
                //}
                //else
                //{
                //    Debug.WriteLine("Skipped datasaver due to lack of recent update.");
                //}
            });
        }


        private class SensorBuffer
        {
            public readonly SensorHistory DataDay;
            public readonly List<SensorDatapoint> FreshBuffer;
            public readonly Sensor Sensor;

            public SensorBuffer(Sensor assignSensor, SensorHistory inDataDay = null)
            {
                Sensor = assignSensor;
                FreshBuffer = new List<SensorDatapoint>();
                DataDay = inDataDay;
            }
        }
    }
}
