using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace HeartPlayer.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is bool boolValue && parameter is string stringParameter)
        {
            var options = stringParameter.Split(',');
            return boolValue ? options[0] : options[1];
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
