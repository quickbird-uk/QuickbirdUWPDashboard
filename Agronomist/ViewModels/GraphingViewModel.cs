namespace Agronomist.ViewModels
{
    public class GraphingViewModel : ViewModelBase
    {
        private string _sampleProperty = "default sample property value";

        public string SampleProperty
        {
            get { return _sampleProperty; }
            set
            {
                if(value == _sampleProperty) return;
                _sampleProperty = value;
                OnPropertyChanged();
            }
        }
    }
}