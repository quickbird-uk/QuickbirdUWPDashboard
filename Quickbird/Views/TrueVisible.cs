namespace Quickbird.Views
{
    using System;
    using Windows.UI.Xaml.Data;

    public class TrueVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool) value)
            {
                return Windows.UI.Xaml.Visibility.Visible;
            }
            return Windows.UI.Xaml.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}