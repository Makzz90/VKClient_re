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
            return (string)obj.GetValue(MultiResImageLoader.UriSourceProperty);
        }

        public static void SetUriSource(Image obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(MultiResImageLoader.UriSourceProperty, (object)value);
        }

        private static void OnUriSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image image = (Image)d;
            string imageUri = (string)e.NewValue;
            if (string.IsNullOrWhiteSpace(imageUri))
            {
                image.Source = null;
            }
            else
            {
                BitmapImage bitmapImage = new BitmapImage((!DesignerProperties.GetIsInDesignMode((DependencyObject)image) ? MultiResolutionHelper.Instance.AppendResolutionSuffix(imageUri, true, "") : MultiResolutionHelper.Instance.AppendResolutionSuffix(imageUri, true, "-WVGA")).ConvertToUri());
                image.Source = (ImageSource)bitmapImage;
            }
        }
    }
}
