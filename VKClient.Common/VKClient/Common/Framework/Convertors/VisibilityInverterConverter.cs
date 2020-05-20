using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKClient.Common.Framework.Convertors
{
  public class VisibilityInverterConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((Visibility) value == Visibility.Collapsed)
        return (object) Visibility.Visible;
      return (object) Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
