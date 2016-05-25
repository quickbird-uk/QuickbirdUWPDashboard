using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agronomist.ViewModels
{
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using DatabasePOCOs;
    using MoreLinq;
    using Util;

    class SensorViewModel : ViewModelBase
    {
        private readonly Sensor _sensorPoco;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<IEnumerable<Messenger.SensorReading>> _dataUpdater;
        private readonly CoreDispatcher _dispatcher;

        public SensorViewModel(Sensor sensorPoco)
        {
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            _sensorPoco = sensorPoco;
            _dataUpdater = async readings =>
            {
                var mostRecent = readings.Where(r => r.SensorId == _sensorPoco.ID).MaxBy(r => r.Timestamp);
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetValue(mostRecent.Value));
            };
            Messenger.Instance.NewSensorDataPoint.Subscribe(_dataUpdater);
        }

        private void SetValue(double value)
        {
            Value = string.Format(value < 10 ? "{0:0.0}" : "{0:0}", value);
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}
