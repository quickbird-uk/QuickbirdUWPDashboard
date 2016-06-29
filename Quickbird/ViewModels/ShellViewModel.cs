namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Windows.UI.Xaml.Controls;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;

        private readonly bool _killTimer = false;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;

        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private bool _isNavOpen = true;

        private object _selectedShellListViewModel;

        /// <summary>
        ///     Initialise the shell.
        /// </summary>
        /// <param name="contentFrame">The frame that should be used for navigations.</param>
        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;

            FirstUpdate();

            Task.Run(() => RunUpdateTimer());

            _updateAction = async s => await Update();

            Messenger.Instance.NewDeviceDetected.Subscribe(_updateAction);
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);
            _localNetworkConflictAction = s => NavToSettingsView();
            Messenger.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
        }

        public ObservableCollection<ShellListViewModel> ShellListViewModels { get; } =
            new ObservableCollection<ShellListViewModel>();

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

        public object SelectedShellListViewModel
        {
            get { return _selectedShellListViewModel; }
            set
            {
                if (value == _selectedShellListViewModel) return;
                _selectedShellListViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     An infinite loop that uses a delay instead of a time so that it is not reentrant.
        /// </summary>
        private async void RunUpdateTimer()
        {
            while (!_killTimer)
            {
                Debug.WriteLine("Auto Sync started...");
                // Disables the sync button in every CropView (there is one for each crop).
                await SetSyncEnabled(false);

                var updateErrors = await DatabaseHelper.Instance.GetUpdatesFromServerAsync();
                if (updateErrors?.Any() ?? false) Debug.WriteLine(updateErrors);

                var postErrors = await DatabaseHelper.Instance.PostUpdatesAsync();
                if (postErrors?.Any() ?? false) Debug.WriteLine(string.Join(",", postErrors));

                var postHistErrors = await DatabaseHelper.Instance.PostHistoryAsync();
                if (postHistErrors?.Any() ?? false) Debug.WriteLine(postHistErrors);


                await SetSyncEnabled(true);

                Debug.WriteLine("...Auto Sync finished.");

                //Run timer at the end so that the program starts with an update.
                // Use a delay to space out syncs, stops it from being reentrant.
                // We don't care about the exact timing.
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        /// <summary>
        ///     Enable or disable the Sync button in every cropview, this task waits until the bound variable is sucessfully set.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        private async Task SetSyncEnabled(bool enabled)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            var completer = new TaskCompletionSource<bool>();

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var shellListViewModel in ShellListViewModels)
                {
                    shellListViewModel.CropViewSyncButtonEnabled = enabled;
                }

                completer.SetResult(true);
            });

            await completer.Task;
        }

        public void ListItemClickedOrChanged()
        {
            var item = SelectedShellListViewModel as ShellListViewModel;
            if (item == null)
            {
                _contentFrame.Navigate(typeof(AddCropCycleView));
            }
            else
            {
                _contentFrame.Navigate(typeof(CropView), item.CropViewModel);
            }
        }

        private async void FirstUpdate()
        {
            Debug.WriteLine("Running First Update");

            await Update();

            if (ShellListViewModels.Count > 0)
            {
                var item = ShellListViewModels[0].CropViewModel;
                _contentFrame.Navigate(typeof(CropView), item);
                SelectedShellListViewModel = item;
            }
            else
                _contentFrame.Navigate(typeof(AddCropCycleView));
        }

        private async Task Update()
        {
            Debug.WriteLine("Shell update triggered");

            var cropCycles = await DatabaseHelper.Instance.GetDataTreeAsync();

            // Remove items that no longer exist.
            var now = DateTimeOffset.Now;
            var validIds =
                cropCycles.Where(cc => !cc.Deleted && (cc.EndDate ?? DateTimeOffset.MaxValue) < now)
                    .Select(cc => cc.ID)
                    .ToList();
            var toRemove = ShellListViewModels.Where(s => !validIds.Contains(s.CropRunId));
            foreach (var invalidItem in toRemove)
            {
                ShellListViewModels.Remove(invalidItem);
            }

            // Add new items, update existing.
            foreach (var cropCycle in cropCycles)
            { 
                if(!validIds.Contains(cropCycle.ID)) continue;

                var item = ShellListViewModels.FirstOrDefault(s => s.CropRunId == cropCycle.ID);
                if (null == item)
                {
                    ShellListViewModels.Add(new ShellListViewModel(cropCycle));
                }
                else
                {
                    item.Update(cropCycle);
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

        public void NavToSettingsView()
        {
            if (_contentFrame.CurrentSourcePageType != typeof(SettingsView))
                _contentFrame.Navigate(typeof(SettingsView));
        }
    }
}