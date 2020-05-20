using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using VKClient.Audio.Base;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class SecondaryTileCreator
  {
    public static void CreateTileFor(long userOrGroupId, bool isGroup, string name, Action<bool> completionCallback, string smallPhoto)
    {
      string str = string.IsNullOrWhiteSpace(name) ? "" : HttpUtility.UrlEncode(name);
      Uri uri = new Uri(string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ClearBackStack=True", (object) userOrGroupId, (object) str), UriKind.Relative);
      SecondaryTileManager.Instance.GetSecondaryTileData(userOrGroupId, isGroup, name, 3, (Action<bool, CycleTileData>) ((res, resData) =>
      {
        if (res)
          ShellTile.Create(uri, (ShellTileData) resData, true);
        completionCallback(res);
      }), smallPhoto);
    }

    public static void CreateTileForConversation(long userOrChatId, bool isChat, string name, List<string> imagesUris, Action<bool> completionCallback)
    {
      //Action<string> action=null;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        string tileType = "Conversation";
        string objId = userOrChatId.ToString() + isChat.ToString() + AppGlobalStateManager.Current.LoggedInUserId.ToString();
        imagesUris = imagesUris.Take<string>(8).ToList<string>();
        List<WaitHandle> waitHandles = SecondaryTileManager.DownloadImages(imagesUris.Select<string, Tuple<int, string>>((Func<string, Tuple<int, string>>) (i => new Tuple<int, string>(imagesUris.IndexOf(i), i))).ToList<Tuple<int, string>>(), tileType, objId);
        new Thread((ThreadStart) (() =>
        {
          if (!WaitHandle.WaitAll(waitHandles.ToArray(), 10000))
          {
            completionCallback(false);
          }
          else
          {
            List<string> localUris = new List<string>();
            for (int ind = 0; ind < imagesUris.Count; ++ind)
            {
              if (!string.IsNullOrWhiteSpace(imagesUris[ind]))
                localUris.Add("/" + SecondaryTileManager.GetLocalNameFor(ind, tileType, objId));
            }
            ConversationTileImageFormatter.CreateTileImage(localUris, userOrChatId, isChat, /*action ?? (action = (*/new Action<string> (uriStr =>
            {
              if (!string.IsNullOrEmpty(uriStr))
              {
                FlipTileData flipTileData = new FlipTileData();
                Uri uri = new Uri("isostore:" + uriStr, UriKind.Absolute);
                flipTileData.SmallBackgroundImage = uri;
                flipTileData.BackgroundImage = uri;
                flipTileData.Title = name;
                ShellTile.Create(new Uri(string.Format("/VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}&FromLookup={2}&NewMessageContents={3}&TileLoggedInUserId={4}&ClearBackStack=True", (object) userOrChatId, (object) isChat, (object) false, (object) false, (object) AppGlobalStateManager.Current.LoggedInUserId), UriKind.Relative), (ShellTileData) flipTileData, false);
                completionCallback(true);
              }
              else
                completionCallback(false);
            }));
          }
        })).Start();
      }));
    }
  }
}
