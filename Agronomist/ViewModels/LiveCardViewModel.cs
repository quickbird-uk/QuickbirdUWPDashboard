namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using DatabasePOCOs;
    using MoreLinq;
    using Util;

    public class LiveCardViewModel : ViewModelBase
    {
        private const string Play = "&#xE768";
        private const string Pause = "&#xE769";
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<IEnumerable<Messenger.SensorReading>> _dataUpdater;
        private readonly CoreDispatcher _dispatcher;

        private bool _propPanelVisible;

        private string _status = "OK";

        private string _unitName = "sensor type";
        private string _units = "Units";

        private string _value = "?";

        public LiveCardViewModel(Sensor poco)
        {
            Id = poco.ID;
            Placement = poco.SensorType.Place.Name;
            PlacementId = poco.SensorType.PlaceID;
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            _dataUpdater = async readings =>
            {
                var ofThisSensor = readings.Where(r => r.SensorId == poco.ID).ToList();
                if (ofThisSensor.Any())
                {
                    var mostRecent = ofThisSensor.MaxBy(r => r.Timestamp);
                    var formattedValue = FormatValue(mostRecent.Value);
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Value = formattedValue);
                }
            };
            Messenger.Instance.NewSensorDataPoint.Subscribe(_dataUpdater);
            Update(poco);
        }

        public long PlacementId { get; }

        public string PropPanelVisible => _propPanelVisible ? "Visible" : "Collapsed";

        /// <summary>
        ///     Current Sensor Value
        /// </summary>
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

        /// <summary>
        ///     Units of the sensor value.
        /// </summary>
        public string Units
        {
            get { return _units; }
            set
            {
                if (value == _units) return;
                _units = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public string UnitName
        {
            get { return _unitName; }
            set
            {
                if (value == _unitName) return;
                _unitName = value;
                OnPropertyChanged();
            }
        }

        public Guid Id { get; }

        public string Placement { get; }

        public void SetPropPanelVisibility(bool isVisible)
        {
            _propPanelVisible = isVisible;
            OnPropertyChanged(nameof(PropPanelVisible));
        }

        public void Update(Sensor poco)
        {
            Units = poco.SensorType.Param.Unit;
            UnitName = poco.SensorType.Param.Name;
            //TODO: Status = poco.AlertStatus
        }

        /// <summary>
        ///     Formats the raw datavalue for display.
        /// </summary>
        /// <param name="value">The sensor reading number.</param>
        /// <returns>A formatted number ready for display.</returns>
        private string FormatValue(double value)
        {
            return string.Format(value < 10 ? "{0:0.0}" : "{0:0}", value);
        }
    }
}