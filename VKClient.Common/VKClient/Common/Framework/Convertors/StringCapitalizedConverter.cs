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
        return (object) "";
      string @string = value.ToString();
      return (object) (@string[0].ToString().ToUpperInvariant() + @string.Substring(1));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
