namespace Quickbird.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using DbStructure.User;
    using Util;
    using Views;

    public class CropViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";
        private readonly DashboardViewModel _dashboardViewModel;

        private readonly Guid _id;

        private string _boxName;

        private Frame _cropContentFrame;
        private string _cropName;

        private bool _isNotificationsOpen;
        private string _notificationsButtonText = ShowNotificationsString;
        private string _notificationsCount = "0";
        private string _plantingDate;

        private bool _syncButtonEnabled = true;
        private string _varietyName;
        private string _yield;

        public CropViewModel(CropCycle cropCycle)
        {
            _id = cropCycle.ID;
            _dashboardViewModel = new DashboardViewModel(cropCycle);
            Update(cropCycle);
        }

        public Guid CropRunId { get; set; }

        public string CropName
        {
            get { return _cropName; }
            set
            {
                if (value == _cropName) return;
                _cropName = value;
                OnPropertyChanged();
            }
        }

        public string VarietyName
        {
            get { return _varietyName; }
            set
            {
                if (value == _varietyName) return;
                _varietyName = value;
                OnPropertyChanged();
            }
        }

        public string BoxName
        {
            get { return _boxName; }
            set
            {
                if (value == _boxName) return;
                _boxName = value;
                OnPropertyChanged();
            }
        }

        public string PlantingDate
        {
            get { return _plantingDate; }
            set
            {
                if (value == _plantingDate) return;
                _plantingDate = value;
                OnPropertyChanged();
            }
        }

        public string Yield
        {
            get { return _yield; }
            set
            {
                if (value == _yield) return;
                _yield = value;
                OnPropertyChanged();
            }
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

        public void SetContentFrame(Frame contentFrame)
        {
            _cropContentFrame = contentFrame;
            _cropContentFrame.Navigate(typeof(Dashboard), _dashboardViewModel);
        }

        /// <summary>
        ///     Updates the properties of this viewmodel with data from POCO.
        /// </summary>
        /// <param name="cropRun">Requires CropType (for Variety) and Location (for name) to be included.</param>
        public void Update(CropCycle cropRun)
        {
            CropRunId = cropRun.ID;
            CropName = cropRun.CropTypeName;
            VarietyName = cropRun.CropVariety;
            PlantingDate = cropRun.StartDate.ToString("dd/MM/yyyy");
            BoxName = cropRun.Location.Name;
            Yield = $"{cropRun.Yield}kg";
            _dashboardViewModel.Update(cropRun);
        }

        public void NavToAddYield()
        {
            _cropContentFrame?.Navigate(typeof(AddYieldView), _id);
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

            var updateErrors = await DatabaseHelper.Instance.GetUpdatesFromServerAsync();
            if (updateErrors?.Any() ?? false) Debug.WriteLine(updateErrors);

            var postErrors = await DatabaseHelper.Instance.PostUpdatesAsync();
            if (postErrors?.Any() ?? false) Debug.WriteLine(string.Join(",", postErrors));

            var postHistErrors = await DatabaseHelper.Instance.PostHistoryAsync();
            if(postHistErrors?.Any() ?? false) Debug.WriteLine(postHistErrors);

            SyncButtonEnabled = true;
        }
    }
}