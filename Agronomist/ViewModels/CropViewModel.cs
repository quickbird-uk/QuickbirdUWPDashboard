namespace Agronomist.ViewModels
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml.Controls;
    using Views;

    public class CropViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";

        private readonly Frame _cropContentFrame;

        private SharedCropRunViewModel _sharedCropRunViewModel;
        private DashboardViewModel _dashboardViewModel;

        private bool _isNotificationsOpen;
        private string _notificationsButtonText = ShowNotificationsString;
        private string _notificationsCount = "0";

        private bool _syncButtonEnabled = true;
        private string _test = "test";

        public CropViewModel(Frame cropContentFrame, SharedCropRunViewModel sharedCropRunViewModel)
        {
            _cropContentFrame = cropContentFrame;
            _sharedCropRunViewModel = sharedCropRunViewModel;
            _dashboardViewModel = new DashboardViewModel();
            _cropContentFrame.Navigate(typeof(Dashboard), sharedCropRunViewModel);
        }

        public bool SyncButtonEnabled
        {
            get { return _syncButtonEnabled; }
            set
            {
                if (value == _syncButtonEnabled) return;
                _syncButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public string Test
        {
            get { return _test; }
            set
            {
                if (value == _test) return;
                _test = value;
                OnPropertyChanged();
            }
        }

        public SharedCropRunViewModel SharedCropRunViewModel
        {
            get { return _sharedCropRunViewModel; }
            set
            {
                if (value == _sharedCropRunViewModel) return;
                _sharedCropRunViewModel = value;
                OnPropertyChanged();
            }
        }


        public string NotificationsCount
        {
            get { return _notificationsCount; }
            set
            {
                if (value == _notificationsCount) return;
                _notificationsCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Text changes when the notifications drawer is opened and closed.
        /// </summary>
        public string NotificationsButtonText
        {
            get { return _notificationsButtonText; }
            set
            {
                if (value == _notificationsButtonText) return;
                _notificationsButtonText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Bound two way because the notifications drawer closes automatically.
        /// </summary>
        public bool IsNotificationsOpen
        {
            get { return _isNotificationsOpen; }
            set
            {
                if (value == _isNotificationsOpen) return;
                _isNotificationsOpen = value;
                // Notifcations closing is an automatic popup event, so we need to handle it here.
                if (value == false)
                {
                    CloseNotifications();
                }
                OnPropertyChanged();
            }
        }
        
        public void NavToAddYield()
        {
            if (_sharedCropRunViewModel != null)
            {
                _cropContentFrame.Navigate(typeof(AddYieldView), _sharedCropRunViewModel.CropRunId);
            }
        }


        public void ToggleNotifications()
        {
            if (IsNotificationsOpen)
            {
                CloseNotifications();
            }
            else
            {
                OpenNotifications();
            }
        }

        private void OpenNotifications()
        {
            IsNotificationsOpen = true;
            NotificationsButtonText = "Hide Notifications";
        }

        private void CloseNotifications()
        {
            IsNotificationsOpen = false;
            NotificationsButtonText = ShowNotificationsString;
        }

        public void Sync(object sender, object e)
        {
            //TODO: Implement sync.
            Debug.WriteLine("Sync clicked, not implemented.");
        }
    }
}