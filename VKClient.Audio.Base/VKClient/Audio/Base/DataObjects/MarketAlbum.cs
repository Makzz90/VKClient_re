using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class MarketAlbum : IBinarySerializable
  {
    public long id { get; set; }

    public long owner_id { get; set; }

    public string title { get; set; }

    public Photo photo { get; set; }

    public int count { get; set; }

    public int updated_time { get; set; }

    public VKList<Product> products { get; set; }

    public string Title
    {
      get
      {
        return this.title.ForUI();
      }
    }

    public override string ToString()
    {
      return string.Format("market_album{0}_{1}", this.owner_id, this.id);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.Write<Photo>(this.photo, false);
      writer.Write(this.count);
      writer.Write(this.updated_time);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.photo = reader.ReadGeneric<Photo>();
      this.count = reader.ReadInt32();
      this.updated_time = reader.ReadInt32();
    }
  }
}
