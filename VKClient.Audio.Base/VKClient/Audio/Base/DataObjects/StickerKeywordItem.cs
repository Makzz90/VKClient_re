using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class StickerKeywordItem : IBinarySerializable
  {
    public List<string> words { get; set; }

    public List<int> user_stickers { get; set; }

    public List<int> promoted_stickers { get; set; }

    public StickerKeywordItem()
    {
      this.user_stickers = new List<int>();
      this.promoted_stickers = new List<int>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList(this.words);
      writer.WriteList(this.user_stickers);
      writer.WriteList(this.promoted_stickers);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.words = reader.ReadList();
      this.user_stickers = reader.ReadListInt();
      this.promoted_stickers = reader.ReadListInt();
    }
  }
}
