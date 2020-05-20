using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.AudioCache
{
  public class PlayedFilesInfo : IBinarySerializable
  {
    public readonly int MaxPlayedFiles = 25;

    public List<RemoteFileInfo> PlayedFiles { get; set; }

    public PlayedFilesInfo()
    {
      this.PlayedFiles = new List<RemoteFileInfo>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList<RemoteFileInfo>((IList<RemoteFileInfo>) this.PlayedFiles, 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.PlayedFiles = reader.ReadList<RemoteFileInfo>();
    }

    public void Save()
    {
      CacheManager.TrySerialize((IBinarySerializable) this, "PlayedAudioFiles", false, CacheManager.DataType.CachedData);
    }

    public void Restore()
    {
      CacheManager.TryDeserialize((IBinarySerializable) this, "PlayedAudioFiles", CacheManager.DataType.CachedData);
    }

    internal void Add(Uri uri, string uniqueId)
    {
      if (this.PlayedFiles.Count == this.MaxPlayedFiles)
        this.PlayedFiles.Clear();
      RemoteFileInfo remoteFileInfo1 = this.PlayedFiles.FirstOrDefault<RemoteFileInfo>((Func<RemoteFileInfo, bool>) (rfi => rfi.UniqueId == uniqueId));
      if (remoteFileInfo1 != null)
      {
        if (!(remoteFileInfo1.UriStr != uri.OriginalString))
          return;
        remoteFileInfo1.UriStr = uri.OriginalString;
        this.Save();
      }
      else
      {
        RemoteFileInfo remoteFileInfo2 = new RemoteFileInfo();
        remoteFileInfo2.UriStr = uri.OriginalString;
        string[] strArray = uniqueId.Split(new char[2]
        {
          '_',
          '|'
        });
        remoteFileInfo2.ownerId = long.Parse(strArray[0]);
        remoteFileInfo2.audioId = long.Parse(strArray[1]);
        this.PlayedFiles.Add(remoteFileInfo2);
        this.Save();
      }
    }
  }
}
