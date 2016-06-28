namespace Agronomist.Views
{
    using System;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using ViewModels;

    /// <summary>
    ///     All sorts of settings are viewed and edited here.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel = new SettingsViewModel();

        public SettingsView()
        {
            InitializeComponent();
        }

        public class TrueVisible : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (value is bool && (bool)value)
                {
                    return "Visible";
                }
                else
                {
                    return "Collapsed";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }
        public class FalseVisible : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (value is bool && (bool)value)
                {
                    return "Collapsed";
                }
                else
                {
                    return "Visible";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }
    }
}