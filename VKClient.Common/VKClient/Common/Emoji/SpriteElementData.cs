using System.IO;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Emoji
{
  public class SpriteElementData : IBinarySerializable
  {
    public Rect Position { get; set; }

    public string ElementCode { get; set; }

    public SpriteElementData()
    {
      this.Position = new Rect();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.ElementCode);
      writer.Write(this.Position.X);
      writer.Write(this.Position.Y);
      writer.Write(this.Position.Width);
      writer.Write(this.Position.Height);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.ElementCode = reader.ReadString();
      this.Position = new Rect(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
    }
  }
}
