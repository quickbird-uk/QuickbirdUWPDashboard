namespace Quickbird.Util
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Windows.Foundation.Collections;
    using Windows.Storage;
    using JetBrains.Annotations;

    /// <summary>A singleton to create settings properties in. Local and roaming are hard coded into
    /// individual properties.</summary>
    internal class Settings : INotifyPropertyChanged
    {
        public delegate void ChangeHandler();

        /// <summary>Enum for choosing where a property that you want to delete exists.</summary>
        public enum SettingsType
        {
            Local,
            Roaming
        }

        private readonly ApplicationDataContainer _localSettings;
        private readonly ApplicationDataContainer _roamingSettings;
        private ApplicationDataCompositeValue _combinedCreds;
        private string _token;
        private Guid _personGuid;
        private string _username;

        /// <summary>Creata a new settings obbject that gives acces to local and roaming settings.</summary>
        private Settings()
        {
            _roamingSettings = ApplicationData.Current.RoamingSettings;
            _localSettings = ApplicationData.Current.LocalSettings;

            if (!_localSettings.Values.ContainsKey(nameof(CombinedCredentials)))
            {
                _combinedCreds = new ApplicationDataCompositeValue();
                _localSettings.Values[nameof(CombinedCredentials)] = _combinedCreds;
            }
            else
            {
                _combinedCreds = CombinedCredentials;
            }
            UpdateCredPropsFromCombined();
            _localSettings.Values.MapChanged += ValuesOnMapChanged;
        }

        /// <summary>Singleton instance accessor.</summary>
        public static Settings Instance { get; } = new Settings();

        public bool IsLoggedIn => Token != null;

        /// <summary>The last time the db (excluding histories) was successfully updated from the internet.</summary>
        public DateTimeOffset LastSuccessfulGeneralDbGet
        {
            get { return Get(_localSettings, default(DateTimeOffset)); }

            set
            {
                if (value == LastSuccessfulGeneralDbGet) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>The last time the db (excluding histories) was successfully pushed to the internet.</summary>
        public DateTimeOffset LastSuccessfulGeneralDbPost
        {
            get { return Get(_localSettings, default(DateTimeOffset)); }

            set
            {
                if (value == LastSuccessfulGeneralDbPost) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>Local setting that allows the app to run local network for device management. Defaults to
        /// false.</summary>
        public bool LocalDeviceManagementEnabled
        {
            get { return Get(_localSettings, false); }
            set
            {
                if (value == LocalDeviceManagementEnabled) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>The credentials token.</summary>
        /// <remarks>The setter does not update the setting, that must be done via SetNewCreds.</remarks>
        public string Token
        {
            get { return _token; }
            private set
            {
                if (value == Token) return;
                _token = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The unique identifier for the user from the database.</summary>
        /// <remarks>The setter does not update the setting, that must be done via SetNewCreds.</remarks>
        public Guid PersonGuid
        {
            get { return _personGuid; }
            private set
            {
                if (value == PersonGuid) return;
                _personGuid = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The user's login username.</summary>
        /// <remarks>The setter does not update the setting, that must be done via SetNewCreds.</remarks>
        public string Username
        {
            get { return _username; }
            private set
            {
                if (value == Username) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        /// <summary>All the credentials packed into a single object so that their values update and sync at
        /// the same time.</summary>
        private ApplicationDataCompositeValue CombinedCredentials
        {
            get { return Get(_localSettings, default(ApplicationDataCompositeValue)); }
            set
            {
                Set(_localSettings, value);
                UpdateCredPropsFromCombined();
                OnPropertyChanged();
            }
        }

        private ApplicationDataCompositeValue RoamingCombinedCredentials
        {
            get { return Get(_roamingSettings, default(ApplicationDataCompositeValue)); }
            set
            {
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event ChangeHandler CredsChanged;

        /// <summary>Method to unset a method value if it exists, otherwise it does nothing.</summary>
        /// <param name="settingsName">Name of the setting to unset.</param>
        /// <param name="settingsType">Roaming or local.</param>
        public void Delete([NotNull] string settingsName, SettingsType settingsType)
        {
            ApplicationDataContainer container;
            switch (settingsType)
            {
                case SettingsType.Local:
                    container = _localSettings;
                    break;
                case SettingsType.Roaming:
                    container = _localSettings;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settingsType), settingsType, null);
            }

            if (container.Values.ContainsKey(settingsName))
                container.Values.Remove(settingsName);
        }


        public void ResetDatabaseAndPostSettings()
        {
            LastSuccessfulGeneralDbGet = default(DateTimeOffset);
            LastSuccessfulGeneralDbPost = default(DateTimeOffset);
        }

        /// <summary>
        /// Updates all settings related to credentials simultaneously.
        /// </summary>
        /// <param name="username">The user-friendly login name.</param>
        /// <param name="token">The auth token.</param>
        /// <param name="personGuid">The user's linked person Guid.</param>
        public void SetNewCreds(string username, string token, Guid personGuid)
        {
            _combinedCreds[nameof(Token)] = token;
            _combinedCreds[nameof(Username)] = username;
            _combinedCreds[nameof(PersonGuid)] = personGuid;

            CombinedCredentials = _combinedCreds;
            RoamingCombinedCredentials = _combinedCreds;

            UpdateCredPropsFromCombined();
        }

        public void UnsetCreds()
        {
            SetNewCreds(null, null, Guid.Empty);
            Delete(nameof(RoamingCombinedCredentials), SettingsType.Roaming);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Gets the setting with the name of the property it is called from.</summary>
        /// <typeparam name="T">The type of the setting, must be the same as the property.</typeparam>
        /// <param name="settingsContainer">Local or roaming.</param>
        /// <param name="defaultValue">The value to return if the setting is not set.</param>
        /// <param name="settingsName">The name of the setting, should be autoset when called from a property.</param>
        /// <returns></returns>
        private T Get<T>([NotNull] ApplicationDataContainer settingsContainer, [CanBeNull] T defaultValue,
            [CallerMemberName] string settingsName = null)
        {
            if (settingsName == null)
                throw new ArgumentException("Tried to get with a null settings value.");
            if (settingsContainer.Values.ContainsKey(settingsName))
                return (T) settingsContainer.Values[settingsName];
            return defaultValue;
        }

        /// <summary>Sets the setting with the name of the property it is called from.</summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="settingsContainer">Local or roaming.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="settingsName">The name of the setting, should be autoset when called from a property.</param>
        private void Set<T>([NotNull] ApplicationDataContainer settingsContainer, T value,
            [CallerMemberName] string settingsName = null)
        {
            settingsContainer.Values[settingsName] = value;
        }

        private void UpdateCredPropsFromCombined()
        {
            var cc = CombinedCredentials;

            if (cc.ContainsKey(nameof(Token))) _token = (string) cc[nameof(Token)];
            if (cc.ContainsKey(nameof(Username))) _username = (string) cc[nameof(Username)];
            if (cc.ContainsKey(nameof(PersonGuid))) _personGuid = (Guid) cc[nameof(PersonGuid)];
        }

        private void ValuesOnMapChanged(IObservableMap<string, object> sender, IMapChangedEventArgs<string> @event)
        {
            if (sender.ContainsKey(nameof(CombinedCredentials)))
            {
                UpdateCredPropsFromCombined();
                _combinedCreds = CombinedCredentials;
                CredsChanged?.Invoke();
            }
        }
    }
}
