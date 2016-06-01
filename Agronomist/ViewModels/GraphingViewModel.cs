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

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";
        private MainDbContext _db = null;


        /// <summary>
        /// Cached Data, of all cropCycles
        /// </summary>
        private List<Tuple<Location, CropCycle, List<Sensor>>> _cache = new List<Tuple<Location, CropCycle, List<Sensor>>>();

        /* This data applies to the chosen crop cycle only*/
        private CropCycle _selectedCropCycle; 
        private List<Sensor> _sensors;
        private DateTimeOffset _startTime;
        private DateTimeOffset? _endTime;
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
                .ToListAsync();

            var sensorList = await _db.Sensors.ToListAsync(); //Need to edit 
            
            List<Tuple<Location, CropCycle, List<Sensor>>> cache = 
                new List<Tuple<Location, CropCycle, List<Sensor>>>();


            foreach (CropCycle crop in dbLocations.SelectMany(loc => loc.CropCycles))
            {
                Tuple<Location, CropCycle, List<Sensor>> cacheItem = new Tuple<Location, CropCycle, List<Sensor>>(
                    crop.Location, crop, new List<Sensor>());

                List<Guid> deviceIDs = crop.Location.Devices.Select(dev => dev.ID).ToList(); 
                foreach(Sensor sensor in sensorList)
                {
                    if (deviceIDs.Contains(sensor.DeviceID))
                    {
                        cacheItem.Item3.Add(sensor);
                    }
                }

                cache.Add(cacheItem); 
            }
            if(_selectedCropCycle == null)
            {
                _selectedCropCycle = cache.FirstOrDefault().Item2; 
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
                    string displayName = $"{tuple.Item1.Name} - {tuple.Item2.CropTypeName}: " 
                       + $"{tuple.Item2.StartDate.LocalDateTime.Date.ToString("dd MMM")}"
                       + $"-{tuple.Item2.EndDate?.LocalDateTime.Date.ToString("dd MMM") ?? "Now"}"; 
                        
                    result.Add(new KeyValuePair<CropCycle, string>(tuple.Item2, displayName)); 
                }
                return result; 
            }
        }

        public List<Tuple<Location, CropCycle, List<Sensor>>> Cache
        {
            get { return _cache; }
            set
            {
                if (value == _cache) return;
                else
                {
                    _cache = value;
                    if(_selectedCropCycle != null)
                    SelectedCropCycle = _cache.First(l => l.Item2.ID == _selectedCropCycle.ID).Item2; 

                    OnPropertyChanged();
                    OnPropertyChanged("Locations");
                    OnPropertyChanged("CropRunList");
                }

            }
        }

        public List<Location> Locations
        {
            get { return Cache.DistinctBy(c => c.Item1).Select(tup => tup.Item1).ToList();}
        }

        public List<Sensor> Sensors
        {
            get { return _sensors; }
            set
            {
                if (value == _sensors) return;
                _sensors = value;
                OnPropertyChanged();
            }
        }



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
                    Sensors = _cache.First(c => c.Item2.ID == value.ID).Item3;
                    EndTime = _selectedCropCycle.EndDate ?? DateTimeOffset.Now;
                    _endTime = _selectedCropCycle.EndDate;
                    StartTime = _selectedCropCycle.StartDate; 

                    OnPropertyChanged();
                }
            }
        }

        public DateTimeOffset EndTime
        {
            get { return _endTime ?? DateTimeOffset.Now; }
            set{
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public DateTimeOffset StartTime
        {
            get { return _startTime; }
            set { _startTime = value;
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
    }
}