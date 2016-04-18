namespace Agronomist.Services.SettingsServices
{
    using System;
    using Windows.UI.Xaml;
    using Template10.Common;
    using Template10.Services.SettingsService;
    using Template10.Utils;
    using Views;

    public class SettingsService
    {
        private readonly ISettingsHelper _helper;

        static SettingsService()
        {
            // implement singleton pattern
            Instance = Instance ?? new SettingsService();
        }

        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        public static SettingsService Instance { get; }

        public bool UseShellBackButton
        {
            get { return _helper.Read(nameof(UseShellBackButton), true); }
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.Dispatcher.Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                    BootStrapper.Current.NavigationService.Refresh();
                });
            }
        }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = _helper.Read(nameof(AppTheme), theme.ToString());
                return Enum.TryParse(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                ((FrameworkElement) Window.Current.Content).RequestedTheme = value.ToElementTheme();
                Shell.HamburgerMenu.RefreshStyles(value);
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }
    }
}