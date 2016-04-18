namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Template10.Services.SerializationService;

    public sealed partial class SettingsPage : Page
    {
        private readonly ISerializationService _SerializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;
        }
    }
}