namespace Agronomist.Services.SettingsServices
{
    using System;
    using Windows.UI.Xaml;
    using Template10.Common;
    using Template10.Services.SettingsService;
    using Template10.Utils;
    using Views;

    /// <summary>
    ///     Wraps access to local and romaing winsows 10 settings.
    /// </summary>
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
                    BootStrapper.Current.ShowShellBackButton = false;
                    BootStrapper.Current.UpdateShellBackButton();
                    BootStrapper.Current.NavigationService.Refresh();
                });
            }
        }

        /// <summary>
        ///     Auth token in roaming settings.
        /// </summary>
        public string AuthToken
        {
            get { return _helper.Read<string>(nameof(AuthToken), null, SettingsStrategies.Roam); }
            set { _helper.Write(nameof(AuthToken), value, SettingsStrategies.Roam); }
        }

        public DateTimeOffset? AuthExpiry
        {
            get { return _helper.Read<DateTimeOffset?>(nameof(AuthExpiry), null, SettingsStrategies.Roam); }
            set { _helper.Write(nameof(AuthExpiry), value, SettingsStrategies.Roam); }
        }

        public DateTimeOffset? LastAuth
        {
            get { return _helper.Read<DateTimeOffset?>(nameof(LastAuth), null, SettingsStrategies.Roam); }
            set { _helper.Write(nameof(LastAuth), value, SettingsStrategies.Roam); }
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

        /// <summary>
        ///     The UserId linked to the authentication token.
        /// </summary>
        public string UserId
        {
            get { return _helper.Read<string>(nameof(UserId), null, SettingsStrategies.Roam); }
            set { _helper.Write(nameof(UserId), value, SettingsStrategies.Roam); }
        }
    }
}