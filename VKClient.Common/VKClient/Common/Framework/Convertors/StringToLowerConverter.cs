using System;
using System.Globalization;
using System.Windows.Data;

namespace VKClient.Common.Framework.Convertors
{
  public class StringToLowerConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) "";
      return (object) value.ToString().ToLowerInvariant();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) "";
    }
  }
}
