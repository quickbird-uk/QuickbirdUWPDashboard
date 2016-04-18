namespace Agronomist.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
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

        private DelegateCommand _showBusyCommand;

        public SettingsPartViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = SettingsService.Instance;
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