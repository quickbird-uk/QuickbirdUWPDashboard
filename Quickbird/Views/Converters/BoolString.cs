namespace Quickbird.Views.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    /// <summary>
    ///     First parameter corresponds to true, second to false. Delimit parameters with |.
    /// </summary>
    public class BoolString : IValueConverter
    {
        /// <summary>
        ///     Converts true and false to corresponding strings supplied as parameters.
        /// </summary>
        /// <param name="value">a boolean</param>
        /// <param name="targetType">string</param>
        /// <param name="parameter">Use a bar to separate params: "trueString|falseString"</param>
        /// <param name="language">ignored</param>
        /// <returns>string</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var items = parameter.ToString().Split('|');
            if (items.Length != 2)
            {
                throw new ArgumentException("Invalid parameter for (IValueConverter)BoolString.Convert");
            }

            if (!(value is bool))
            {
                throw new ArgumentException("Invalid value type for (IValueConverter)BoolString.Convert");
            }

            if ((bool) value)
            {
                return items.First();
            }
            return items.Last();
        }

        /// <summary>
        ///     string to bool, expected strings supplied in params.
        /// </summary>
        /// <param name="value">A string or any object where ToString() works.</param>
        /// <param name="targetType">string</param>
        /// <param name="parameter">Use a bar to separate params: "trueString|falseString"</param>
        /// <param name="language">ignored</param>
        /// <returns>bool or false no parameters match the value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var items = parameter.ToString().Split('|');
            if (items.Length != 2)
            {
                throw new ArgumentException("Invalid parameter for  (IValueConverter)BoolString.Convert");
            }
            var s = value.ToString();
            if (items.First() == s)
                return true;
            return false;
        }
    }
}