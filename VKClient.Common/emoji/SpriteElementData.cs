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
      this.Position =  new Rect();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.ElementCode);
      BinaryWriter binaryWriter1 = writer;
      Rect position1 = this.Position;
      // ISSUE: explicit reference operation
      double x = ((Rect) @position1).X;
      binaryWriter1.Write(x);
      BinaryWriter binaryWriter2 = writer;
      Rect position2 = this.Position;
      // ISSUE: explicit reference operation
      double y = ((Rect) @position2).Y;
      binaryWriter2.Write(y);
      BinaryWriter binaryWriter3 = writer;
      Rect position3 = this.Position;
      // ISSUE: explicit reference operation
      double width = ((Rect) @position3).Width;
      binaryWriter3.Write(width);
      BinaryWriter binaryWriter4 = writer;
      Rect position4 = this.Position;
      // ISSUE: explicit reference operation
      double height = ((Rect) @position4).Height;
      binaryWriter4.Write(height);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.ElementCode = reader.ReadString();
      this.Position = new Rect(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
    }
  }
}
