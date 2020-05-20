using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.AudioCache
{
  public class RemoteFileInfo : IBinarySerializable
  {
    public string UriStr { get; set; }

    public long ownerId { get; set; }

    public long audioId { get; set; }

    public string UniqueId
    {
      get
      {
        long num = this.ownerId;
        string str1 = num.ToString();
        string str2 = "_";
        num = this.audioId;
        string str3 = num.ToString();
        return str1 + str2 + str3;
      }
    }

    public RemoteFileInfo(AudioObj audioObj)
    {
      this.UriStr = audioObj.url;
      this.ownerId = audioObj.owner_id;
      this.audioId = audioObj.id;
    }

    public RemoteFileInfo()
    {
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.UriStr);
      writer.Write(this.ownerId);
      writer.Write(this.audioId);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.UriStr = reader.ReadString();
      this.ownerId = reader.ReadInt64();
      this.audioId = reader.ReadInt64();
    }
  }
}
