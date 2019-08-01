using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SignalGoTest.Desktop.Converters
{
    public class IsNullOrEmptyConverter : IValueConverter
    {
        public bool IsInverse { get; set; } = true;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = true;
            if (value == null)
                result = true;
            else if (string.IsNullOrEmpty(value.ToString()))
                result = true;

            if (IsInverse)
                return !result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}