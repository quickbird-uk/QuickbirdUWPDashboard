namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Internet;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;
        private readonly DispatcherTimer _internetCheckTimer;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _localNetworkConflictAction;
        private readonly DispatcherTimer _syncTimer;


        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private string _error = "";
        private bool _isInternetAvailable;

        private bool _isNavOpen = true;
        private object _selectedShellListViewModel;

        /// <summary>
        ///     Initialise the shell.
        /// </summary>
        /// <param name="contentFrame">The frame insided the shell used for most navigations.</param>
        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
            IsInternetAvailable = Request.IsInternetAvailable();

            UpdateInternetInViewModels(IsInternetAvailable);

            _internetCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            _internetCheckTimer.Tick += OnInternetCheckTimerTick;
            DispatcherTimers.Add(_internetCheckTimer);

            _internetCheckTimer.Start();
            
            _syncTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };

            _syncTimer.Tick += OnSyncTimerTick;
            DispatcherTimers.Add(_syncTimer);
            _syncTimer.Start();

            _updateAction = async s => await Update();

            Messenger.Instance.NewDeviceDetected.Subscribe(_updateAction);
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);
            _localNetworkConflictAction = s => NavToSettingsView();
            Messenger.Instance.LocalNetworkConflict.Subscribe(_localNetworkConflictAction);
        }

        public string Error
        {
            get { return _error; }
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public bool IsInternetAvailable
        {
            get { return _isInternetAvailable; }
            set
            {
                if (IsInternetAvailable == value) return;
                _isInternetAvailable = value;
                UpdateInternetInViewModels(value);
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

        public ObservableCollection<ShellListViewModel> ShellListViewModels { get; } =
            new ObservableCollection<ShellListViewModel>();

        public override void Kill()
        {
            Messenger.Instance.LocalNetworkConflict.Unsubscribe(_localNetworkConflictAction);
            Messenger.Instance.NewDeviceDetected.Unsubscribe(_updateAction);
            Messenger.Instance.TablesChanged.Unsubscribe(_updateAction);
            _internetCheckTimer.Stop();
            _internetCheckTimer.Tick -= OnInternetCheckTimerTick;
            _syncTimer.Stop();
            _syncTimer.Tick -= OnSyncTimerTick;

            foreach (var shellListViewModel in ShellListViewModels)
            {
                shellListViewModel.Kill();
            }
        }

        public void ListItemClicked(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ShellListViewModel;
            Debug.WriteLine($"ListItemClicked: {item?.BoxName ?? "null"}");
            if (item != null)
            {
                _contentFrame.Navigate(typeof(CropView), item.CropViewModel);
            }
        }

        public void NavToAddNewView()
        {
            _contentFrame.Navigate(typeof(AddCropCycleView));
            SelectedShellListViewModel = null;
        }

        public void NavToArchiveView()
        {
            _contentFrame.Navigate(typeof(ArchiveView));
            SelectedShellListViewModel = null;
        }

        public void NavToGraphingView()
        {
            _contentFrame.Navigate(typeof(GraphingView));
            SelectedShellListViewModel = null;
        }

        public void NavToSettingsView()
        {
            if (_contentFrame.CurrentSourcePageType != typeof(SettingsView))
                _contentFrame.Navigate(typeof(SettingsView));
            SelectedShellListViewModel = null;
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

        public async void FirstUpdate()
        {
            Debug.WriteLine("Running First Update");

            try
            {
                await Update();
            }
            catch (Exception e)
            {
                Error = e.ToString();
                Log.ShouldNeverHappen($"ShellViewModel.FirstUpdate() {e}");
            }


            if (ShellListViewModels.Count > 0)
            {
                var item = ShellListViewModels[0].CropViewModel;
                _contentFrame.Navigate(typeof(CropView), item);
                SelectedShellListViewModel = item;
            }
            else
            {
                _contentFrame.Navigate(typeof(AddCropCycleView));
                SelectedShellListViewModel = null;
            }
        }

        private void OnInternetCheckTimerTick(object sender, object o)
        {
            IsInternetAvailable = Request.IsInternetAvailable();
        }

        /// <summary>
        ///     An infinite loop that uses a delay instead of a time so that it is not reentrant.
        /// </summary>
        private async void OnSyncTimerTick(object sender, object other)
        {
            _syncTimer.Stop();
            Debug.WriteLine("Auto Sync started...");
            // Disables the sync button in every CropView (there is one for each crop).
            await SetSyncEnabled(false);

            await DatabaseHelper.Instance.Sync();

            await SetSyncEnabled(true);

            Debug.WriteLine("...Auto Sync finished.");
            _syncTimer.Start();
        }

        /// <summary>
        ///     Enable or disable the Sync button in every cropview, this task waits until the bound variable is sucessfully set.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        private async Task SetSyncEnabled(bool enabled)
        {
            var dispatcher = ((App) Application.Current).Dispatcher;
            if (dispatcher == null)
            {
                Log.ShouldNeverHappen($"Messenger.Instance.Dispatcher null at ShellViewModel.SetSyncEnabled()");
                throw new Exception("The app dispatcher is missing: ShellViewModel.SetSyncEnabled()");
            }

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

        private async Task Update()
        {
            Debug.WriteLine("Shell update triggered");

            var cropCycles = await DatabaseHelper.Instance.GetDataTreeAsync();

            // Remove items that no longer exist.
            var now = DateTimeOffset.Now;
            var validIds =
                cropCycles.Where(cc => !cc.Deleted && (cc.EndDate ?? DateTimeOffset.MaxValue) > now)
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
                if (!validIds.Contains(cropCycle.ID)) continue;

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

            // Navigate away if the currently selected cropcycle has been removed.
            if (!ShellListViewModels.Contains(SelectedShellListViewModel))
            {
                if (_contentFrame.Content is Dashboard)
                {
                    if (ShellListViewModels.Any())
                    {
                        var item = ShellListViewModels[0].CropViewModel;
                        _contentFrame.Navigate(typeof(CropView), item);
                        SelectedShellListViewModel = item;
                    }
                    else
                    {
                        _contentFrame.Navigate(typeof(AddCropCycleView));
                        SelectedShellListViewModel = null;
                    }
                }
                else
                {
                    // It should already be null.
                    SelectedShellListViewModel = null;
                }
            }

            UpdateInternetInViewModels(IsInternetAvailable);
        }

        private void UpdateInternetInViewModels(bool isInternetAvailable)
        {
            foreach (var shellListViewModel in ShellListViewModels)
            {
                shellListViewModel.UpdateInternetStatus(isInternetAvailable);
            }
        }
    }
}
