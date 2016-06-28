namespace Agronomist.ViewModels
{
    using System;
    using Windows.UI.Xaml;
    using LocalNetworking;
    using Util;

    public class SettingsViewModel : ViewModelBase
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        public SettingsViewModel()
        {
            _lastConflictDetected = DateTimeOffset.MinValue;
            _localNetworkConflictAction = LocalNetworkConflictDetected;
            Messenger.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds()
            };
        }

        private void LocalNetworkConflictDetected(string ip)
        {
            IsNetworkConflict = true;
            _lastConflictDetected = DateTimeOffset.Now;
        }

        private bool _isNetworkConflict;
        private DateTimeOffset _lastConflictDetected;

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
                OnPropertyChanged();
            }
        }
    }
}