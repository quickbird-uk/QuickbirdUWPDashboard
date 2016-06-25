namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
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
        private const string Visible = "Visible";
        private const string Collapsed = "Collapsed";

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<IEnumerable<Messenger.SensorReading>> _dataUpdater;
        private readonly CoreDispatcher _dispatcher;

        private string _ageStatus;

        private string _cardBackColour = NormalCardColour;

        private bool _highAlertIsEnabled;

        private bool _lowAlertIsEnabled;

        private string _readingSideVisible = Visible;

        private double _scaleX = 300;

        private double _scaleY = 260;
        private string _settingSideVisible = Collapsed;

        private bool _showSettingsToggleChecked;

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
                    await
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () => UpdateValueAndAgeStatusIfNew(mostRecent.Value, mostRecent.Timestamp));
                }
            };

            var ageStatusUpdateTime = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            ageStatusUpdateTime.Tick += AgeStatusUpdateTimeOnTick;
            ageStatusUpdateTime.Start();

            Messenger.Instance.NewSensorDataPoint.Subscribe(_dataUpdater);
            Update(poco);
        }

        private DateTimeOffset TimeOfCurrentValue { get; set; }

        public string AgeStatus
        {
            get { return _ageStatus; }
            set
            {
                if (value == _ageStatus) return;
                _ageStatus = value;
                OnPropertyChanged();
            }
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

        public string ReadingSideVisible
        {
            get { return _readingSideVisible; }
            private set
            {
                if (value == _readingSideVisible) return;
                _readingSideVisible = value;
                OnPropertyChanged();
            }
        }

        public string SettingSideVisible
        {
            get { return _settingSideVisible; }
            private set
            {
                if (value == _settingSideVisible) return;
                _settingSideVisible = value;
                OnPropertyChanged();
            }
        }

        public Guid Id { get; }

        public string Placement { get; }

        public bool? ShowSettingsToggleChecked
        {
            get { return _showSettingsToggleChecked; }
            set
            {
                if (value == _showSettingsToggleChecked) return;
                _showSettingsToggleChecked = value ?? false;
                if (_showSettingsToggleChecked)
                {
                    DisplaySettingsPage();
                }
                OnPropertyChanged();
            }
        }

        public bool HighAlertIsEnabled
        {
            get { return _highAlertIsEnabled; }
            set
            {
                if (value == _highAlertIsEnabled) return;
                _highAlertIsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool LowAlertIsEnabled
        {
            get { return _lowAlertIsEnabled; }
            set
            {
                if (value == _lowAlertIsEnabled) return;
                _lowAlertIsEnabled = value;
                OnPropertyChanged();
            }
        }

        public double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                OnPropertyChanged();
            }
        }

        public double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                OnPropertyChanged();
            }
        }

        private void AgeStatusUpdateTimeOnTick(object sender, object o)
        {
            UpdateAgeStatusMessage();
        }

        /// <summary>
        /// Update the age status message according to how old it is.
        /// </summary>
        private void UpdateAgeStatusMessage()
        {
            var now = DateTimeOffset.Now;
            var age = now - TimeOfCurrentValue;
            if (age < TimeSpan.FromSeconds(5))
            {
                AgeStatus = "live reading";
            }
            else if (age < TimeSpan.FromSeconds(60))
            {
                var seconds = age.Seconds;
                var plural = seconds == 1 ? "" : "s";
                AgeStatus = $"{seconds} second{plural} ago";
            }
            else if (age < TimeSpan.FromMinutes(60))
            {
                var mins = age.Minutes;
                var plural = mins == 1 ? "" : "s";
                AgeStatus = $"{mins} minute{plural} ago";
            }
            else if (age < TimeSpan.FromHours(24))
            {
                var hours = age.Hours;
                var plural = hours == 1 ? "" : "s";
                AgeStatus = $"{hours} hour{plural} ago";
            }
            else
            {
                var days = (int) Math.Floor(age.TotalDays);
                var plural = days == 1 ? "" : "s";
                AgeStatus = $"{days} day{plural} ago";
            }
        }

        /// <summary>
        /// Only updates the UI if the value is newer.
        /// </summary>
        /// <param name="value">The value to display.</param>
        /// <param name="time">The datestamp of the reading.</param>
        private void UpdateValueAndAgeStatusIfNew(double value, DateTimeOffset time)
        {
            if (time > TimeOfCurrentValue)
            {
                TimeOfCurrentValue = time;
                var formattedValue = FormatValue(value);
                Value = formattedValue;
                UpdateAgeStatusMessage();
            }
        }

        /// <summary>
        /// Updates the basic data on the live card.
        /// </summary>
        /// <param name="poco"></param>
        public void Update(Sensor poco)
        {
            Units = poco.SensorType.Param.Unit;
            UnitName = poco.SensorType.Place.Name + " " + poco.SensorType.Param.Name;
            //TODO: Status = poco.AlertStatus
        }

        private void DisplaySettingsPage()
        {
            SettingSideVisible = Visible;
            ReadingSideVisible = Collapsed;

            //TODO: Get the high and low alert numbers.
            //TODO: Get high and low alert enabled vars.
            //TODO: Set the fetched values on the settings page.
        }

        public void SaveSettingsChanges()
        {
            //TODO: Save settings.

            // Now hide the settings page.
            CancelSettingsChanges();
        }

        /// <summary>
        ///     Hides the settings page.
        /// </summary>
        public void CancelSettingsChanges()
        {
            ReadingSideVisible = Visible;
            SettingSideVisible = Collapsed;
            ShowSettingsToggleChecked = false;
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