namespace Agronomist.Views
{
    using System;
    using Windows.UI.Xaml.Data;

    public class FalseVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool) value)
            {
                return Windows.UI.Xaml.Visibility.Collapsed;
            }
            return Windows.UI.Xaml.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}