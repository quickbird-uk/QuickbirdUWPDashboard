namespace Quickbird.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _password;

        private string _phone;

        private bool _registerEnable;

        private string _repeatPassword;

        private string _username;

        private string _email;

        private string _infoText;

        private string _problems;

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

        public void Register()
        {

        }


        public void Cancel()
        {

        }

        public override void Kill()
        {
            // Nothing special used here.
        }
    }
}
