namespace Agronomist.ViewModels
{
    using System;
    using Models;
    using NetLib;
    using Util;

    public class MainPageViewModel : ViewModelBase
    {
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

        private string _databaseErrors = "none";

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
            var settings = new Settings();
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

            var settings = new Settings();
            settings.SetNewCreds(creds.Token, creds.Userid, Guid.Parse(creds.StableSid.Remove(0,4)));

            UpdateCredsAndTokens();

            LoginEnabled = true;
        }

        public void DeleteCreds()
        {
            new Settings().UnsetCreds();
            UpdateCredsAndTokens();
        }

        public async void UpdateData()
        {
            using (var context = new MainDbContext())
            {
                var settings = new Settings();
                var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
                var errors = await context.UpdateFromServer(DateTimeOffset.MinValue, creds);
                DatabaseErrors = errors;
            }
        }
    }
}