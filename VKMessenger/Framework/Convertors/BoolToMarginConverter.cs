using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKMessenger.Framework.Convertors
{
  public class BoolToMarginConverter : IValueConverter
  {
    private static readonly Thickness _ownMessageMargin = new Thickness(96.0, 10.0, 0.0, 9.0);
    private static readonly Thickness _replyMessageMargin = new Thickness(0.0, 10.0, 96.0, 9.0);
    private static readonly Thickness _replyChatMessageMargin = new Thickness(0.0, 10.0, 36.0, 9.0);
    private static readonly Thickness _onlineMargin = new Thickness(12.0, 0.0, 12.0, 0.0);
    private static readonly Thickness _offlineMargin = new Thickness(12.0, 0.0, 0.0, 0.0);
    private static readonly Thickness _forwardMessageMargin = new Thickness(11.0, 0.0, 11.0, 0.0);
    private static readonly Thickness _forwardMessageMarginNoText = new Thickness(11.0, -24.0, 11.0, 0.0);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int)
      {
        switch ((int) value)
        {
          case 1:
            return BoolToMarginConverter._replyMessageMargin;
          case 0:
            return BoolToMarginConverter._ownMessageMargin;
          default:
            return BoolToMarginConverter._replyChatMessageMargin;
        }
      }
      else
      {
        if (value is bool && parameter == null)
        {
          if ((bool) value)
            return BoolToMarginConverter._onlineMargin;
          return BoolToMarginConverter._offlineMargin;
        }
        if (!(value is bool) || parameter == null || string.Compare(parameter.ToString(), "ForwardedMessage") != 0)
          return  null;
        if ((bool) value)
          return BoolToMarginConverter._forwardMessageMargin;
        return BoolToMarginConverter._forwardMessageMarginNoText;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
