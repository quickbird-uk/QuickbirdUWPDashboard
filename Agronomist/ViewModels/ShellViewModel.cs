namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using DatabasePOCOs.User;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using NetLib;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;
        private readonly List<DashboardViewModel> _dashboardViewModels = new List<DashboardViewModel>();
        private ObservableCollection<SharedCropRunViewModel> _sharedCropRunViewModels = new ObservableCollection<SharedCropRunViewModel>();

        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private SharedCropRunViewModel _currentSharedCropRun;

        private bool _isNavOpen = true;



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

            if (_sharedCropRunViewModels.Count > 0)
                _contentFrame.Navigate(typeof(CropView),
                    _sharedCropRunViewModels[0]);
            else
                _contentFrame.Navigate(typeof(AddCropCycleView));
        }

        /// <summary>
        ///     The collection of valid running cropruns.
        /// </summary>
        public ObservableCollection<SharedCropRunViewModel> SharedCropRunViewModels
        {
            get { return _sharedCropRunViewModels; }
            set
            {
                if (value == _sharedCropRunViewModels) return;
                _sharedCropRunViewModels = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        ///     Needs to be an opbjects to allow two-way binding with the ListView.
        /// </summary>
        public object CurrentCropRunAsObject
        {
            get { return CurrentSharedCropRun; }
            set
            {
                var conv = value as SharedCropRunViewModel;
                CurrentSharedCropRun = conv;
                // Don't OnPropertyChanged, causes a Stack Overflow. Also pointless.
            }
        }

        public bool IsCropRunSet => _currentSharedCropRun != null;

        /// <summary>
        ///     The current selected crop run.
        /// </summary>
        public SharedCropRunViewModel CurrentSharedCropRun
        {
            get { return _currentSharedCropRun; }
            set
            {
                _currentSharedCropRun = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCropRunSet));

                var dashboardViewModel = _dashboardViewModels.FirstOrDefault(dvm => dvm.CropId == value?.CropRunId)
                                ?? _dashboardViewModels.FirstOrDefault();

                if (dashboardViewModel != null)
                    _contentFrame.Navigate(typeof(CropView), dashboardViewModel);
                else
                    _contentFrame.Navigate(typeof(AddCropCycleView));
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

            //At The end, update CropRun selection
            if (CurrentSharedCropRun == null)
            {
                CurrentSharedCropRun = _sharedCropRunViewModels.FirstOrDefault();
            }
        }

        private void UpdateCropRunVMs(List<CropCycle> cropRuns)
        {
            // Remove no longer valid items.
            var invalid = SharedCropRunViewModels.Where(r => cropRuns.FirstOrDefault(cr => cr.ID == r.CropRunId) == null).ToList();
            foreach (var invalidRun in invalid)
            {
                SharedCropRunViewModels.Remove(invalidRun);
            }

            // Update or Add valid items.
            foreach (var run in cropRuns)
            {
                var exisiting = _sharedCropRunViewModels.FirstOrDefault(r => r.CropRunId == run.ID);
                if (null == exisiting)
                {
                    var vm = new SharedCropRunViewModel(run);
                    SharedCropRunViewModels.Add(vm);
                }
                else
                {
                    exisiting.Update(run);
                }
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
    }
}