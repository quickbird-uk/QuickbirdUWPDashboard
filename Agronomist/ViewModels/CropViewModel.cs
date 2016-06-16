namespace Agronomist.ViewModels
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml.Controls;
    using Models;
    using NetLib;
    using Util;
    using Views;

    public class CropViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";

        private readonly Frame _contentFrame;

        private CropRunViewModel _currentCropRun;
        private bool _syncButtonEnabled = true;
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

        private bool _isNotificationsOpen;
        private string _notificationsButtonText = ShowNotificationsString;
        private string _notificationsCount = "0";
        private string _test = "test";

        public CropViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
            Update();
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

        public CropRunViewModel CurrentCropRun
        {
            get { return _currentCropRun; }
            set
            {
                if (value == _currentCropRun) return;
                _currentCropRun = value;
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

        public bool IsCropRunSet => _currentCropRun != null;

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

        private void Update()
        {
            throw new NotImplementedException();
        }

        public void NavToAddYield()
        {
            if (_currentCropRun != null)
            {
                _contentFrame.Navigate(typeof(AddYieldView), _currentCropRun.CropRunId);
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

        public async void Sync(object sender, object e)
        {
            SyncButtonEnabled = false;
            using (var context = new MainDbContext())
            {
                var settings = new Settings();
                var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
                var now = DateTimeOffset.Now;
                var errors = await context.UpdateFromServer(settings.LastDatabaseUpdate, creds);
                settings.LastDatabaseUpdate = now;
                Debug.WriteLine(errors);

                var posterrors = string.Join(",", await context.PostChanges());
                Debug.WriteLine(posterrors);

                Debug.WriteLine(await context.PostHistoryChanges());
            }
            SyncButtonEnabled = true;
        }
    }
}