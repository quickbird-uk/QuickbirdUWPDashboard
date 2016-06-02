namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DatabasePOCOs.User;
    using JetBrains.Annotations;

    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<LiveCardViewModel> _cards = new ObservableCollection<LiveCardViewModel>();

        public DashboardViewModel([NotNull]CropCycle run)
        {
            CropID = run.ID;
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

        public Guid CropID { get; }

        /// <summary>
        ///     Updates are externally imposed after a bigger database query.
        /// </summary>
        /// <param name="run">CropCycle including Location.Devices.Sensors.Params</param>
        public void Update(CropCycle run)
        {
            var sensors = run.Location.Devices.SelectMany(device => device.Sensors);
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
        }
    }
}