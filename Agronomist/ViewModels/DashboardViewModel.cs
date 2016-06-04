namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DatabasePOCOs.User;
    using JetBrains.Annotations;

    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<LiveCardViewModel> _ambientCards = new ObservableCollection<LiveCardViewModel>();

        private ObservableCollection<LiveCardViewModel> _cards = new ObservableCollection<LiveCardViewModel>();

        private ObservableCollection<LiveCardViewModel> _plantCards = new ObservableCollection<LiveCardViewModel>();

        private ObservableCollection<LiveCardViewModel> _waterCards = new ObservableCollection<LiveCardViewModel>();

        public DashboardViewModel([NotNull] CropCycle run)
        {
            CropId = run.ID;
            Update(run);
        }

        public ObservableCollection<LiveCardViewModel> Cards
        {
            get { return _cards; }
            set
            {
                if (value == _cards) return;
                _cards = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LiveCardViewModel> WaterCards
        {
            get { return _waterCards; }
            set
            {
                if (value == _waterCards) return;
                _waterCards = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LiveCardViewModel> AmbientCards
        {
            get { return _ambientCards; }
            set
            {
                if (value == _ambientCards) return;
                _ambientCards = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LiveCardViewModel> PlantCards
        {
            get { return _plantCards; }
            set
            {
                if (value == _plantCards) return;
                _plantCards = value;
                OnPropertyChanged();
            }
        }

        public Guid CropId { get; }

        /// <summary>
        ///     Updates are externally imposed after a bigger database query.
        /// </summary>
        /// <param name="run">CropCycle including Location.Devices.Sensors.Params</param>
        public void Update(CropCycle run)
        {
            var sensors = run.Location.Devices.SelectMany(device => device.Sensors);
            var missingSensors = _cards.Where(c => sensors.FirstOrDefault(s => s.ID == c.Id) == null).ToList();
            foreach (var missingSensor in missingSensors)
            {
                _cards.Remove(missingSensor);
                if (PlantCards.Contains(missingSensor)) PlantCards.Remove(missingSensor);
                if (WaterCards.Contains(missingSensor)) WaterCards.Remove(missingSensor);
                if (AmbientCards.Contains(missingSensor)) AmbientCards.Remove(missingSensor);
            }

            // Add, remove or update for each sensor.
            foreach (var sensor in sensors)
            {
                var existing = _cards.FirstOrDefault(c => c.Id == sensor.ID);
                if (existing == null)
                {
                    if (!sensor.Deleted)
                        Cards.Add(new LiveCardViewModel(sensor));
                }
                else
                {
                    if (sensor.Deleted)
                    {
                        _cards.Remove(existing);
                    }
                    else
                    {
                        existing.Update(sensor);
                    }
                }
            }
            //WE chose these two sensors for plantID's. The system we have does not have a good selection right now 
            var plantIds = new long[]
            {
                11, 8
            };
            var waterIds = new long[]
            {
                13, 19, 4, 16
            };
            var ambientIds = new long[]
            {
                5, 6
            };

            var plantItems = Cards.Where(c => plantIds.Contains(c.SensorTypeID));
            var waterItems = Cards.Where(c => waterIds.Contains(c.SensorTypeID));
            var ambientItems = Cards.Where(c => ambientIds.Contains(c.SensorTypeID));

            foreach (var item in plantItems)
            {
                if (!PlantCards.Contains(item)) PlantCards.Add(item);
            }
            foreach (var item in waterItems)
            {
                if (!WaterCards.Contains(item)) WaterCards.Add(item);
            }
            foreach (var item in ambientItems)
            {
                if (!AmbientCards.Contains(item)) AmbientCards.Add(item);
            }
        }
    }
}