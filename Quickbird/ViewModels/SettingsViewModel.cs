namespace Quickbird.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Internet;
    using LocalNetworking;
    using Models;
    using Util;
    using Views;

    public class SettingsViewModel : ViewModelBase
    {
        private static DateTimeOffset _lastConflictDetected = DateTimeOffset.MinValue;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        private bool _isNetworkConflict;
        private Frame _mainAppFrame;

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

        /// <summary>
        ///     Signs out the twitter account by deleteing the creds, deleteing the database, stopping the live data and navigating
        ///     back to the landing page.
        /// </summary>
        public async void SignOut()
        {
            try
            {
                await WebSocketConnection.Instance.Stop();
            }
            catch (Exception e)
            {
                Log.ShouldNeverHappen($"WebSocketConnection.Instance.Stop() in SettingsViewModel.SignOut() failed: {e}");
            }

            Settings.Instance.UnsetCreds();
            DeviceManagementEnabled = false;

            // Delete the database.
            var localFolder = ApplicationData.Current.LocalFolder;
            await (await localFolder.GetItemAsync(MainDbContext.FileName)).DeleteAsync();

            if (_mainAppFrame == null)
            {
                await Task.Delay(1000);
                if (_mainAppFrame == null)
                {
                    Log.ShouldNeverHappen(
                        "SettingsViewModel._mainAppFrame is null, this should have been set in the ctor.");
                    // Crashing the app is one way of going back to the first screen :D
                    throw new Exception("SettingsViewModel._mainAppFrame was never set to a proper value.");
                }
            }

            _mainAppFrame.Navigate(typeof(LandingPage));
        }

        private void LocalNetworkConflictDetected(string ip)
        {
            IsNetworkConflict = true;
            _lastConflictDetected = DateTimeOffset.Now;
        }

        public void SetMainAppFrame(Frame frame)
        {
            _mainAppFrame = frame;
        }
    }
}