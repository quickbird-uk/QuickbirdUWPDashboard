namespace Agronomist.Views
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using JetBrains.Annotations;
    using ViewModels;

    public sealed partial class LiveCard : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(LiveCardViewModel), typeof(UserControl), new PropertyMetadata(null));

        const string Visible = "Visible";
        const string Collapsed = "Collapsed";

        private bool _alertsToggle;
        private string _mainVisibility = Visible;

        private string MainVisibility
        {
            get { return _mainVisibility; }
            set
            {
                _mainVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool? AlertsToggle
        {
            get { return _alertsToggle; }
            set
            {
                _alertsToggle = value ?? false;
                MainVisibility = (value ?? false) ? Collapsed : Visible;
                OnPropertyChanged();
            }
        }

        public LiveCard()
        {
            InitializeComponent();
            Bindings.Update();
        }

        /// <summary>
        ///     Dep prop for setting the ViewModel in an ItemTemplate.
        /// </summary>
        public LiveCardViewModel ViewModel
        {
            get { return (LiveCardViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}