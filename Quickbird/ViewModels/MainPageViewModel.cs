namespace Quickbird.ViewModels
{
    using System;
    using Internet;
    using Util;

    /// <summary>
    ///     This is a testing class not actually used in the app.
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        private string _databaseErrors = "none";
        private bool _loginEnabled = true;

        private string _token;

        public MainPageViewModel()
        {
            UpdateCredsAndTokens();
        }

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

        public string DatabaseErrors
        {
            get { return _databaseErrors; }
            set
            {
                if (value == _databaseErrors) return;
                _databaseErrors = value;
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

        private void UpdateCredsAndTokens()
        {
            var settings = Settings.Instance;
            Token = settings.CredToken ?? "No saved auth.";
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

            var settings = Settings.Instance;
            settings.SetNewCreds(creds.Token, creds.Userid, Guid.Parse(creds.StableSid.Remove(0, 4)));

            UpdateCredsAndTokens();

            LoginEnabled = true;
        }

        public void DeleteCreds()
        {
            Settings.Instance.UnsetCreds();
            UpdateCredsAndTokens();
        }

        public async void UpdateData()
        {
            var settings = Settings.Instance;
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var now = DateTimeOffset.Now;
            var errors = await DatabaseHelper.Instance.GetUpdatesFromServerAsync();
            DatabaseErrors = string.Join(",", errors);
            settings.LastDatabaseUpdate = now;
        }

        public async void PostSensorData()
        {
            DatabaseErrors = "posting";
            DatabaseErrors = await DatabaseHelper.Instance.PostHistoryAsync();
        }

        public async void PostDatabase()
        {
            DatabaseErrors = "posting";
            DatabaseErrors = string.Join(",", await DatabaseHelper.Instance.PostUpdatesAsync());
        }
    }
}