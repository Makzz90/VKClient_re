using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Audio.Base.Core;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.AudioCache
{
  public class MediaLRUCache : IBinarySerializable
  {
    private static readonly char SEPARATOR_CHAR = '_';
    public static readonly int DEFAULT_CAPACITY = 50000000;
    private LRUCache<string, string> _lruCache;
    private int _capacity;
    private static MediaLRUCache _instance;

    public static MediaLRUCache Instance
    {
      get
      {
        if (MediaLRUCache._instance == null)
        {
          MediaLRUCache._instance = new MediaLRUCache(MediaLRUCache.DEFAULT_CAPACITY);
          CacheManager.TryDeserialize((IBinarySerializable) MediaLRUCache._instance, "MediaLRUCache", CacheManager.DataType.CachedData);
        }
        return MediaLRUCache._instance;
      }
    }

    public MediaLRUCache(int capacity)
    {
      this._capacity = capacity;
      this.InstantiateLruCache();
    }

    public void Save()
    {
      CacheManager.TrySerialize((IBinarySerializable) this, "MediaLRUCache", false, CacheManager.DataType.CachedData);
    }

    private async void HandleRemovedItem(LRUCacheItem<string, string> ci)
    {
      await CacheManager.TryDeleteAsync(MediaLRUCache.RemoveLengthPrefixFromLocalFilePath(ci.value));
    }

    public void AddLocalFile(string uri, string filePath, int fileSize)
    {
      this._lruCache.Add(uri, fileSize.ToString() + "_" + filePath, true);
    }

    public string GetLocalFile(string uri)
    {
      string localFile = this._lruCache.Get(uri);
      if (string.IsNullOrEmpty(localFile))
        return  null;
      return MediaLRUCache.RemoveLengthPrefixFromLocalFilePath(localFile);
    }

    public bool HasLocalFile(string uri)
    {
      return !string.IsNullOrEmpty(this.GetLocalFile(uri));
    }

    private static string RemoveLengthPrefixFromLocalFilePath(string localFile)
    {
      return localFile.Remove(0, localFile.IndexOf(MediaLRUCache.SEPARATOR_CHAR) + 1);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this._capacity);
      writer.WriteDictionary(this.ConvertLruCacheToDict());
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._capacity = reader.ReadInt32();
      this.InstantiateLruCache();
      foreach (KeyValuePair<string, string> read in reader.ReadDictionary())
        this._lruCache.Add(read.Key, read.Value, false);
    }

    private Dictionary<string, string> ConvertLruCacheToDict()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      foreach (LRUCacheItem<string, string> lruCacheItem in this._lruCache)
        dictionary[lruCacheItem.key] = lruCacheItem.value;
      return dictionary;
    }

    private void InstantiateLruCache()
    {
      this._lruCache = new LRUCache<string, string>(this._capacity, (Func<string, int>) (filePath => int.Parse(((IEnumerable<string>) filePath.Split(MediaLRUCache.SEPARATOR_CHAR)).First<string>())));
      this._lruCache.OnRemovedCallback = new Action<LRUCacheItem<string, string>>(this.HandleRemovedItem);
    }
  }
}
