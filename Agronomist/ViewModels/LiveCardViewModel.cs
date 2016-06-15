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
        public const string Play = "\xE768";
        public const string Pause = "\xE769";
        public const string Warn = "\xE8C9";
        public const string Tick = "\xE8FB";
        public const string Up = "\xE898";
        public const string Down = "\xE896";
        public const string NormalCardColour = "#FF4A90E2";
        public const string WarnCardColour = "#FFFFFF00";
        public const string ErrorCardColour = "#FFFF0000";

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<IEnumerable<Messenger.SensorReading>> _dataUpdater;
        private readonly CoreDispatcher _dispatcher;
        
        private string _statusSymbol = Tick;
        private string _unitName = "sensor type";
        private string _units = "Units";
        private string _value = "?";

        public LiveCardViewModel(Sensor poco)
        {
            Id = poco.ID;
            Placement = poco.SensorType.Place.Name;
            PlacementId = poco.SensorType.PlaceID;
            ParameterID = poco.SensorType.ParamID;
            SensorTypeID = poco.SensorTypeID;
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

        public long SensorTypeID { get; }

        public long ParameterID { get; }

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
        
        public string StatusSymbol
        {
            get { return _statusSymbol; }
            set
            {
                if (value == _statusSymbol) return;
                _statusSymbol = value;
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

        private string _cardBackColour = NormalCardColour;

        public string CardBackColour
        {
            get { return _cardBackColour; }
            set
            {
                if (value == _cardBackColour) return;
                _cardBackColour = value;
                OnPropertyChanged();
            }
        }

        public Guid Id { get; }

        public string Placement { get; }

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