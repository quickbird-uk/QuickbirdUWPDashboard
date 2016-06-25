namespace Agronomist.ViewModels
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
        private bool _loginEnabled;

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

        private string _friendlyText = "Your Twitter Account is used to authenticate you.";

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

        private void UpdateCredsAndTokens()
        {
            var settings = Settings.Instance;
            Debug.WriteLine(settings.CredToken ?? "No saved auth.");
        }

        public async void Login()
        {

            LoginEnabled = false;
            const string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";
            
            if (!Internet.Request.IsInternetAvailable())
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
            catch (Exception)
            {
                Debug.WriteLine("Login failed or cancelled.");
                FriendlyText = "Login failed or cancelled.";
                LoginEnabled = true;
                return;
            }

            var settings = Settings.Instance;
            settings.SetNewCreds(creds.Token, creds.Userid, Guid.Parse(creds.StableSid.Remove(0, 4)));

            UpdateCredsAndTokens();

            ((Frame) Window.Current.Content).Navigate(typeof(Shell));
        }
    }
}