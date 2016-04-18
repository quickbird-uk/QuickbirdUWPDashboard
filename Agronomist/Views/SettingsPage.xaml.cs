namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Template10.Services.SerializationService;

    public sealed partial class SettingsPage : Page
    {
        private readonly ISerializationService _serializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _serializationService = SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_serializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;
        }
    }
}