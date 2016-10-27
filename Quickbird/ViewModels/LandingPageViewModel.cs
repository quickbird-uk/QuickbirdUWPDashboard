namespace Quickbird.ViewModels
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Internet;
    using Views;

    public class LandingPageViewModel : ViewModelBase
    {
        private string _email;
        private string _friendlyText = "Please enter your username and password to login or click register to create a new account.";
        private bool _loginEnabled = true;
        private string _password;

        public string Email
        {
            get { return _email; }
            set
            {
                if (Email == value) return;
                _email = value;
                OnPropertyChanged();
            }
        }

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

        public string Password
        {
            get { return _password; }
            set
            {
                if (Password == value) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public override void Kill()
        {
            // No special resources use here.
        }

        public void Register()
        {
            ((Frame) Window.Current.Content).Navigate(typeof(RegisterView), Tuple.Create(Email, Password));
        }

        public async void Login()
        {
            LoginEnabled = false;

            if (!Request.IsInternetAvailable())
            {
                LoginEnabled = true;
                FriendlyText = "Internet appears to be unavailable, please check your connection and try again.";
                return;
            }

            try
            {

            }
            catch (Exception)
            {
                Debug.WriteLine("Login failed or cancelled.");
                FriendlyText = "Login failed or cancelled.";
                LoginEnabled = true;
                return;
            }

            ((Frame) Window.Current.Content).Navigate(typeof(SyncingView));
        }
    }
}
