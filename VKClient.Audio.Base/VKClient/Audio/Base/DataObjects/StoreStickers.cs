using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class StoreStickers : IBinarySerializable
  {
    public string base_url { get; set; }

    public List<int> sticker_ids { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.base_url);
      writer.WriteList(this.sticker_ids);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.base_url = reader.ReadString();
      this.sticker_ids = reader.ReadListInt();
    }
  }
}
