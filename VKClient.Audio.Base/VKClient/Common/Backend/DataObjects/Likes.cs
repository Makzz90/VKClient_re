using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Likes : IBinarySerializable
  {
    public int count { get; set; }

    public int user_likes { get; set; }

    public int can_like { get; set; }

    public int can_publish { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.count);
      writer.Write(this.user_likes);
      writer.Write(this.can_like);
      writer.Write(this.can_publish);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.count = reader.ReadInt32();
      this.user_likes = reader.ReadInt32();
      this.can_like = reader.ReadInt32();
      this.can_publish = reader.ReadInt32();
    }
  }
}
