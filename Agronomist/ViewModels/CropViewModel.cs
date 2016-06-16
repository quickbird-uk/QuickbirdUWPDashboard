namespace Agronomist.ViewModels
{
    public class CropViewModel : ViewModelBase
    {
        private string _test = "test";

        public string Test
        {
            get { return _test; }
            set
            {
                if (value == _test) return;
                _test = value;
                OnPropertyChanged();
            }
        }
    }
}