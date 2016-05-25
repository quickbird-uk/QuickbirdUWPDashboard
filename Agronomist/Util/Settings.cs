namespace Agronomist.Util
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Windows.Storage;
    using JetBrains.Annotations;

    /// <summary>
    ///     A class to create settings properties in.
    ///     Local and roaming are hard coded into individual properties.
    /// </summary>
    internal class Settings : INotifyPropertyChanged
    {
        /// <summary>
        ///     Enum for choosing where a property that you want to delete exists.
        /// </summary>
        public enum SettingsType
        {
            Local,
            Roaming
        }

        private readonly ApplicationDataContainer _localSettings;
        private readonly ApplicationDataContainer _roamingSettings;

        /// <summary>
        ///     Creata a new settings obbject that gives acces to local and roaming settings.
        /// </summary>
        public Settings()
        {
            _roamingSettings = ApplicationData.Current.RoamingSettings;
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool CredsSet
        {
            get { return Get(_roamingSettings, false); }
            private set
            {
                if (value == CredsSet) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The credentials token setting.
        /// </summary>
        public string CredToken
        {
            get { return Get<string>(_roamingSettings, null); }
            private set
            {
                if (value == CredToken) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The credentials userID setting.
        /// </summary>
        public string CredUserId
        {
            get { return Get<string>(_roamingSettings, null); }
            private set
            {
                if (value == CredUserId) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        public Guid CredStableSid
        {
            get { return Get(_roamingSettings, default(Guid)); }
            private set
            {
                if (value == CredStableSid) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetNewCreds(string token, string userId, Guid stableSid)
        {
            CredToken = token;
            CredUserId = userId;
            CredStableSid = stableSid;
            CredsSet = true;
        }

        public void UnsetCreds()
        {
            CredsSet = false;
            Delete(nameof(CredToken), SettingsType.Roaming);
            Delete(nameof(CredUserId), SettingsType.Roaming);
            Delete(nameof(CredStableSid), SettingsType.Roaming);
        }

        public DateTimeOffset LastDatabaseUpdate
        {
            get { return Get(_localSettings, default(DateTimeOffset)); }
            set
            {
                if (value == LastDatabaseUpdate) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }

        public DateTimeOffset LastSensorDataPost
        {
            get { return Get(_localSettings, default(DateTimeOffset)); }
            set
            {
                if (value == LastDatabaseUpdate) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }
        public DateTimeOffset LastDatabasePost
        {
            get { return Get(_localSettings, default(DateTimeOffset)); }
            set
            {
                if (value == LastDatabaseUpdate) return;
                Set(_localSettings, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the setting with the name of the property it is called from.
        /// </summary>
        /// <typeparam name="T">The type of the setting, must be the same as the property.</typeparam>
        /// <param name="settingsContainer">Local or roaming.</param>
        /// <param name="defaultValue">The value to return if the setting is not set.</param>
        /// <param name="settingsName">The name of the setting, should be autoset when called from a property.</param>
        /// <returns></returns>
        private T Get<T>([NotNull] ApplicationDataContainer settingsContainer, [CanBeNull] T defaultValue,
            [CallerMemberName] string settingsName = null)
        {
            if (settingsName == null)
            {
                throw new ArgumentException("Tried to get with a null settings value.");
            }
            if (settingsContainer.Values.ContainsKey(settingsName))
            {
                return (T) settingsContainer.Values[settingsName];
            }
            return defaultValue;
        }

        /// <summary>
        ///     Sets the setting with the name of the property it is called from.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="settingsContainer">Local or roaming.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="settingsName">The name of the setting, should be autoset when called from a property.</param>
        private void Set<T>([NotNull] ApplicationDataContainer settingsContainer, T value,
            [CallerMemberName] string settingsName = null)
        {
            settingsContainer.Values[settingsName] = value;
        }

        /// <summary>
        ///     Method to unset a method value if it exists, otherwise it does nothing.
        /// </summary>
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
                    container = _roamingSettings;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settingsType), settingsType, null);
            }

            if (container.Values.ContainsKey(settingsName))
                container.Values.Remove(settingsName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}