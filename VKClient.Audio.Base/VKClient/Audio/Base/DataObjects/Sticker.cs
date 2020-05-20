using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class Sticker : IBinarySerializable
  {
    public long id { get; set; }

    public int product_id { get; set; }

    public string photo_64 { get; set; }

    public string photo_128 { get; set; }

    public string photo_256 { get; set; }

    public int width { get; set; }

    public int height { get; set; }

    public void Write(BinaryWriter writer)
    {
      this.SerializeForVersion1(writer);
    }

    private void SerializeForVersion1(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.photo_64);
      writer.WriteString(this.photo_128);
      writer.WriteString(this.photo_256);
      writer.Write(this.width);
      writer.Write(this.height);
      writer.Write(this.product_id);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      int num2 = 1;
      if (num1 >= num2)
      {
        this.photo_64 = reader.ReadString();
        this.photo_128 = reader.ReadString();
        this.photo_256 = reader.ReadString();
        this.width = reader.ReadInt32();
        this.height = reader.ReadInt32();
      }
      int num3 = 2;
      if (num1 < num3)
        return;
      this.product_id = reader.ReadInt32();
    }
  }
}
