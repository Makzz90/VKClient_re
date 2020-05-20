using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKClient.Common.Framework.Convertors
{
  public class CountToVisibilityConverter : IValueConverter
  {
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is int))
        return (object) Visibility.Visible;
      int result = 0;
      if (parameter is string)
        int.TryParse(parameter.ToString(), out result);
      int num = (int) value;
      if (this.IsInverted)
        return (object) (Visibility) (num > result ? 1 : 0);
      return (object) (Visibility) (num > result ? 0 : 1);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
