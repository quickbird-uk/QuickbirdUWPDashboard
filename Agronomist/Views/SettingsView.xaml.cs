namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;

    /// <summary>
    ///     All sorts of settings are viewed and edited here.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel = new SettingsViewModel();

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}