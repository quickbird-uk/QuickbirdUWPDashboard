using DatabasePOCOs;
using DatabasePOCOs.User;
using DatabasePOCOs.Global;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using Agronomist.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using MoreLinq;
using Windows.UI.Xaml.Data;
using System.Collections.ObjectModel;
using Agronomist.Util;

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";
        private MainDbContext _db = null;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<IEnumerable<Messenger.SensorReading>> _recieveDatapointAction;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _loadCacheAction;

        /// <summary>
        /// Cached Data, of all cropCycles
        /// </summary>
        private List<CroprunTuple> _cache = new List<CroprunTuple>();

        /* This data applies to the chosen crop cycle only*/
        private CropCycle _selectedCropCycle; 

        private IEnumerable<IGrouping<string, SensorTuple>> _sensors;
        private DateTimeOffset _selectedStartTime;
        private DateTimeOffset? _selectedEndTime;
        private bool _currentlyRunning = true;

        private bool _realtimeMode; 

        //Probaablyt don't need this
        private DispatcherTimer _refresher = null; 

        //Other stuff
        //Hour Long Buffer
        //Histrotical Buffer 

        public GraphingViewModel(){
            _db = new MainDbContext();

            _recieveDatapointAction = ReceiveDatapoint;
            _loadCacheAction = LoadCache;
            Messenger.Instance.NewSensorDataPoint.Subscribe(_recieveDatapointAction);
            Messenger.Instance.TablesChanged.Subscribe(_loadCacheAction);

            //Settings settings = new Settings();
            //settings.UnsetCreds(); 

            //LoadData
            LoadCache(); 
        }

        public void LoadCache(string obj)
        {
            LoadCache(); 
        }

        public void ReceiveDatapoint(IEnumerable<Messenger.SensorReading> readings)
        {
            if (SensorsGrouped != null)
            {
                List<SensorTuple> sensorsUngrouped = SensorsGrouped.SelectMany(group => group).ToList();
                foreach (Messenger.SensorReading reading in readings)
                {
                    SensorTuple tuple = sensorsUngrouped.FirstOrDefault(stup => stup.sensor.ID == reading.SensorId);
                    if (tuple != null)
                    {
                        if (tuple.hourlyDatapoints.Any(dp => dp.timestamp == reading.Timestamp.LocalDateTime) == false)
                        {
                            BindableDatapoint datapoint = new BindableDatapoint(reading.Timestamp, reading.Value);
                            tuple.hourlyDatapoints.Add(datapoint);
                        }
                        //Remove datapoints if we are storing more than an hour of them
                        TimeSpan period;
                        do
                        {
                            period = tuple.historicalDatapoints[0].timestamp - tuple.historicalDatapoints.Last().timestamp;
                            if (period.TotalHours > 1)
                            {
                                tuple.historicalDatapoints.RemoveAt(0);
                            }
                        } while (period.TotalHours > 1); 
                    }
                }
                var tupleForChartUpdate = sensorsUngrouped.FirstOrDefault(); 
                if (tupleForChartUpdate?.RealtimeMode ?? false)
                {
                    tupleForChartUpdate.Axis.Minimum = tupleForChartUpdate.hourlyDatapoints.FirstOrDefault()?.timestamp ?? DateTime.Now.AddHours(-1);
                    tupleForChartUpdate.Axis.Maximum = tupleForChartUpdate.hourlyDatapoints.LastOrDefault()?.timestamp ?? DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Refreshed Cache
        /// </summary>
        private async void LoadCache()
        {
            var dbLocations = await _db.Locations
                .Include(loc => loc.CropCycles)
                .Include(loc => loc.Devices)
                .AsNoTracking().ToListAsync();

            var sensorList = await _db.Sensors
                .Include(sen => sen.SensorType)
                .Include(sen => sen.SensorType.Place)
                .Include(sen => sen.SensorType.Param)
                .Include(sen => sen.SensorType.Subsystem)
                .AsNoTracking().ToListAsync(); //Need to edit 
            
            List<CroprunTuple> cache = 
                new List<CroprunTuple>();


            foreach (CropCycle crop in dbLocations.SelectMany(loc => loc.CropCycles))
            {
                CroprunTuple cacheItem = new CroprunTuple(crop, crop.Location);

                List<Guid> deviceIDs = crop.Location.Devices.Select(dev => dev.ID).ToList(); 
                foreach(Sensor sensor in sensorList)
                {
                    if (deviceIDs.Contains(sensor.DeviceID))
                    {
                        cacheItem.sensors.Add(sensor);
                    }
                }

                cache.Add(cacheItem); 
            }
            if(_selectedCropCycle == null)
            {
                _selectedCropCycle = cache.FirstOrDefault().cropCycle; 
            }
            Cache = cache; 
            
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if(value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public List<KeyValuePair<CropCycle, string>> CropRunList
        {
            get
            {
                List<KeyValuePair<CropCycle, string>> result = new List<KeyValuePair<CropCycle, string>>(); 
                foreach(var tuple in Cache)
                {
                    string displayName = $"{tuple.location.Name} - {tuple.cropCycle.CropTypeName}: " 
                       + $"{tuple.cropCycle.StartDate.LocalDateTime.Date.ToString("dd MMM")}"
                       + $"-{tuple.cropCycle.EndDate?.LocalDateTime.Date.ToString("dd MMM") ?? "Now"}"; 
                        
                    result.Add(new KeyValuePair<CropCycle, string>(tuple.cropCycle, displayName)); 
                }
                return result; 
            }
        }

        public List<CroprunTuple> Cache
        {
            get { return _cache; }
            set
            {
                if (value == _cache) return;
                else
                {
                    _cache = value;
                    if(_selectedCropCycle != null)
                    SelectedCropCycle = _cache.First(l => l.cropCycle.ID == _selectedCropCycle.ID).cropCycle; 

                    OnPropertyChanged();
                    OnPropertyChanged("Locations");
                    OnPropertyChanged("CropRunList");
                }

            }
        }

        public List<Location> Locations
        {
            get { return Cache.DistinctBy(c => c.location).Select(tup => tup.location).ToList();}
        }


        public IEnumerable<IGrouping<string, SensorTuple>> SensorsGrouped //Replace with Igrouping or CollectionViewSource
        {
            get { return _sensors; }
            set
            {
                if (value == _sensors) return;
                _sensors = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SensorTuple> SensorsToGraph { get; set; } = new ObservableCollection<SensorTuple>();  

        public CropCycle SelectedCropCycle
        {
            get { return _selectedCropCycle; }
            set
            {
                if (value == _selectedCropCycle) return;
                else
                {
                    _selectedCropCycle = value;
                    
                    SensorsToGraph.Clear();

                    if (_selectedCropCycle.EndDate == null)
                    { _currentlyRunning = true;  }
                    else
                    { _currentlyRunning = false; }
                    RealtimeMode = false;
                    OnPropertyChanged("RealtimeMode");
                    OnPropertyChanged("LiveCropRun");
                   

                    List<Sensor> sensors = _cache.First(c => c.cropCycle.ID == value.ID).sensors;
                    
                    foreach (var sensor in sensors)
                    {
                        var tuple = new SensorTuple
                        {
                            displayName = sensor.SensorType.Param.Name,
                            sensor = sensor
                        };
                        SensorsToGraph.Add(tuple);
                    }
                    SensorsGrouped = SensorsToGraph.GroupBy(tup => tup.sensor.SensorType.Place.Name);
                    LoadHistoricalData(); 
                    _selectedEndTime = _selectedCropCycle.EndDate;

                    OnPropertyChanged();
                    OnPropertyChanged("CycleEndTime");
                    OnPropertyChanged("CycleStartTime");
                }
            }
        }

        private async void LoadHistoricalData()
        {
            DateTimeOffset endDate = _selectedCropCycle.EndDate?.AddDays(1) ?? DateTimeOffset.Now.AddDays(1); 
            var sensorsHistories = await _db.SensorsHistory.Where(sh => sh.LocationID == _selectedCropCycle.LocationID
                    && sh.TimeStamp > _selectedCropCycle.StartDate &&
                    sh.TimeStamp < endDate).ToListAsync();

            foreach (SensorTuple tuple in SensorsToGraph)
            {
                var shCollection = sensorsHistories.Where(sh => sh.SensorID == tuple.sensor.ID);
                List<BindableDatapoint> datapointCollection = new List<BindableDatapoint>(); 
                foreach (SensorHistory sh in shCollection)
                {
                    sh.DeserialiseData(); 
                    foreach(SensorDatapoint dp in sh.Data)
                    {
                        var bindable = new BindableDatapoint(dp);
                        datapointCollection.Add(bindable); 
                    }
                }
                tuple.historicalDatapoints = new ObservableCollection<BindableDatapoint>(datapointCollection); 
            }           
        }

        public bool LiveCropRun
        {
            get {return _currentlyRunning; }
        }

        public bool RealtimeMode
        {
            get { return _realtimeMode; }
            set { _realtimeMode = value;
                OnPropertyChanged("HistControls");
                TimeLabel = _realtimeMode ? "HH:mm": "MMM dd" ;
                OnPropertyChanged("TimeLabel");
                foreach (var tuple in SensorsToGraph)
                {
                    tuple.RealtimeMode = _realtimeMode; 
                }
            }
        }
        public Visibility HistControls
        {
            get { return _realtimeMode ? Visibility.Collapsed: Visibility.Visible; }
        }

        public DateTimeOffset CycleEndTime
        {
            get { return _selectedCropCycle?.EndDate ?? DateTimeOffset.Now; }
        }
        

        public DateTimeOffset CycleStartTime
        {
            get { return _selectedCropCycle?.StartDate ?? DateTimeOffset.Now.AddDays(-1); }
        }




        public string TimeLabel { get; set; } = "HH:mm"; 

        //Destructor
        ~GraphingViewModel(){
            try
            {
                _refresher?.Stop();
                _db?.Dispose();
            }
            catch(Exception e)
            {

            }
            
        }

        public class SensorTuple : ViewModelBase
        {
            public string displayName { get; set; }
            public Sensor sensor;

            private bool _visible = false;
            private bool _realtimeMode = false;

            private ObservableCollection<BindableDatapoint> _historicalData = new ObservableCollection<BindableDatapoint>(); 

            public bool RealtimeMode
            {
                get { return _realtimeMode; }
                set
                {
                    _realtimeMode = value;
                    if (ChartSeries != null)
                    {
                        ChartSeries.ItemsSource = _realtimeMode ? hourlyDatapoints: historicalDatapoints;
                        if(_realtimeMode)
                        {
                            Axis.Minimum = hourlyDatapoints.FirstOrDefault()?.timestamp ?? DateTime.Now.AddHours(-1);
                            Axis.Maximum = hourlyDatapoints.LastOrDefault()?.timestamp ?? DateTime.Now;
                        }

                    }

                }
            }

            public bool visible {
                get
                {
                    return _visible; 
                }
                set
                {
                    _visible = value;
                    if(ChartSeries != null)
                    {
                        ChartSeries.IsEnabled = _visible;
                        ChartSeries.IsSeriesVisible = _visible;
                    }
                }
            } 
            
            /// <summary>
            /// Updated as soon as possible
            /// </summary>
            public ObservableCollection<BindableDatapoint> hourlyDatapoints { get; set; } = new ObservableCollection<BindableDatapoint>();

            /// <summary>
            /// Only read from the DB, not reloaded in realtime
            /// </summary>
            public ObservableCollection<BindableDatapoint> historicalDatapoints
            {
                get
                { return _historicalData; }
                set
                {
                    _historicalData = value;
                    OnPropertyChanged();
                }
            }

            public Syncfusion.UI.Xaml.Charts.ChartSeries ChartSeries = null;

            public Syncfusion.UI.Xaml.Charts.DateTimeAxis Axis = null; 
        }

        public struct CroprunTuple
        {
            public CroprunTuple(CropCycle inCropCycle, Location inLocation)
            {
                cropCycle = inCropCycle;
                location = inLocation;
                sensors = new System.Collections.Generic.List<Sensor>(); 
            }
            public CropCycle cropCycle;
            public Location location;
            public List<Sensor> sensors; 
        }

        public class BindableDatapoint
        {
            public BindableDatapoint(SensorDatapoint datapoint)
            {
                timestamp = datapoint.TimeStamp.LocalDateTime;
                value = datapoint.Value; 
            }

            public BindableDatapoint(DateTimeOffset inTimestamp, double inValue)
            {
                timestamp = inTimestamp.LocalDateTime;
                value = inValue; 
            }
            public DateTime timestamp { get; set; }
            public double value { get; set; }
        }
    }
}