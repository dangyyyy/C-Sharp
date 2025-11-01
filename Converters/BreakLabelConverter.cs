using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Kursovoy.Converters
{
    public class BreakLabelConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
                return $"После пары {index + 1}:";

            return "-";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}