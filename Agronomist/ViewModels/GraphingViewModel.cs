using DatabasePOCOs;
using DatabasePOCOs.User;
using DatabasePOCOs.Global;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using Agronomist.Models;

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";
        private MainDbContext _db = null; 


        /// <summary>
        /// Cached Data, of all cropCycles
        /// </summary>
        private List<CropCycle> _cropCycles;

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

        public List<CropCycle> CropCycles
        {
            get { return _cropCycles; }
            set
            {
                if (value == _cropCycles) return;
                _cropCycles = value;
                OnPropertyChanged();
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
                    Sensors = _db.At
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