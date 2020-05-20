using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class StickersKeywordsData : IBinarySerializable
  {
    public string base_url { get; set; }

    public int count { get; set; }

    public List<StickerKeywordItem> dictionary { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.base_url);
      writer.Write(this.count);
      writer.WriteList<StickerKeywordItem>((IList<StickerKeywordItem>) this.dictionary, 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.base_url = reader.ReadString();
      this.count = reader.ReadInt32();
      this.dictionary = reader.ReadList<StickerKeywordItem>();
    }
  }
}
