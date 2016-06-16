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

        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private CropRunViewModel _currentCropRun;

        private bool _isNavOpen = true;

        private ObservableCollection<CropRunViewModel> _runs = new ObservableCollection<CropRunViewModel>();


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

        public bool IsCropRunSet => _currentCropRun != null;

        /// <summary>
        ///     The current selected crop run.
        /// </summary>
        public CropRunViewModel CurrentCropRun
        {
            get { return _currentCropRun; }
            set
            {
                _currentCropRun = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCropRunSet));

                var viewModel = _dashboardViewModels.FirstOrDefault(dvm => dvm.CropId == value?.CropRunId)
                                ?? _dashboardViewModels.FirstOrDefault();

                if (viewModel != null)
                    _contentFrame.Navigate(typeof(Dashboard), viewModel);
                else
                //TODO This should navigate to some sort of guidance page
                    _contentFrame.Navigate(typeof(Dashboard));
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
            Debug.WriteLine("Shell update triggered");
            var now = DateTimeOffset.Now;
            List<CropCycle> allCropRuns = DatabaseHelper.Instance.GetDatatree();
            var unfinishedCropRuns = allCropRuns.Where(cc => cc.EndDate == null || cc.EndDate > now).ToList();
            var cropRuns = unfinishedCropRuns.Where(cc => cc.Deleted == false).ToList();

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