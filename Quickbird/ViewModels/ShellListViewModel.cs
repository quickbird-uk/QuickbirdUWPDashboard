namespace Quickbird.ViewModels
{
    using System;
    using System.Linq;
    using DbStructure.User;

    public class ShellListViewModel : ViewModelBase
    {
        public const string Visible = "Visible";
        public const string Collapsed = "Collapsed";
        private string _boxName;
        private string _cropName;

        private CropViewModel _cropViewModel;
        private string _iconLetter;

        private bool _isAlerted;

        public ShellListViewModel(CropCycle cropCycle)
        {
            CropRunId = cropCycle.ID;
            CropViewModel = new CropViewModel(cropCycle);
            Update(cropCycle);
        }

        public Guid CropRunId { get; }


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

        public string IconLetter
        {
            get { return _iconLetter; }
            set
            {
                if (value == _iconLetter) return;
                _iconLetter = value;
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

        public bool IsAlerted
        {
            get { return _isAlerted; }
            set
            {
                if (value == _isAlerted) return;
                _isAlerted = value;
                OnPropertyChanged();
            }
        }

        public CropViewModel CropViewModel
        {
            get { return _cropViewModel; }
            set
            {
                if (value == _cropViewModel) return;
                _cropViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Provides the shell the ability to enable and disable the sync button on every CropView.
        ///     Ideally the sync button should be part of the shell but we havn't found a nice place to put it.
        /// </summary>
        public bool CropViewSyncButtonEnabled
        {
            set { CropViewModel.SyncButtonEnabled = value; }
        }

        public void Update(CropCycle cropCycle)
        {
            CropName = cropCycle.CropTypeName;
            BoxName = cropCycle.Location.Name;
            IconLetter = CropName.Substring(0, 1);
            CropViewModel.Update(cropCycle);

            IsAlerted = cropCycle.Location.Devices.SelectMany(s => s.Sensors).Any(s => s.Alarmed);

            _cropViewModel.Update(cropCycle);
        }

        public void UpdateInternetStatus(bool isInternetAvailable)
        {
            CropViewModel.UpdateInternetStatus(isInternetAvailable);
        }
    }
}