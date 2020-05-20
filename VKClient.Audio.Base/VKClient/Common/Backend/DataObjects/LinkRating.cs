using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkRating : IBinarySerializable
  {
    public double stars { get; set; }

    public long reviews_count { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.stars);
      writer.Write(this.reviews_count);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.stars = reader.ReadDouble();
      this.reviews_count = reader.ReadInt64();
    }
  }
}
