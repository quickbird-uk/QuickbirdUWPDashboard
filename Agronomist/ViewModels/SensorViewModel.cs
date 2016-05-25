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
    using Util;

    class SensorViewModel : ViewModelBase
    {
        private readonly Sensor _sensorPoco;
        private Action<IEnumerable<Messenger.SensorReading>> _dataUpdater;
        private CoreDispatcher _dispatcher;

        public SensorViewModel(Sensor sensorPoco)
        {
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            _sensorPoco = sensorPoco;
            _dataUpdater = readings =>
            {
                var mostRecent = readings.Where(r => r.SensorId == _sensorPoco.ID).Max(r => r.Timestamp);
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetValue(mostRecent.))
            };
            Messenger.Instance.NewSensorDataPoint.Subscribe(_dataUpdater);
        }
    }
}
