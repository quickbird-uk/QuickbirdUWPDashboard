using DatabasePOCOs;
using DatabasePOCOs.User;
using DatabasePOCOs.Global;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml;

namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _title = "Graphs";

        //Cached Data
        private List<CropCycle> _cropCycles = new List<CropCycle>();
        private List<Sensor> _theseSensors = new List<Sensor>();
        private DateTime _startTime;
        private DateTime _endTime;
        private bool _currentTun;
        private DispatcherTimer _refresher; 

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

        public List<Sensor> CropCycles
        {
            get { return _cropCycles; }
            set
            {
                if (value == _cropCycles) return;
                _cropCycles = value;
                OnPropertyChanged();
            }
        }


        public void SelectCropCycle(CropCycle cycle)
        {

        }

    }
}