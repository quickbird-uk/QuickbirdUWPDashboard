namespace Agronomist.ViewModels
{
    using System;
    using NetLib;

    internal class MainPageViewModel : ViewModelBase
    {
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
                creds = await NetLib.Creds.FromBroker(entryUrl, resultUrl);
            }
            catch (Exception)
            {
                LoginEnabled = true;
                return;
            }

            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["CredToken"] = creds.Token;
            roamingSettings.Values["CredUserId"] = creds.Userid;

        }
    }
}