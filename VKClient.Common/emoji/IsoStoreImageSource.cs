using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Audio.Base.Utils;

namespace VKClient.Common.Emoji
{
    public class IsoStoreImageSource : DependencyObject
    {
        public static readonly DependencyProperty IsoStoreFileNameProperty = DependencyProperty.RegisterAttached("IsoStoreFileName", typeof(string), typeof(IsoStoreImageSource), new PropertyMetadata("", new PropertyChangedCallback(IsoStoreImageSource.Changed)));
        private static int MAX_CACHE_SIZE = 7000000;
        private static int CurrentCacheSize = 0;
        private static Dictionary<string, byte[]> _cacheDict;

        public IsoStoreImageSource()
        {
            //base.\u002Ector();
        }

        public static void SetIsoStoreFileName(UIElement element, string value)
        {
            ((DependencyObject)element).SetValue(IsoStoreImageSource.IsoStoreFileNameProperty, value);
        }

        public static string GetIsoStoreFileName(UIElement element)
        {
            return (string)((DependencyObject)element).GetValue(IsoStoreImageSource.IsoStoreFileNameProperty);
        }

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (IsoStoreImageSource._cacheDict == null)
                IsoStoreImageSource._cacheDict = new Dictionary<string, byte[]>();
            Image img = d as Image;
            if (img == null)
                return;
            // ISSUE: explicit reference operation
            string path = e.NewValue as string;
            if (string.IsNullOrEmpty(path))
                img.Source = (null);
            else if (IsoStoreImageSource._cacheDict.ContainsKey(path))
            {
                MemoryStream memoryStream = new MemoryStream(IsoStoreImageSource._cacheDict[path]);
                IsoStoreImageSource.SetSource(img, (Stream)memoryStream);
                memoryStream.Close();
            }
            else
            {
                SynchronizationContext uiThread = SynchronizationContext.Current;
                Task.Factory.StartNew((Action)(() =>
                {
                    try
                    {
                        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (!storeForApplication.FileExists(path))
                                return;
                            IsolatedStorageFileStream stream = storeForApplication.OpenFile(path, FileMode.Open, FileAccess.Read);
                            uiThread.Post((SendOrPostCallback)(_ =>
                            {
                                if (IsoStoreImageSource.CurrentCacheSize < IsoStoreImageSource.MAX_CACHE_SIZE)
                                {
                                    IsoStoreImageSource._cacheDict[path] = StreamUtils.ReadFullyToByteArray((Stream)stream);
                                    IsoStoreImageSource.CurrentCacheSize += IsoStoreImageSource._cacheDict[path].Length;
                                }
                                stream.Position = 0L;
                                if (IsoStoreImageSource.GetIsoStoreFileName((UIElement)img) == path)
                                    IsoStoreImageSource.SetSource(img, (Stream)stream);
                                stream.Close();
                            }), null);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }));
            }
        }

        private static void SetSource(Image img, Stream stream)
        {
            BitmapImage bitmapImage = new BitmapImage();
            if (((FrameworkElement)img).Height > 0.0 && ((FrameworkElement)img).Height != double.NaN)
            {
                bitmapImage.DecodePixelType = ((DecodePixelType)1);
                bitmapImage.DecodePixelHeight = (Convert.ToInt32(((FrameworkElement)img).Height));
            }
            ((BitmapSource)bitmapImage).SetSource(stream);
            img.Source = ((ImageSource)bitmapImage);
        }
    }
}
