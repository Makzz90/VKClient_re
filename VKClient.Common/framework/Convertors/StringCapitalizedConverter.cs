using System;
using System.Globalization;
using System.Windows.Data;

namespace VKClient.Common.Framework.Convertors
{
  public class StringCapitalizedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || value.ToString() == string.Empty)
        return "";
      string str = value.ToString();
      return (str[0].ToString().ToUpperInvariant() + str.Substring(1));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
