using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Common.Utils;

using Windows.Storage;
using System.Threading.Tasks;

namespace VKClient.Common.Framework
{
    public class ImageLoader
    {
        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnUriSourceChanged)));
        public static readonly DependencyProperty StreamSourceProperty = DependencyProperty.RegisterAttached("StreamSource", typeof(Stream), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnStreamSourceChanged)));
        public static readonly DependencyProperty ImageBrushSourceProperty = DependencyProperty.RegisterAttached("ImageBrushSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnImageBrushSourceChanged)));
        public static readonly DependencyProperty ImageBrushMultiResSourceProperty = DependencyProperty.RegisterAttached("ImageBrushMultiResSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnImageBrushMultiResSourceChanged)));

        public static string GetUriSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(ImageLoader.UriSourceProperty);
        }

        public static void SetUriSource(Image obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageLoader.UriSourceProperty, (object)value);
        }

        public static string GetStreamSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(ImageLoader.StreamSourceProperty);
        }

        public static void SetStreamSource(Image obj, Stream value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageLoader.StreamSourceProperty, (object)value);
        }

        public static string GetImageBrushSource(ImageBrush obj)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            return (string)obj.GetValue(ImageLoader.ImageBrushSourceProperty);
        }

        public static void SetImageBrushSource(ImageBrush obj, string value)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            obj.SetValue(ImageLoader.ImageBrushSourceProperty, (object)value);
        }

        public static string GetImageBrushMultiResSource(ImageBrush obj)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            return (string)obj.GetValue(ImageLoader.ImageBrushMultiResSourceProperty);
        }

        public static void SetImageBrushMultiResSource(ImageBrush obj, string value)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            obj.SetValue(ImageLoader.ImageBrushMultiResSourceProperty, (object)value);
        }

        private static void OnImageBrushSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageLoader.ProcessImageBrush((ImageBrush)d, e.NewValue);
        }


        private static void OnImageBrushMultiResSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageBrush ib = (ImageBrush)d;
            if (e.NewValue == null)
            {
                ImageLoader.ProcessImageBrush(ib, null);
            }
            else
            {
                string @string = e.NewValue.ToString();

                string str = !DesignerProperties.GetIsInDesignMode((DependencyObject)ib) ? MultiResolutionHelper.Instance.AppendResolutionSuffix(@string, true, "") : MultiResolutionHelper.Instance.AppendResolutionSuffix(@string, true, "-WVGA");
                
                ImageLoader.ProcessImageBrush(ib, str);
            }
        }

        private static void ProcessImageBrush(ImageBrush ib, object newSource)
        {
            if (newSource == null)
            {
                ib.ImageSource = null;
            }
            else
            {
                Uri uri = new Uri((string)newSource, UriKind.RelativeOrAbsolute);
                ib.ImageSource = (ImageSource)new BitmapImage()
                {
                    CreateOptions = (BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation),
                    UriSource = uri
                };
            }
        }

        private static void OnStreamSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image image = (Image)d;
            Stream streamSource = e.NewValue as Stream;
            if (streamSource == null)
            {
                image.Source = null;
            }
            else
            {
                BitmapImage bitmapImage = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation
                };
                bitmapImage.SetSource(streamSource);
                image.Source = (ImageSource)bitmapImage;
            }
        }

        private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ImageLoader.HandleUriChangeLowProfile((Image)o, (string)e.NewValue);
        }

        public static void SetSourceForImage(Image image, string uriStr, bool animateOpacity = false)
        {
            ImageLoader.SetUriSource(image, uriStr);
        }

        private static void HandleUriChangeLowProfile(Image image, string uriStr)
        {
            Uri uri = uriStr.ConvertToUri();
            VeryLowProfileImageLoader.SetUriSource(image, uri);
        }
    }
}
