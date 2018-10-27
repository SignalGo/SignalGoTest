using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup;
using Avalonia.Media.Imaging;
using SignalGo.Shared.Helpers;
using System;
using System.Globalization;
using System.IO;

namespace SignalGoTest.Desktop.Converters
{
    public class TypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images");
            if (value is SerializeObjectType objectType && objectType == SerializeObjectType.Enum)
            {
                return new Bitmap (Path.Combine(path,"enumIcon.png"));
            }
            return new Avalonia.Media.Imaging.Bitmap(Path.Combine(path, "classicons.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
