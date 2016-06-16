namespace Agronomist.ViewModels
{
    using System;
    using DatabasePOCOs.User;

    public class SharedCropRunViewModel : ViewModelBase
    {
        private string _boxName;

        private string _cropName;

        private string _iconLetter;

        private bool _isAlerted;

        private string _plantingDate;

        private string _varietyName;

        private string _yield;

        /// <summary>
        ///     Initialises the properties of this viewmodel with data from POCO.
        /// </summary>
        /// <param name="cropRun">Requires CropType and Location to be included.</param>
        public SharedCropRunViewModel(CropCycle cropRun)
        {
            Update(cropRun);
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

        /// <summary>
        /// Used to set alert UI elements to visible or hidden. Change status by modifying backing bool.
        /// </summary>
        public string IsAlerted => _isAlerted ? "Visible" : "Collapsed";

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
            IconLetter = CropName.Substring(0, 1);
            Yield = $"{cropRun.Yield}kg";
            _isAlerted = false; //TODO: IsAnySensorAlerted();
        }
    }
}