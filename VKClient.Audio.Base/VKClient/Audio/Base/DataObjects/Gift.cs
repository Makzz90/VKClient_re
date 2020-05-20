using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class Gift : IBinarySerializable
  {
    public long id { get; set; }

    public string thumb_256 { get; set; }

    public long stickers_product_id { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.id);
      writer.WriteString(this.thumb_256);
      writer.Write(this.stickers_product_id);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.thumb_256 = reader.ReadString();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.stickers_product_id = reader.ReadInt64();
    }
  }
}
