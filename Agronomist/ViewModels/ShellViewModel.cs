namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml.Controls;
    using DatabasePOCOs.User;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using NetLib;
    using Util;
    using Views;
    using Windows.UI.Xaml;

    public class ShellViewModel : ViewModelBase
    {
        private const string ShowNotificationsString = "Show Notifications";

        private readonly Frame _contentFrame;


        DispatcherTimer _syncTimer; 
        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private CropRunViewModel _currentCropRun;
        private readonly List<DashboardViewModel> _dashboardViewModels = new List<DashboardViewModel>();

        private bool _isNavOpen = true;

        private bool _isNotificationsOpen;

        private string _notificationsButtonText = ShowNotificationsString;

        private string _notificationsCount = "0";

        private ObservableCollection<CropRunViewModel> _runs = new ObservableCollection<CropRunViewModel>();

        private bool _syncButtonEnabled = true;

        /// <summary>
        ///     Initialise the shell.
        /// </summary>
        /// <param name="contentFrame">The frame that should be used for navigations.</param>
        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
            Update();
            _updateAction = s => Update();

            Messenger.Instance.NewDeviceDetected.Subscribe(_updateAction);
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);

            _syncTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _syncTimer.Tick += Sync;
            _syncTimer.Start();


            if (_runs.Count > 0)
                _contentFrame.Navigate(typeof(Dashboard),
                    _dashboardViewModels[0]);
            else
                //TODO This should navigate to some sort of guidance page, that tells people to add a box, etc. 
                _contentFrame.Navigate(typeof(Dashboard));
        }

        /// <summary>
        ///     The collection of valid running cropruns.
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
                OnPropertyChanged("IsCropRunSet");

                var viewModel = _dashboardViewModels.FirstOrDefault(dvm => dvm.CropId == value?.CropRunId)
                    ?? _dashboardViewModels.FirstOrDefault();

                if (viewModel != null)
                    _contentFrame.Navigate(typeof(Dashboard), viewModel);
                else
                    //TODO This should navigate to some sort of guidance page
                    _contentFrame.Navigate(typeof(Dashboard)); 


            }
        }

        public bool IsCropRunSet
        {
            get { return _currentCropRun != null; }
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

        private void Update()
        {
            Debug.WriteLine("Shell update triggered");
            List<CropCycle> cropRuns;
            using (var db = new MainDbContext())
            {
                var now = DateTimeOffset.Now;
                var allCropRuns = db.CropCycles
                    .Include(cc => cc.Location)
                    .Include(cc => cc.CropType)
                    .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Param)
                    .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Place)
                    .AsNoTracking();
                var unfinishedCropRuns = allCropRuns.Where(cc => cc.EndDate == null || cc.EndDate > now).ToList();
                var validCropRuns = unfinishedCropRuns.Where(cc => cc.Deleted == false);
                cropRuns = validCropRuns.ToList();
            }

            UpdateCropRunVMs(cropRuns);

            UpdateDashboardVMs(cropRuns);

            //At The end, update CropRun selection
            if (CurrentCropRun == null)
            {
                CurrentCropRun = _runs.FirstOrDefault();
            }
        }

        private void UpdateDashboardVMs(List<CropCycle> cropRuns)
        {
            var invalidDashVMs =
                _dashboardViewModels.Where(dvm => cropRuns.FirstOrDefault(cr => cr.ID == dvm.CropId) == null).ToList();
            foreach (var invalidDashVm in invalidDashVMs)
            {
                _dashboardViewModels.Remove(invalidDashVm);
            }
            foreach (var run in cropRuns)
            {
                var existing = _dashboardViewModels.FirstOrDefault(dvm => dvm.CropId == run.ID);
                if (null == existing)
                {
                    var dvm = new DashboardViewModel(run);
                    _dashboardViewModels.Add(dvm);
                }
                else
                {
                    existing.Update(run);
                }
            }
        }

        private void UpdateCropRunVMs(List<CropCycle> cropRuns)
        {
// Remove no longer valid items.
            var invalid = Runs.Where(r => cropRuns.FirstOrDefault(cr => cr.ID == r.CropRunId) == null).ToList();
            foreach (var invalidRun in invalid)
            {
                Runs.Remove(invalidRun);
            }

            // Update or Add valid items.
            foreach (var run in cropRuns)
            {
                var exisiting = _runs.FirstOrDefault(r => r.CropRunId == run.ID);
                if (null == exisiting)
                {
                    var vm = new CropRunViewModel(run);
                    Runs.Add(vm);
                }
                else
                {
                    exisiting.Update(run);
                }
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

        public void NavToAddNewView()
        {
            _contentFrame.Navigate(typeof(AddCropCycleView));
        }

        
        public void NavToArchiveView()
        {
            _contentFrame.Navigate(typeof(ArchiveView));
            
        }


        public void NavToAddYield()
        {
            if (_currentCropRun != null)
            {
                _contentFrame.Navigate(typeof(AddYieldView), _currentCropRun.CropRunId);
            }
        }

        public void Sync(object sender, object e)
        {
            Sync(); 
        }

        public async void Sync()
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