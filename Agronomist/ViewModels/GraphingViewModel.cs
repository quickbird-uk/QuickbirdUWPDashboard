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

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";
        private MainDbContext _db = null;


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

        //Probaablyt don't need this
        private DispatcherTimer _refresher = null; 

        //Other stuff
        //Hour Long Buffer
        //Histrotical Buffer 

        public GraphingViewModel(){
            _db = new MainDbContext();

            
            //LoadData
            LoadCache(); 
        }

        /// <summary>
        /// Refreshed Cache
        /// </summary>
        public async void LoadCache()
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

        public ObservableCollection<SensorTuple> SensorsToGraph; 

        public CropCycle SelectedCropCycle
        {
            get { return _selectedCropCycle; }
            set
            {
                if (value == _selectedCropCycle) return;
                else
                {
                    _selectedCropCycle = value;
                    if (_selectedCropCycle.EndDate == null)
                    { _currentlyRunning = true;  }
                    else
                    { _currentlyRunning = false; }
                    List<Sensor> sensors = _cache.First(c => c.cropCycle.ID == value.ID).sensors;
                    List<SensorTuple> sensorTuples = new List<SensorTuple>(); 
                    foreach(var sensor in sensors)
                    {
                        sensorTuples.Add(new SensorTuple
                        {
                            displayName = sensor.SensorType.Param.Name,
                            sensor = sensor
                        });
                    }
                    SensorsGrouped = sensorTuples.GroupBy(tup => tup.sensor.SensorType.Subsystem.Name); 
                    
                    EndTime = _selectedCropCycle.EndDate ?? DateTimeOffset.Now;
                    _selectedEndTime = _selectedCropCycle.EndDate;
                    StartTime = _selectedCropCycle.StartDate; 

                    OnPropertyChanged();
                }
            }
        }

        public DateTimeOffset EndTime
        {
            get { return _selectedEndTime ?? DateTimeOffset.Now; }
            set{
                _selectedEndTime = value;
                OnPropertyChanged();
            }
        }

        public DateTimeOffset StartTime
        {
            get { return _selectedStartTime; }
            set { _selectedStartTime = value;
                OnPropertyChanged(); 
            }
        }


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

        public class SensorTuple
        {
            public string displayName;
            public Sensor sensor;
            public bool toggled = false;
            /// <summary>
            /// Updated as soon as possible
            /// </summary>
            public ObservableCollection<SensorDatapoint> hourlyDatapoints = new ObservableCollection<SensorDatapoint>();

            /// <summary>
            /// Only read from the DB, not reloaded in realtime
            /// </summary>
            public ObservableCollection<SensorDatapoint> HistoricalDatapoints = new ObservableCollection<SensorDatapoint>(); 
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
    }
}