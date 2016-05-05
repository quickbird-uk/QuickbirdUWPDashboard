namespace Agronomist.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
    using Models;
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
        private bool _authButtonEnabled = true;
        private string _busyText = "Please wait...";
        private string _lastUpdate = "-";
        private DateTimeOffset _lastUpdateTime;
        private string _nextUpdate = "-";
        private DelegateCommand _showBusyCommand;
        private bool _updateButtonEnabled = true;
        private string _updateStatus = "-";

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

        public string RenewBefore { get; private set; }

        public bool AuthButtonEnabled
        {
            get { return _authButtonEnabled; }
            set
            {
                _authButtonEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool UpdateButtonEnabled
        {
            get { return _updateButtonEnabled; }
            set
            {
                _updateButtonEnabled = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     The last succesful update
        /// </summary>
        public string LastUpdate
        {
            get { return _lastUpdate; }

            set
            {
                _lastUpdate = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     If the last update attempt was successful or if there is an update underway.
        /// </summary>
        public string UpdateStatus
        {
            get { return _updateStatus; }

            set
            {
                _updateStatus = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     When the next update is scheduled for.
        /// </summary>
        public string NextUpdate
        {
            get { return _nextUpdate; }

            set
            {
                _nextUpdate = value;
                RaisePropertyChanged();
            }
        }

        public async void Update()
        {
            UpdateStatus = "Synchronisation in progress...";
            using (var db = new MainDbContext())
            {
                var creds = Creds.FromUserIdAndToken(_settings.UserId, _settings.AuthToken);
                if (null == creds)
                {
                    Debug.WriteLine($"Update aborted, no valid creds available.");
                    return;
                }

                var result = await db.PullAndPopulate(_lastUpdateTime, creds);
                var now = DateTimeOffset.Now;
                if (null == result)
                {
                    UpdateStatus = $"Successfuly updated.";
                    LastUpdate = now.LocalDateTime.ToString("R");
                    _lastUpdateTime = now;
                }
                else
                {
                    UpdateStatus = $"{result} ({now.LocalDateTime.ToString("R")}).";
                }
            }
        }

        public async void Authenticate()
        {
            if (!AuthButtonEnabled) return;
            AuthButtonEnabled = false;
            const string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";
            try
            {
                var cred = await Creds.FromBroker(entryUrl, resultUrl);
                SaveNewToken(cred);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Getting new authenication creds failed.");
                Debug.WriteLine(ex);
            }
            AuthButtonEnabled = true;
        }

        /// <summary>
        ///     Saves a new token, sets and notifies all relevant properties for the UI.
        /// </summary>
        /// <param name="cred"></param>
        public void SaveNewToken(Creds cred)
        {
            _settings.AuthToken = cred.Token;
            _settings.AuthExpiry = cred.Expiry;
            _settings.LastAuth = cred.Start;
            _settings.UserId = cred.Userid;
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
                RenewBefore = "--";
            else
            {
                var now = DateTimeOffset.Now;
                var timeleft = expiriy.Value - now;
                RenewBefore = timeleft < TimeSpan.Zero
                    ? "expired"
                    : $"{timeleft.Days} days, {timeleft.Hours} hours and {timeleft.Minutes} minutes left.";
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