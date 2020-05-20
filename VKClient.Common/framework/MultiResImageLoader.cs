using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
  public class MultiResImageLoader
  {
      public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(string), typeof(MultiResImageLoader), new PropertyMetadata(new PropertyChangedCallback(MultiResImageLoader.OnUriSourceChanged)));

    public static string GetUriSource(Image obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return (string) ((DependencyObject) obj).GetValue(MultiResImageLoader.UriSourceProperty);
    }

    public static void SetUriSource(Image obj, string value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      ((DependencyObject) obj).SetValue(MultiResImageLoader.UriSourceProperty, value);
    }

    private static void OnUriSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Image image = (Image) d;
      // ISSUE: explicit reference operation
      string newValue = (string) e.NewValue;
      if (string.IsNullOrWhiteSpace(newValue))
      {
        image.Source = ( null);
      }
      else
      {
        BitmapImage bitmapImage = new BitmapImage((!DesignerProperties.GetIsInDesignMode((DependencyObject) image) ? MultiResolutionHelper.Instance.AppendResolutionSuffix(newValue, true, "") : MultiResolutionHelper.Instance.AppendResolutionSuffix(newValue, true, "-WVGA")).ConvertToUri());
        image.Source = ((ImageSource) bitmapImage);
      }
    }
  }
}
