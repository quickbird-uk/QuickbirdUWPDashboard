namespace Agronomist
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Windows.Storage;
    using JetBrains.Annotations;

    /// <summary>
    /// A class to create settings properties in. 
    /// Local and roaming are hard coded into individual properties.
    /// </summary>
    internal class Settings : INotifyPropertyChanged
    {
        public enum SettingsType
        {
            Local,
            Roaming
        }

        private readonly ApplicationDataContainer _localSettings;
        private readonly ApplicationDataContainer _roamingSettings;

        public Settings()
        {
            _roamingSettings = ApplicationData.Current.RoamingSettings;
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public string CredToken
        {
            get { return Get<string>(_roamingSettings, null); }
            set
            {
                if (value == CredToken) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        public string CredUserId
        {
            get { return Get<string>(_roamingSettings, null); }
            set
            {
                if (value == CredUserId) return;
                Set(_roamingSettings, value);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private void Set<T>([NotNull] ApplicationDataContainer settingsContainer, T value,
            [CallerMemberName] string settingsName = null)
        {
            settingsContainer.Values[settingsName] = value;
        }

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

            container.Values.Remove(settingsName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}