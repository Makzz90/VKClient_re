using System;
using System.Globalization;
using System.Windows.Data;
using VKClient.Common.Localization;

namespace VKClient.Common.Framework.Convertors
{
  public class BoolToSwitchConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return CommonResources.Settings_DisabledNeutral;
      if (!"On".Equals(value))
        return CommonResources.Settings_DisabledNeutral;
      return CommonResources.Settings_EnabledNeutral;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value.Equals(CommonResources.Settings_EnabledNeutral);
    }
  }
}
