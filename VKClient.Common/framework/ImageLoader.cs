using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Common.Utils;

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
      return (string) obj.GetValue(ImageLoader.UriSourceProperty);
    }

    public static void SetUriSource(Image obj, string value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      obj.SetValue(ImageLoader.UriSourceProperty, value);
    }

    public static string GetStreamSource(Image obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return (string) obj.GetValue(ImageLoader.StreamSourceProperty);
    }

    public static void SetStreamSource(Image obj, Stream value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      obj.SetValue(ImageLoader.StreamSourceProperty, value);
    }

    public static string GetImageBrushSource(ImageBrush obj)
    {
      if (obj == null)
        throw new ArgumentException("obj");
      return (string) obj.GetValue(ImageLoader.ImageBrushSourceProperty);
    }

    public static void SetImageBrushSource(ImageBrush obj, string value)
    {
      if (obj == null)
        throw new ArgumentException("obj");
      obj.SetValue(ImageLoader.ImageBrushSourceProperty, value);
    }

    public static string GetImageBrushMultiResSource(ImageBrush obj)
    {
      if (obj == null)
        throw new ArgumentException("obj");
      return (string) obj.GetValue(ImageLoader.ImageBrushMultiResSourceProperty);
    }

    public static void SetImageBrushMultiResSource(ImageBrush obj, string value)
    {
      if (obj == null)
        throw new ArgumentException("obj");
      obj.SetValue(ImageLoader.ImageBrushMultiResSourceProperty, value);
    }

    private static void OnImageBrushSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ImageLoader.ProcessImageBrush((ImageBrush)d, e.NewValue);
    }


    private static void OnImageBrushMultiResSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ImageBrush imageBrush = (ImageBrush)d;
        if (e.NewValue == null)
        {
            ImageLoader.ProcessImageBrush(imageBrush, null);
            return;
        }
        string text = e.NewValue.ToString();
        if (DesignerProperties.GetIsInDesignMode(imageBrush))
        {
            text = MultiResolutionHelper.Instance.AppendResolutionSuffix(text, true, "-WVGA");
        }
        else
        {
            text = MultiResolutionHelper.Instance.AppendResolutionSuffix(text, true, "");
        }
        ImageLoader.ProcessImageBrush(imageBrush, text);
    }


    private static void ProcessImageBrush(ImageBrush ib, object newSource)
    {
      if (newSource == null)
      {
        ib.ImageSource=( null);
      }
      else
      {
        Uri uri = new Uri((string) newSource, UriKind.RelativeOrAbsolute);
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.CreateOptions = ((BitmapCreateOptions) 18);
        bitmapImage.UriSource = uri;
        ib.ImageSource=((ImageSource) bitmapImage);
      }
    }

    private static void OnStreamSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Image image = (Image)d;
        Stream stream = e.NewValue as Stream;
        if (stream == null)
        {
            image.Source=(null);
            return;
        }
        BitmapImage expr_24 = new BitmapImage();
        expr_24.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
        BitmapImage bitmapImage = expr_24;
        bitmapImage.SetSource(stream);
        image.Source=(bitmapImage);
    }


    private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        Image arg_14_0 = (Image)o;
        string uriStr = (string)e.NewValue;
        ImageLoader.HandleUriChangeLowProfile(arg_14_0, uriStr);
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
