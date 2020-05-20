using System;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class FriendRequest : IBinarySerializable
  {
    public long user_id { get; set; }

    public VKList<long> mutual { get; set; }

    public long from { get; set; }

    public Action<FriendRequests> RequestHandledAction { get; set; }

    public string message { get; set; }

    public FriendRequest()
    {
      this.mutual = new VKList<long>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.user_id);
      writer.Write(this.mutual.count);
      writer.WriteList(this.mutual.users);
      writer.Write(this.from);
      writer.WriteString(this.message);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.user_id = reader.ReadInt64();
      this.mutual = new VKList<long>()
      {
        count = reader.ReadInt32()
      };
      this.mutual.users = reader.ReadListLong();
      this.from = reader.ReadInt64();
      this.message = reader.ReadString();
    }
  }
}
