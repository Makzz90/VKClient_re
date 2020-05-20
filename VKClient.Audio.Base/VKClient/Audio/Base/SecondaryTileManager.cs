using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base
{
  public class SecondaryTileManager
  {
    private static readonly string LastUpdatedKey = "LastUpdated";
    private static SecondaryTileManager _instance;

    public static SecondaryTileManager Instance
    {
      get
      {
        if (SecondaryTileManager._instance == null)
          SecondaryTileManager._instance = new SecondaryTileManager();
        return SecondaryTileManager._instance;
      }
    }

    public bool TileExistsFor(long userOrGroupId, bool isGroup)
    {
      return this.FindTile(userOrGroupId, isGroup) != null;
    }

    public bool TileExistsForConversation(long userOrChatId, bool isChat)
    {
      if (isChat)
        return ShellTile.ActiveTiles.FirstOrDefault<ShellTile>((Func<ShellTile, bool>) (t =>
        {
          if (t.NavigationUri.OriginalString.Contains(string.Format("VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}", userOrChatId, isChat)))
            return t.NavigationUri.OriginalString.Contains("TileLoggedInUserId=" + AppGlobalStateManager.Current.LoggedInUserId);
          return false;
        })) != null;
      return ShellTile.ActiveTiles.FirstOrDefault<ShellTile>((Func<ShellTile, bool>) (t => t.NavigationUri.OriginalString.Contains(string.Format("VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}", userOrChatId, isChat)))) != null;
    }

    public void DeleteTileFor(long userOrGroupId, bool isGroup)
    {
      ShellTile tile = this.FindTile(userOrGroupId, isGroup);
      if (tile == null)
        return;
      tile.Delete();
    }

    private ShellTile FindTile(long userOrGroupId, bool isGroup)
    {
      return ShellTile.ActiveTiles.FirstOrDefault<ShellTile>((Func<ShellTile, bool>) (t => t.NavigationUri.OriginalString.Contains(string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}", userOrGroupId))));
    }

    public void UpdateAllExistingTiles(Action<bool> completionCallback)
    {
      Logger.Instance.Info("Entering SecondaryTileManager.UpdateAllExistingTiles");
      this.ProcessTile(0, ((IEnumerable<ShellTile>) ShellTile.ActiveTiles.Where<ShellTile>((Func<ShellTile, bool>) (t =>
      {
        if (t.NavigationUri !=  null)
          return t.NavigationUri.OriginalString.Contains("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId=");
        return false;
      })).OrderBy<ShellTile, int>(new Func<ShellTile, int>(this.GetLastUpdatedDate))).ToList<ShellTile>(), completionCallback);
    }

    private int GetLastUpdatedDate(ShellTile t)
    {
      Dictionary<string, string> queryString = t.NavigationUri.ParseQueryString();
      if (queryString.ContainsKey(SecondaryTileManager.LastUpdatedKey))
        return int.Parse(queryString[SecondaryTileManager.LastUpdatedKey]);
      return 0;
    }

    private void ProcessTile(int ind, List<ShellTile> list, Action<bool> completionCallback)
    {
      if (ind >= list.Count)
      {
        completionCallback(true);
      }
      else
      {
        ShellTile tile = list[ind];
        Dictionary<string, string> queryString = tile.NavigationUri.ParseQueryString();
        bool isGroup = queryString.ContainsKey("GroupId");
        long userOrGroupId = isGroup ? long.Parse(queryString["GroupId"]) : long.Parse(queryString["UserOrGroupId"]);
        string name = queryString["Name"];
        this.GetSecondaryTileData(userOrGroupId, isGroup, name, 5, (Action<bool, CycleTileData>) ((res, resData) =>
        {
          if (res)
          {
            Logger.Instance.Info("Updating secondary tile with new info " + tile.NavigationUri);
            try
            {
              tile.Update((ShellTileData) resData);
            }
            catch (Exception )
            {
            }
          }
          else
            Logger.Instance.Error("Failed to process tile update " + tile.NavigationUri);
          this.ProcessTile(ind + 1, list, completionCallback);
        }),  null);
      }
    }

    public void GetSecondaryTileData(long userOrGroupId, bool isGroup, string name, int maxImages, Action<bool, CycleTileData> completionCallback, string smallPhoto = null)
    {
      //Func<Tuple<int, string>, bool> func1;
      //Func<Tuple<int, string>, Uri> func2;
      this.GetRemoteUrisFor(Math.Abs(userOrGroupId), isGroup, (Action<bool, List<string>>) ((res, resData) =>
      {
        if (res)
        {
          List<Tuple<int, string>> imagesToDownloadData = new List<Tuple<int, string>>();
          resData = resData.Take<string>(maxImages).ToList<string>();
          int num1 = 0;
          foreach (string str in resData)
          {
            imagesToDownloadData.Add(new Tuple<int, string>(num1, str));
            ++num1;
          }
          if (resData.Count == 0)
          {
            Action<bool, CycleTileData> action = completionCallback;
            int num2 = 1;
            CycleTileData cycleTileData = new CycleTileData();
            string str = SecondaryTileManager.FormatNameForTile(name);
            ((ShellTileData) cycleTileData).Title = str;
            action(num2 != 0, cycleTileData);
          }
          else
          {
            if (!string.IsNullOrWhiteSpace(smallPhoto))
              imagesToDownloadData.Add(new Tuple<int, string>(1000, smallPhoto));
            List<WaitHandle> waitHandles = SecondaryTileManager.DownloadImages(imagesToDownloadData, "UserOrGroup", userOrGroupId.ToString() + isGroup.ToString());
            new Thread((ThreadStart) (() =>
            {
              if (!WaitHandle.WaitAll(waitHandles.ToArray(), 15000))
              {
                completionCallback(false,  null);
              }
              else
              {
                CycleTileData cycleTileData = new CycleTileData();
                string str = SecondaryTileManager.FormatNameForTile(name);
                ((ShellTileData) cycleTileData).Title = str;
                List<Uri> list = imagesToDownloadData.Where<Tuple<int, string>>(/*func1 ?? (func1 = (*/new Func<Tuple<int, string>, bool>(im => im.Item2 != smallPhoto)).Select<Tuple<int, string>, Uri>(/*func2 ?? (func2 = (*/new Func<Tuple<int, string>, Uri>(im => new Uri("isostore:/" + SecondaryTileManager.GetLocalNameFor(im.Item1, "UserOrGroup", userOrGroupId.ToString() + isGroup.ToString()), UriKind.Absolute))).ToList<Uri>();
                                    
                cycleTileData.CycleImages=((IEnumerable<Uri>) list);
                Uri uri = smallPhoto == null ?  null : new Uri("isostore:/" + SecondaryTileManager.GetLocalNameFor(1000, "UserOrGroup", userOrGroupId.ToString() + isGroup.ToString()), UriKind.Absolute);
                cycleTileData.SmallBackgroundImage = uri;
                completionCallback(true, cycleTileData);
              }
            })).Start();
          }
        }
        else
          completionCallback(false,  null);
      }));
    }

    public static string FormatNameForTile(string name)
    {
      name = HttpUtility.UrlDecode(name);
      return name;
    }

    private void GetRemoteUrisFor(long userOrGroupId, bool isGroup, Action<bool, List<string>> completionCallback)
    {
      PhotosService.Current.GetAllPhotos(userOrGroupId, isGroup, 0, 9, (Action<BackendResult<PhotosListWithCount, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
        {
          completionCallback(false,  null);
        }
        else
        {
          List<string> stringList1 = new List<string>();
          if (res.ResultData.response != null && res.ResultData.response.Count > 0)
          {
            foreach (Photo photo in res.ResultData.response)
              stringList1.Add(photo.src_big);
            completionCallback(true, stringList1);
          }
          else
            PhotosService.Current.GetPhotos(userOrGroupId, isGroup, "profile",  null, 0, "", (Action<BackendResult<List<Photo>, ResultCode>>) (result =>
            {
              if (result.ResultCode != ResultCode.Succeeded)
              {
                completionCallback(false,  null);
              }
              else
              {
                List<string> stringList = new List<string>();
                foreach (Photo photo in result.ResultData)
                  stringList.Add(photo.src_big);
                completionCallback(true, stringList);
              }
            }));
        }
      }));
    }

    private ShellTile FindTile(string partOfUri)
    {
      return ShellTile.ActiveTiles.FirstOrDefault<ShellTile>((Func<ShellTile, bool>) (tile => tile.NavigationUri.ToString().Contains(partOfUri)));
    }

    public static List<WaitHandle> DownloadImages(List<Tuple<int, string>> indexedUris, string tileType, string objId)
    {
      List<WaitHandle> waitHandleList = new List<WaitHandle>();
      foreach (Tuple<int, string> indexedUri in indexedUris)
      {
        int ind = indexedUri.Item1;
        string str = indexedUri.Item2;
        EventWaitHandle threadFinish = new EventWaitHandle(false, EventResetMode.ManualReset);
        waitHandleList.Add((WaitHandle) threadFinish);
        Stream cachedImageStream = ImageCache.Current.GetCachedImageStream(str);
        string localName = SecondaryTileManager.GetLocalNameFor(ind, tileType, objId);
        if (cachedImageStream != null)
        {
          try
          {
            using (IsolatedStorageFileStream storageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(localName, FileMode.Create, FileAccess.ReadWrite))
            {
              using (cachedImageStream)
              {
                byte[] buffer = new byte[1024];
                while (cachedImageStream.Read(buffer, 0, buffer.Length) > 0)
                  storageFileStream.Write(buffer, 0, buffer.Length);
              }
            }
          }
          catch (Exception ex)
          {
            Logger.Instance.Error("SecondaryTileManager.DownloadImages failed", ex);
          }
          finally
          {
            threadFinish.Set();
          }
        }
        else
        {
          try
          {
            HttpWebRequest request = WebRequest.CreateHttp(str);
            request.BeginGetResponse((AsyncCallback) (ir =>
            {
              try
              {
                WebResponse response = request.EndGetResponse(ir);
                using (IsolatedStorageFileStream storageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(localName, FileMode.Create, FileAccess.ReadWrite))
                {
                  using (Stream responseStream = response.GetResponseStream())
                  {
                    byte[] buffer = new byte[1024];
                    while (responseStream.Read(buffer, 0, buffer.Length) > 0)
                      storageFileStream.Write(buffer, 0, buffer.Length);
                  }
                }
              }
              catch (Exception )
              {
              }
              finally
              {
                threadFinish.Set();
              }
            }), null);
          }
          catch (Exception )
          {
            threadFinish.Set();
          }
        }
      }
      return waitHandleList;
    }

    public static string GetLocalNameFor(int ind, string tileType, string objId)
    {
      return "Shared/ShellContent/" + ("SecTileData_" + tileType + "_" + objId + "_" + ind);
    }
  }
}
