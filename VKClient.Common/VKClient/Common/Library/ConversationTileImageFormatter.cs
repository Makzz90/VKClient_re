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
          localUris = localUris.Where<string>((Func<string, bool>) (u => !string.IsNullOrWhiteSpace(u))).ToList<string>();
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
          Logger.Instance.Error("CreateTileImage failed. " + (object) ex);
        }
      }));
    }

    private static void ProcessImages(WriteableBitmap wb, List<string> localUris, List<Rect> map, Action<string> callback, int ind, long userOrChatId, bool isChat)
    {
      if (ind >= localUris.Count || ind >= map.Count)
      {
        Rectangle rectangle = new Rectangle();
        rectangle.Width = (double) ConversationTileImageFormatter.DIMENSION;
        rectangle.Height = (double) ConversationTileImageFormatter.DIMENSION;
        rectangle.Fill = (Brush) new SolidColorBrush(Colors.Black);
        rectangle.Opacity = 0.2;
        wb.Render((UIElement) rectangle, (Transform) null);
        wb.Invalidate();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          string path = "/Shared/ShellContent/conversationTileImg" + (object) userOrChatId + isChat.ToString() + ".jpg";
          if (!storeForApplication.DirectoryExists("/Shared/ShellContent"))
            storeForApplication.CreateDirectory("/Shared/ShellContent");
          using (IsolatedStorageFileStream storageFileStream = new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.Write, storeForApplication))
          {
            int targetWidth = ConversationTileImageFormatter.DIMENSION;
            int targetHeight = ConversationTileImageFormatter.DIMENSION;
            wb.SaveJpeg((Stream) storageFileStream, targetWidth, targetHeight, 0, 80);
          }
          callback(path);
        }
      }
      else
      {
        string uri = localUris[ind];
        Rect rect = map[ind];
        double width = rect.Width;
        rect = map[ind];
        double height = rect.Height;
        Image image1 = ConversationTileImageFormatter.CreateImage(uri, width, height);
        WriteableBitmap writeableBitmap = wb;
        Image image2 = image1;
        TranslateTransform translateTransform = new TranslateTransform();
        rect = map[ind];
        double x = rect.X;
        translateTransform.X = x;
        rect = map[ind];
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
      bitmapImage.CreateOptions = BitmapCreateOptions.None;
      image.Stretch = Stretch.UniformToFill;
      image.Width = width;
      image.Height = height;
      using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
      {
        using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(uri, FileMode.Open, FileAccess.Read))
          bitmapImage.SetSource((Stream) storageFileStream);
      }
      image.Source = (ImageSource) bitmapImage;
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
