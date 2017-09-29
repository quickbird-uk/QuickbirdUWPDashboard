namespace Quickbird.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using LocalNetworking;
    using Util;
    using Views;

    public class SettingsViewModel : ViewModelBase
    {
        private static DateTimeOffset _lastConflictDetected = DateTimeOffset.MinValue;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        private bool _isNetworkConflict;

        public SettingsViewModel()
        {
            _localNetworkConflictAction = LocalNetworkConflictDetected;
            Messenger.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
            var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            EventHandler<object> timerOnTick = (sender, o) =>
            {
                var now = DateTimeOffset.Now;
                var diff = now - _lastConflictDetected;
                IsNetworkConflict = diff < TimeSpan.FromSeconds(UDPMessaging.BroadcastIntervalSeconds + 1);
            };
            timer.Tick += timerOnTick;
            DispatcherTimers.Add(timer);
            timerOnTick.Invoke(null, null);
            timer.Start();
        }

        /// <summary>Enable the local network to communicate with devices. tied directly to settings.</summary>
        public bool DeviceManagementEnabled
        {
            get { return Settings.Instance.LocalDeviceManagementEnabled; }
            set
            {
                if (value == Settings.Instance.LocalDeviceManagementEnabled) return;
                Settings.Instance.LocalDeviceManagementEnabled = value;

                // StartOrKillNetworkManagerBasedOnSettings uses locking to make itself pool-safe.
                Task.Run(() => ((App) Application.Current).StartOrKillNetworkManagerBasedOnSettings());

                OnPropertyChanged();
            }
        }

        public bool ToastsEnabled
        {
            get { return Settings.Instance.ToastsEnabled; }
            set
            {
                if (value == Settings.Instance.ToastsEnabled) return;
                Settings.Instance.ToastsEnabled = value;

                OnPropertyChanged();
            }
        }


        public bool DebugToastsEnabled
        {
            get { return Settings.Instance.DebugToastsEnabled; }
            set
            {
                if (value == Settings.Instance.DebugToastsEnabled) return;
                Settings.Instance.DebugToastsEnabled = value;

                OnPropertyChanged();
            }
        }

        /// <summary>Enable the local network to communicate with devices. tied directly to settings.</summary>
        public bool VirtualDeviceEnabled
        {
            get { return Settings.Instance.VirtualDeviceEnabled; }
            set
            {
                if (value == Settings.Instance.VirtualDeviceEnabled) return;
                Settings.Instance.VirtualDeviceEnabled = value;

                // StartOrKillNetworkManagerBasedOnSettings uses locking to make itself pool-safe.
              

                OnPropertyChanged();
            }
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

        public override void Kill()
        {
            Messenger.Instance.LocalNetworkConflict.Unsubscribe(_localNetworkConflictAction);
        }

        /// <summary>Signs out the twitter account by deleteing the creds, deleteing the database, stopping the
        /// live data and navigating back to the landing page.</summary>
        public void SignOut()
        {
            ((App) Application.Current).RootFrame.Navigate(typeof(SignOutView));
        }

        private void LocalNetworkConflictDetected(string ip)
        {
            IsNetworkConflict = true;
            _lastConflictDetected = DateTimeOffset.Now;
        }
    }
}
