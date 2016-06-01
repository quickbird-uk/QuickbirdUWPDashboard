namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using DatabasePOCOs.User;
    using Models;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";
        private readonly Frame _contentFrame;

        private bool _isNavOpen = true;

        private bool _isNotificationsOpen;
        private string _notificationsButtonText = ShowNotificationsString;

        private string _notificationsCount = "0";

        private ObservableCollection<CropRunViewModel> _runs = new ObservableCollection<CropRunViewModel>();
       
        public ObservableCollection<CropRunViewModel> Runs
        {
            get { return _runs; }
            set
            {
                if (value == _runs) return;
                _runs = value;
                OnPropertyChanged();
            }
        }

        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
            Update();
        }

        private void Update()
        {
            List<CropCycle> cropRuns = null;
            using (var db = new MainDbContext())
            {
                //TODO: Load cropruns from DB.
            }

#if DEBUG
            // Fake data for testing.
            cropRuns = new List<CropCycle>
                {
                    new CropCycle()
                    {
                        ID = Guid.NewGuid(),
                        CropTypeName = "Sweet",
                        Name = "Peach",
                        StartDate = DateTimeOffset.Now,
                        Location = new Location() {Name = "Ga"},
                    },
                    new CropCycle()
                    {
                        ID =Guid.NewGuid(),
                        CropTypeName = "Mountain",
                        Name = "Goat",
                        StartDate = DateTimeOffset.Now,
                        Location = new Location() {Name = "Site15"}
                    }
                };

#endif
            foreach (var run in cropRuns)
            {
                var exisiting = _runs.FirstOrDefault(r => r.CropRunId == run.ID);
                if (null == exisiting)
                {
                    var vm = new CropRunViewModel(run);
                    _runs.Add(vm);
                }
                else
                {
                    exisiting.Update(run);
                }
            }
        }

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

        private CropRunViewModel _currentCropRun;

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

        public bool IsNavOpen
        {
            get { return _isNavOpen; }
            set
            {
                if (value == _isNavOpen) return;
                _isNavOpen = value;
                OnPropertyChanged();
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

        public void ToggleNav()
        {
            Debug.WriteLine("Toggle Nav.");
            if (IsNavOpen)
            {
                Debug.WriteLine("Close.");
                IsNavOpen = false;
            }
            else
            {
                Debug.WriteLine("Open.");
                IsNavOpen = true;
            }
        }

        public void NavToGraphingView()
        {
            _contentFrame.Navigate(typeof(GraphingView));
        }
    }
}