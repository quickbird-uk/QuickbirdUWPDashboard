namespace Quickbird.ViewModels
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Views;

    public class RegisterViewModel : ViewModelBase
    {
        private string _email;
        private string _infoText;
        private string _password;
        private string _phone;
        private string _problems;
        private bool _registerEnable = true;
        private string _repeatPassword;
        private string _username;

        public string Email
        {
            get { return _email; }
            set
            {
                if (value == Email) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        public string InfoText
        {
            get { return _infoText; }
            set
            {
                if (value == InfoText) return;
                _infoText = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == Password) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                if (value == Phone) return;
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string Problems
        {
            get { return _problems; }
            set
            {
                if (value == Problems) return;
                _problems = value;
                OnPropertyChanged();
            }
        }

        public bool RegisterEnable
        {
            get { return _registerEnable; }
            set
            {
                if (value == RegisterEnable) return;
                _registerEnable = value;
                OnPropertyChanged();
            }
        }

        public string RepeatPassword
        {
            get { return _repeatPassword; }
            set
            {
                if (value == RepeatPassword) return;
                _repeatPassword = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (value == Username) return;
                _username = value;
                OnPropertyChanged();
            }
        }


        public void Cancel() { ((Frame) Window.Current.Content).Navigate(typeof(LandingPage)); }

        public override void Kill()
        {
            // Nothing special used here.
        }

        public void Register()
        {
            RegisterEnable = false;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Phone))
            {
                Problems = "Please complete all the fields above.";
            }
            else if (Password != RepeatPassword)
            {
                Problems = "Passwords do no match, please type them in again.";
            }
            else if (Password.Length < 8)
            {
                Problems = "Password must be at-least 8 characters long.";
            }
            else
            {
                Problems = "Please wait...";
                throw new NotImplementedException("Need to fire register request.");
            }
            RegisterEnable = true;
        }
    }
}
