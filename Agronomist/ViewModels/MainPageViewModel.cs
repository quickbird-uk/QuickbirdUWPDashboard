namespace Agronomist.ViewModels
{
    using System;
    using NetLib;

    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            var settings = new Settings();
            _token = settings.CredToken ?? "No saved auth.";
        }

        private string _token;

        public string Token
        {
            get { return _token; }
            set
            {
                if (value == _token) return;
                _token = value;
                OnPropertyChanged();
            }
        }

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

        public async void Login()
        {
            LoginEnabled = false;
            const string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";

            Creds creds;
            try
            {
                creds = await Creds.FromBroker(entryUrl, resultUrl);
            }
            catch (Exception)
            {
                LoginEnabled = true;
                return;
            }

            var settings = new Settings
            {
                CredToken = creds.Token,
                CredUserId = creds.Userid
            };
        }
    }
}