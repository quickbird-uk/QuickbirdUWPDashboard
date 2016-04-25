namespace Agronomist.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
    using NetLib;
    using Services.SettingsServices;
    using Template10.Mvvm;
    using Views;

    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
    }

    public class SettingsPartViewModel : ViewModelBase
    {
        private readonly SettingsService _settings;
        private string _busyText = "Please wait...";
        private string _renewBefore;
        private DelegateCommand _showBusyCommand;
        private bool _authButtonEnabled = true;

        public SettingsPartViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = SettingsService.Instance;
                CalcRenewDate();
            }
        }

        public string AuthExpiry
        {
            get
            {
                var date = _settings.AuthExpiry;
                return date == null ? "--" : date.Value.LocalDateTime.ToString("R");
            }
        }

        public string LastAuth
        {
            get
            {
                var date = _settings.LastAuth;
                return date == null ? "--" : date.Value.LocalDateTime.ToString("R");
            }
        }

        public bool UseShellBackButton
        {
            get { return _settings.UseShellBackButton; }
            set
            {
                _settings.UseShellBackButton = value;
                RaisePropertyChanged();
            }
        }

        public bool UseLightThemeButton
        {
            get { return _settings.AppTheme.Equals(ApplicationTheme.Light); }
            set
            {
                _settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark;
                RaisePropertyChanged();
            }
        }

        public string BusyText
        {
            get { return _busyText; }
            set
            {
                Set(ref _busyText, value);
                _showBusyCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand ShowBusyCommand
            => _showBusyCommand ?? (_showBusyCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, _busyText);
                await Task.Delay(5000);
                Busy.SetBusy(false);
            }, () => !string.IsNullOrEmpty(BusyText)));

        public string RenewBefore => _renewBefore;

        public bool AuthButtonEnabled
        {
            get { return _authButtonEnabled; }
            set
            {
                _authButtonEnabled = value;
                RaisePropertyChanged();
            }

        }

        public async void Authenticate()
        {
            if (!AuthButtonEnabled) return;
            AuthButtonEnabled = false;
            const string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";
            var cred = await Creds.FromBroker(entryUrl, resultUrl);
            SaveNewToken(cred);
            AuthButtonEnabled = true;
        }

        /// <summary>
        /// Saves a new token, sets and notifies all relevant properties for the UI.
        /// </summary>
        /// <param name="cred"></param>
        public void SaveNewToken(Creds cred)
        {
            _settings.AuthToken = cred.Token;
            _settings.AuthExpiry = cred.Expiry;
            _settings.LastAuth = cred.Start;
            CalcRenewDate();
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(AuthExpiry));
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(LastAuth));
        }

        private void CalcRenewDate()
        {
            var expiriy = _settings.AuthExpiry;
            if (expiriy == null)
                _renewBefore = "--";
            else
            {
                var now = DateTimeOffset.Now;
                var timeleft = now - expiriy.Value;
                _renewBefore = timeleft < TimeSpan.Zero
                    ? "expired"
                    : $"{timeleft.Days} days, {timeleft.Hours} hours and {timeleft.Minutes} left.";
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(RenewBefore));
        }
    }

    public class AboutPartViewModel : ViewModelBase
    {
        public Uri Logo => Package.Current.Logo;

        public string DisplayName => Package.Current.DisplayName;

        public string Publisher => Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public Uri RateMe => new Uri("http://aka.ms/template10");
    }
}