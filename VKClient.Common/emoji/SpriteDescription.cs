using System.Collections.Generic;
using System.IO;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Emoji
{
  public class SpriteDescription : IBinarySerializable
  {
    public string SpritePath { get; set; }

    public int WidthInPixels { get; set; }

    public int HeightInPixels { get; set; }

    public List<SpriteElementData> Elements { get; set; }

    public SpriteDescription()
    {
      this.Elements = new List<SpriteElementData>();

        this.SpritePath = "";
    }

    public SpriteElementData GetElementByRelativePoint(Point p)
    {
        Point point = new Point(p.X * (double)this.WidthInPixels, p.Y * (double)this.HeightInPixels);
      foreach (SpriteElementData element in this.Elements)
      {
          if (element.Position.Contains(point))
              return element;
      }
      return  null;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.SpritePath);
      writer.Write(this.WidthInPixels);
      writer.Write(this.HeightInPixels);
      writer.WriteList<SpriteElementData>((IList<SpriteElementData>) this.Elements, 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.SpritePath = reader.ReadString();
      this.WidthInPixels = reader.ReadInt32();
      this.HeightInPixels = reader.ReadInt32();
      this.Elements = reader.ReadList<SpriteElementData>();
    }
  }
}
