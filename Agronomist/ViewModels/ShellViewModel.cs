namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using Windows.UI.Xaml.Controls;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;

        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

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

            if (ShellListViewModels.Count > 0)
                _contentFrame.Navigate(typeof(CropView), ShellListViewModels[0].CropViewModel);
            else
                _contentFrame.Navigate(typeof(AddCropCycleView));
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

        private void Update()
        {
            Debug.WriteLine("Shell update triggered");
            //TODO: Get data from abstraction

            foreach (var shellListViewModel in ShellListViewModels)
            {
                
            }
        }

        private object _selectedShellListViewModel;

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