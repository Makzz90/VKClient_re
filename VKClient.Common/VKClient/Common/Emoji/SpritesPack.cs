using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Emoji
{
  public class SpritesPack : IBinarySerializable
  {
    public List<SpriteDescription> SpritesVertical { get; set; }

    public List<SpriteDescription> SpritesHorizontal { get; set; }

    public SpritesPack()
    {
      this.SpritesVertical = new List<SpriteDescription>();
      this.SpritesHorizontal = new List<SpriteDescription>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList<SpriteDescription>((IList<SpriteDescription>) this.SpritesVertical, 10000);
      writer.WriteList<SpriteDescription>((IList<SpriteDescription>) this.SpritesHorizontal, 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.SpritesVertical = reader.ReadList<SpriteDescription>();
      this.SpritesHorizontal = reader.ReadList<SpriteDescription>();
    }
  }
}
