namespace Agronomist.ViewModels
{
    using System;
    using DatabasePOCOs.User;

    public class CropRunViewModel : ViewModelBase
    {
        private string _boxName;

        private string _cropName;

        private string _iconLetter;

        private bool _isAlerted;

        private string _plantingDate;

        private string _varietyName;

        private string _yield;


        public CropRunViewModel(CropCycle cropRun)
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


        public void Update(CropCycle cropRun)
        {
            CropRunId = cropRun.ID;
            CropName = cropRun.Name;
            VarietyName = cropRun.CropTypeName;
            PlantingDate = cropRun.StartDate.ToString("dd/MM/yyyy");
            BoxName = cropRun.Location.Name;
            IconLetter = CropName.Substring(0, 1);
            Yield = "2 bushels";
            //TODO figure out alerts
        }
    }
}