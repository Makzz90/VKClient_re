using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.Social
{
  public static class RemoteIdHelper
  {
    public static string GenerateUniqueRemoteId(string itemId, RemoteIdHelper.RemoteIdItemType itemType)
    {
      return "VK#%^" + itemType.ToString() + "#%^" + itemId;
    }

    public static long GetItemIdByRemoteId(string remoteId)
    {
      return long.Parse(remoteId.Split(new string[1]
      {
        "#%^"
      }, StringSplitOptions.RemoveEmptyEntries)[2]);
    }

    public static NewsFeedGetParams GetNewsFeedGetParamsBy(string lastNewsFeedItemRemoteId, int itemsCount)
    {
      NewsFeedGetParams newsFeedGetParams = new NewsFeedGetParams();
      newsFeedGetParams.NewsListId = -10L;
      int offset;
      string from;
      RemoteIdHelper.NewsFeedNewFromData.Instance.GetParams(lastNewsFeedItemRemoteId, out offset, out from);
      int num = itemsCount;
      newsFeedGetParams.count = num;
      string str = from;
      newsFeedGetParams.from = str;
      return newsFeedGetParams;
    }

    public class NewsFeedNewFromData : IBinarySerializable
    {
      private static object _lockObj = new object();
      public Dictionary<string, string> _dict = new Dictionary<string, string>();
      private static RemoteIdHelper.NewsFeedNewFromData _instance;

      public static RemoteIdHelper.NewsFeedNewFromData Instance
      {
        get
        {
          if (RemoteIdHelper.NewsFeedNewFromData._instance == null)
          {
            lock (RemoteIdHelper.NewsFeedNewFromData._lockObj)
            {
              RemoteIdHelper.NewsFeedNewFromData._instance = new RemoteIdHelper.NewsFeedNewFromData();
              CacheManager.TryDeserialize((IBinarySerializable) RemoteIdHelper.NewsFeedNewFromData._instance, "NewsFeedNewFromData", CacheManager.DataType.CachedData);
            }
          }
          return RemoteIdHelper.NewsFeedNewFromData._instance;
        }
      }

      public void Write(BinaryWriter writer)
      {
        writer.Write(1);
        writer.WriteDictionary(this._dict);
      }

      public void Read(BinaryReader reader)
      {
        reader.ReadInt32();
        this._dict = reader.ReadDictionary();
      }

      public void GetParams(string remoteId, out int offset, out string from)
      {
        lock (RemoteIdHelper.NewsFeedNewFromData._lockObj)
        {
          offset = 0;
          from = "";
          if (!this._dict.ContainsKey(remoteId))
            return;
          string[] strArray = this._dict[remoteId].Split(new string[1]
          {
            "|||"
          }, StringSplitOptions.RemoveEmptyEntries);
          offset = int.Parse(strArray[0]);
          from = strArray[1];
        }
      }

      public void SetParams(string remoteId, int offset, string from)
      {
        lock (RemoteIdHelper.NewsFeedNewFromData._lockObj)
        {
          string str = offset.ToString() + "|||" + from + "|||" + DateTime.UtcNow.ToString();
          this._dict[remoteId] = str;
          this.SaveState();
        }
      }

      private void SaveState()
      {
        this.CleanupOld();
        CacheManager.TrySerialize((IBinarySerializable) this, "NewsFeedNewFromData", false, CacheManager.DataType.CachedData);
      }

      private void CleanupOld()
      {
        List<string> stringList = new List<string>();
        foreach (KeyValuePair<string, string> keyValuePair in this._dict)
        {
          string[] strArray = keyValuePair.Value.Split(new string[1]
          {
            "|||"
          }, StringSplitOptions.RemoveEmptyEntries);
          DateTime result = new DateTime();
          int index = 2;
          if (DateTime.TryParse(strArray[index], out result) && (DateTime.UtcNow - result).TotalDays > 14.0)
            stringList.Add(keyValuePair.Key);
        }
        foreach (string key in stringList)
          this._dict.Remove(key);
      }
    }

    public enum RemoteIdItemType
    {
      UserOrGroup,
      WallPost,
      PictureAlbum,
    }
  }
}
