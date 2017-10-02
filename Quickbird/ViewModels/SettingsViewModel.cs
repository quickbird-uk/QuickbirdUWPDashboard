namespace Quickbird.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using LocalNetworking;
    using Util;
    using Views;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.DataTransfer;

    public class SettingsViewModel : ViewModelBase
    {
        private static DateTimeOffset _lastConflictDetected = DateTimeOffset.MinValue;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        private bool _isNetworkConflict;

        public SettingsViewModel()
        {
            _localNetworkConflictAction = LocalNetworkConflictDetected;
            BroadcasterService.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
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
            get { return SettingsService.Instance.LocalDeviceManagementEnabled; }
            set
            {
                if (value == SettingsService.Instance.LocalDeviceManagementEnabled) return;
                SettingsService.Instance.LocalDeviceManagementEnabled = value;

                // StartOrKillNetworkManagerBasedOnSettings uses locking to make itself pool-safe.
                Task.Run(() => ((App)Application.Current).StartOrKillNetworkManagerBasedOnSettings());

                OnPropertyChanged();
            }
        }

        public bool ToastsEnabled
        {
            get { return SettingsService.Instance.ToastsEnabled; }
            set
            {
                if (value == SettingsService.Instance.ToastsEnabled) return;
                SettingsService.Instance.ToastsEnabled = value;

                OnPropertyChanged();
            }
        }


        public bool DebugToastsEnabled
        {
            get { return SettingsService.Instance.DebugToastsEnabled; }
            set
            {
                if (value == SettingsService.Instance.DebugToastsEnabled) return;
                SettingsService.Instance.DebugToastsEnabled = value;

                OnPropertyChanged();
            }
        }

        /// <summary>Enable the local network to communicate with devices. tied directly to settings.</summary>
        public bool VirtualDeviceEnabled
        {
            get { return SettingsService.Instance.VirtualDeviceEnabled; }
            set
            {
                if (value == SettingsService.Instance.VirtualDeviceEnabled) return;
                SettingsService.Instance.VirtualDeviceEnabled = value;

                Services.VirtualDeviceService.UpdateBasedONSettings(); 

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

        public string AppVersion { get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format($"App Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
           }
        }
 
        public string MachineID => $"Machine ID: {SettingsService.Instance.MachineID}";

        /// <summary>
        /// Copies Twitter token to clipboard
        /// </summary>
        public void CopyTwitterToken() {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(SettingsService.Instance.CredToken);
            Clipboard.SetContent(dataPackage);
        }

        public void CopyTwitterUserID()
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(SettingsService.Instance.CredUserId);
            Clipboard.SetContent(dataPackage);
        }

        public override void Kill() => BroadcasterService.Instance.LocalNetworkConflict.Unsubscribe(_localNetworkConflictAction);

        /// <summary>Signs out the twitter account by deleteing the creds, deleteing the database, stopping the
        /// live data and navigating back to the landing page.</summary>
        public void SignOut() => ((App) Application.Current).RootFrame.Navigate(typeof(SignOutView));

        private void LocalNetworkConflictDetected(string ip)
        {
            IsNetworkConflict = true;
            _lastConflictDetected = DateTimeOffset.Now;
        }
    }
}
