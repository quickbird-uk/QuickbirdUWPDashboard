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

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";
        private MainDbContext _db = null;


        /// <summary>
        /// Cached Data, of all cropCycles
        /// </summary>
        private List<Tuple<Location, CropCycle, List<Sensor>>> _cache; 

        /* This data applies to the chosen crop cycle only*/
        private CropCycle _selectedCropCycle; 
        private List<Sensor> _sensors;
        private DateTime _startTime;
        private DateTime _endTime;
        private bool _currentlyRunning = true;
        private DispatcherTimer _refresher = null; 

        public GraphingViewModel(){
            _db = new MainDbContext();

            
            //LoadData
        }

        /// <summary>
        /// Refreshed Cache
        /// </summary>
        public async void LoadCache()
        {
            var dbLocations = await _db.Locations
                .Include(loc => loc.CropCycles)
                .Include(loc => loc.Devices)
                .ThenInclude(devList => devList.SelectMany(dev => dev.Sensors))
                .AsNoTracking().ToListAsync();

            List<Tuple<Location, CropCycle, List<Sensor>>> cache = 
                new List<Tuple<Location, CropCycle, List<Sensor>>>(); 

            foreach(CropCycle crop in dbLocations.SelectMany(loc => loc.CropCycles))
            {
                Tuple<Location, CropCycle, List<Sensor>> cacheItem = new Tuple<Location, CropCycle, List<Sensor>>(
                    crop.Location, crop, new List<Sensor>());
                
                foreach(Sensor sensor in dbLocations.SelectMany(loc => loc.Devices.SelectMany(dev => dev.Sensors)))
                {
                    cacheItem.Item3.Add(sensor); 
                }

                cache.Add(cacheItem); 
            }

            
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


        public List<Tuple<Location, CropCycle, List<Sensor>>> Cache
        {
            get { return _cache; }
            set
            {
                if (value == _cache) return;
                else
                {
                    _cache = value;
                    SelectedCropCycle = _cache.First(l => l.Item2.ID == _selectedCropCycle.ID).Item2; 

                    OnPropertyChanged();
                }

            }
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
                    if(_selectedCropCycle.EndDate == null)
                    {
                        _currentlyRunning = true;
                    }
                    Sensors = _cache.First(c => c.Item2.ID == value.ID).Item3; 
                    OnPropertyChanged();
                }
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