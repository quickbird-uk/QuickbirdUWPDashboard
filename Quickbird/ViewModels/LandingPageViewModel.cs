namespace Quickbird.ViewModels
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Internet;
    using Util;
    using Views;

    public class LandingPageViewModel : ViewModelBase
    {
        private string _friendlyText = "Your Twitter Account is used to authenticate you.";
        private bool _loginEnabled;

        public string FriendlyText
        {
            get { return _friendlyText; }
            set
            {
                if (value == _friendlyText) return;
                _friendlyText = value;
                OnPropertyChanged();
            }
        }

        public bool LoginEnabled
        {
            get { return _loginEnabled; }
            set
            {
                if (value == _loginEnabled) return;
                _loginEnabled = value;
                OnPropertyChanged();
            }
        }

        public override void Kill()
        {
            // No special resources use here.
        }

        public async void Login()
        {
            LoginEnabled = false;
            const string entryUrl = "https://greenhouseapi.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://greenhouseapi.azurewebsites.net/.auth/login/done";

            if (!Request.IsInternetAvailable())
            {
                LoginEnabled = true;
                FriendlyText = "Internet seems unavailable, please check your connection and try again.";
                return;
            }

            Creds creds;
            try
            {
                creds = await Creds.FromBroker(entryUrl, resultUrl);
            }
            catch (Exception ex)
            {
                Util.LoggingService.LogInfo($"Login failed or cancelled with error: {ex}", Windows.Foundation.Diagnostics.LoggingLevel.Warning);
                FriendlyText = "Login failed or cancelled.";
                LoginEnabled = true;
                return;
            }

            var settings = SettingsService.Instance;
            settings.SetNewCreds(creds.Token, creds.Userid, Guid.Parse(creds.StableSid.Remove(0, 4)));

            UpdateCredsAndTokens();

            ((Frame) Window.Current.Content).Navigate(typeof(SyncingView));
        }

        private void UpdateCredsAndTokens()
        {
            var settings = SettingsService.Instance;
            Util.LoggingService.LogInfo(settings.CredToken ?? "No saved auth.", Windows.Foundation.Diagnostics.LoggingLevel.Verbose);
        }
    }
}
