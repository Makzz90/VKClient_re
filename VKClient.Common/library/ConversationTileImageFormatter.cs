using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public static class ConversationTileImageFormatter
  {
    public static int DIMENSION = 336;

    public static double LogicalDim
    {
      get
      {
        return (double) ConversationTileImageFormatter.DIMENSION;
      }
    }

    public static void CreateTileImage(List<string> localUris, long userOrChatId, bool isChat, Action<string> callback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        try
        {
          WriteableBitmap wb = new WriteableBitmap(ConversationTileImageFormatter.DIMENSION, ConversationTileImageFormatter.DIMENSION);
          localUris = (List<string>)Enumerable.ToList<string>(Enumerable.Where<string>(localUris, (Func<string, bool>)(u => !string.IsNullOrWhiteSpace(u))));
          List<Rect> map1 = ConversationTileImageFormatter.CreateMap(localUris.Count, isChat);
          List<string> localUris1 = localUris;
          List<Rect> map2 = map1;
          Action<string> callback1 = callback;
          int ind = 0;
          long userOrChatId1 = userOrChatId;
          int num = isChat ? 1 : 0;
          ConversationTileImageFormatter.ProcessImages(wb, localUris1, map2, callback1, ind, userOrChatId1, num != 0);
        }
        catch (Exception ex)
        {
          callback("");
          Logger.Instance.Error(string.Concat("CreateTileImage failed. ", ex));
        }
      }));
    }

    private static void ProcessImages(WriteableBitmap wb, List<string> localUris, List<Rect> map, Action<string> callback, int ind, long userOrChatId, bool isChat)
    {
      if (ind >= localUris.Count || ind >= map.Count)
      {
        Rectangle rectangle = new Rectangle();
        ((FrameworkElement) rectangle).Width=((double) ConversationTileImageFormatter.DIMENSION);
        ((FrameworkElement) rectangle).Height=((double) ConversationTileImageFormatter.DIMENSION);
        ((Shape) rectangle).Fill = ((Brush) new SolidColorBrush(Colors.Black));
        ((UIElement) rectangle).Opacity = 0.2;
        wb.Render((UIElement) rectangle,  null);
        wb.Invalidate();
        IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication();
        try
        {
          string str = string.Concat(new object[4]{ "/Shared/ShellContent/conversationTileImg", userOrChatId, isChat.ToString(), ".jpg" });
          if (!storeForApplication.DirectoryExists("/Shared/ShellContent"))
            storeForApplication.CreateDirectory("/Shared/ShellContent");
          IsolatedStorageFileStream storageFileStream = new IsolatedStorageFileStream(str, (FileMode) 2, (FileAccess) 2, storeForApplication);
          try
          {
            int dimension1 = ConversationTileImageFormatter.DIMENSION;
            int dimension2 = ConversationTileImageFormatter.DIMENSION;
            System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, (Stream)storageFileStream, dimension1, dimension2, 0, 80);
          }
          finally
          {
            if (storageFileStream != null)
              ((IDisposable) storageFileStream).Dispose();
          }
          callback(str);
        }
        finally
        {
          if (storeForApplication != null)
            ((IDisposable) storeForApplication).Dispose();
        }
      }
      else
      {
        string localUri = localUris[ind];
        Rect rect = map[ind];
        // ISSUE: explicit reference operation
        double width = rect.Width;
        rect = map[ind];
        // ISSUE: explicit reference operation
        double height = rect.Height;
        Image image1 = ConversationTileImageFormatter.CreateImage(localUri, width, height);
        WriteableBitmap writeableBitmap = wb;
        Image image2 = image1;
        TranslateTransform translateTransform = new TranslateTransform();
        rect = map[ind];
        // ISSUE: explicit reference operation
        double x = rect.X;
        translateTransform.X = x;
        rect = map[ind];
        // ISSUE: explicit reference operation
        double y = rect.Y;
        translateTransform.Y = y;
        writeableBitmap.Render((UIElement) image2, (Transform) translateTransform);
        ConversationTileImageFormatter.ProcessImages(wb, localUris, map, callback, ind + 1, userOrChatId, isChat);
      }
    }

    private static Image CreateImage(string uri, double width, double height)
    {
      Image image = new Image();
      BitmapImage bitmapImage = new BitmapImage();
      bitmapImage.CreateOptions = ((BitmapCreateOptions) 0);
      image.Stretch=((Stretch) 3);
      ((FrameworkElement) image).Width = width;
      ((FrameworkElement) image).Height = height;
      IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication();
      try
      {
        IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(uri, (FileMode) 3, (FileAccess) 1);
        try
        {
          ((BitmapSource) bitmapImage).SetSource((Stream) storageFileStream);
        }
        finally
        {
          if (storageFileStream != null)
            ((IDisposable) storageFileStream).Dispose();
        }
      }
      finally
      {
        if (storeForApplication != null)
          ((IDisposable) storeForApplication).Dispose();
      }
      image.Source = ((ImageSource) bitmapImage);
      return image;
    }

    private static List<Rect> CreateMap(int count, bool isChat)
    {
      List<Rect> rectList = new List<Rect>();
      if (isChat)
      {
        if (count == 1)
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim, ConversationTileImageFormatter.LogicalDim));
        else if (count == 2)
        {
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
        }
        else if (count == 3)
        {
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
        }
        else if (count == 4)
        {
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
        }
        else if (count == 5)
        {
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
        }
        else if (count == 6)
        {
          rectList.Add(new Rect(0.0, 0.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(0.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
          rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
        }
        else if (count >= 7)
        {
          rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
          rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
          rectList.Add(new Rect(3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
          rectList.Add(new Rect(3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, 3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
        }
      }
      else
        rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim, ConversationTileImageFormatter.LogicalDim));
      return rectList;
    }
  }
}
