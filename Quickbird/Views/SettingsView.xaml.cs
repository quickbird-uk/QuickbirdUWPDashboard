namespace Quickbird.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;
    using Util;

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

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SignOutButton.IsEnabled = false; 
            await Internet.WebSocketConnection.Instance.Stop();
            Settings.Instance.UnsetCreds();           
            //TODO: delete Database
            //TODO: Stop listening to local devices
        }
    }
}