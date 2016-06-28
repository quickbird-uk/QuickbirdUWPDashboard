namespace Agronomist.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using LocalNetworking;
    using Util;

    public class SettingsViewModel : ViewModelBase
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        private bool _isNetworkConflict;
        private static DateTimeOffset _lastConflictDetected = DateTimeOffset.MinValue;

        public SettingsViewModel()
        {
            _localNetworkConflictAction = LocalNetworkConflictDetected;
            Messenger.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            EventHandler<object> timerOnTick = (sender, o) =>
            {
                var now = DateTimeOffset.Now;
                var diff = now - _lastConflictDetected;
                Debug.WriteLine(diff.TotalSeconds);
                IsNetworkConflict = diff < TimeSpan.FromSeconds(UDPMessaging.BroadcastIntervalSeconds + 1);
            };
            timer.Tick += timerOnTick;
            DispatcherTimers.Add(timer);
            timerOnTick.Invoke(null, null);
            timer.Start();
        }

        public bool IsNetworkConflict
        {
            get { return _isNetworkConflict; }
            set
            {
                if (value == _isNetworkConflict) return;
                _isNetworkConflict = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Enable the local network to communicate with devices. tied directly to settings.
        /// </summary>
        public bool DeviceManagementEnabled
        {
            get { return Settings.Instance.LocalDeviceManagementEnabled; }
            set
            {
                if (value == Settings.Instance.LocalDeviceManagementEnabled) return;
                Settings.Instance.LocalDeviceManagementEnabled = value;
                // While we cany guarantee the order of the messenger invokes, the actual settings value will be correct so we don't care.
                Task.Run(() => Messenger.Instance.DeviceManagementEnableChanged.Invoke(null));
                OnPropertyChanged();
            }
        }

        private void LocalNetworkConflictDetected(string ip)
        {
            IsNetworkConflict = true;
            _lastConflictDetected = DateTimeOffset.Now;
        }
    }
}