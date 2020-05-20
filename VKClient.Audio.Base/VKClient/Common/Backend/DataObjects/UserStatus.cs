using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class UserStatus : IBinarySerializable
  {
    public long online { get; set; }

    public long time { get; set; }

    public int platform { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(this.online);
      writer.Write(this.time);
    }

    public void Read(BinaryReader reader)
    {
      this.online = reader.ReadInt64();
      this.time = reader.ReadInt64();
    }
  }
}
