namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DbStructure.User;
    using JetBrains.Annotations;

    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<LiveCardViewModel> _ambientCards = new ObservableCollection<LiveCardViewModel>();

        private ObservableCollection<LiveCardViewModel> _cards = new ObservableCollection<LiveCardViewModel>();

        private ObservableCollection<LiveCardViewModel> _mainCards = new ObservableCollection<LiveCardViewModel>();

        /// <summary>All data in this model trickles up from the DashboardViewModel, making this a simple data
        /// model class.</summary>
        /// <param name="run"></param>
        public DashboardViewModel([NotNull] CropCycle run)
        {
            CropId = run.ID;
            Update(run);
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

        public Guid CropId { get; }

        public ObservableCollection<LiveCardViewModel> MainCards
        {
            get { return _mainCards; }
            set
            {
                if (value == _mainCards) return;
                _mainCards = value;
                OnPropertyChanged();
            }
        }

        public override void Kill()
        {
            foreach (var liveCardViewModel in AmbientCards)
            {
                liveCardViewModel.Kill();
            }

            foreach (var liveCardViewModel in Cards)
            {
                liveCardViewModel.Kill();
            }

            foreach (var liveCardViewModel in MainCards)
            {
                liveCardViewModel.Kill();
            }
        }

        /// <summary>Updates are externally imposed after a bigger database query.</summary>
        /// <param name="run">CropCycle including Location.Devices.Sensors.Params</param>
        public void Update(CropCycle run)
        {
            var sensors = run.Location.Devices.SelectMany(device => device.Sensors);
            var missingSensors = _cards.Where(c => sensors.FirstOrDefault(s => s.ID == c.Id) == null).ToList();
            foreach (var missingSensor in missingSensors)
            {
                _cards.Remove(missingSensor);
                if (MainCards.Contains(missingSensor)) MainCards.Remove(missingSensor);
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
            var mainIds = new long[] {8, 13, 19, 4, 16};
            var ambientIds = new long[] {5, 6, 11};

            var mainItems = Cards.Where(c => mainIds.Contains(c.SensorTypeID));
            var ambientItems = Cards.Where(c => ambientIds.Contains(c.SensorTypeID));

            foreach (var item in mainItems)
            {
                if (!MainCards.Contains(item)) MainCards.Add(item);
            }
            foreach (var item in ambientItems)
            {
                if (!AmbientCards.Contains(item)) AmbientCards.Add(item);
            }
        }
    }
}
