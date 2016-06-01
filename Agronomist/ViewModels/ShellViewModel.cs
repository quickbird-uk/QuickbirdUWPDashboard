namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;
    using Models;
    using NetLib;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";

        private readonly Frame _contentFrame;

        private CropRunViewModel _currentCropRun;

        private bool _isNavOpen = true;

        private bool _isNotificationsOpen;

        private string _notificationsButtonText = ShowNotificationsString;

        private string _notificationsCount = "0";

        private ObservableCollection<CropRunViewModel> _runs = new ObservableCollection<CropRunViewModel>();

        /// <summary>
        /// Initialise the shell. 
        /// </summary>
        /// <param name="contentFrame">The frame that should be used for navigations.</param>
        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
            Update();
        }

        /// <summary>
        /// The collection of valid running cropruns.
        /// </summary>
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

        /// <summary>
        /// Text changes when the notifications drawer is opened and closed.
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
        /// Bound two way because the notifications drawer closes automatically.
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

        /// <summary>
        ///     Needs to be an opbjects to allow two-way binding with the ListView.
        /// </summary>
        public object CurrentCropRunAsObject
        {
            get { return CurrentCropRun; }
            set
            {
                var conv = value as CropRunViewModel;
                CurrentCropRun = conv;
                // Don't OnPropertyChanged, causes a Stack Overflow. Also pointless.
            }
        }

        /// <summary>
        ///     The current selected crop run.
        /// </summary>
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

        private void Update()
        {
            List<CropCycle> cropRuns = null;
            using (var db = new MainDbContext())
            {
                cropRuns = db.CropCycles.ToList();
            }

            // Fake data for testing.
            //cropRuns = FakeCropRuns();

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

            if (CurrentCropRun == null)
            {
                CurrentCropRun = _runs.FirstOrDefault();
            }
        }

        private static List<CropCycle> FakeCropRuns()
        {
            return new List<CropCycle>
            {
                new CropCycle
                {
                    ID = Guid.NewGuid(),
                    CropTypeName = "Cheese",
                    StartDate = DateTimeOffset.Now,
                    Location = new Location {Name = "Ga"}
                },
                new CropCycle
                {
                    ID = Guid.NewGuid(),
                    CropTypeName = "Goat",
                    StartDate = DateTimeOffset.Now,
                    Location = new Location {Name = "Site15"}
                }
            };
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

        public async void Sync()
        {
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
            }
        }
    }
}