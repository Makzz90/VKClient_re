using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base
{
  public class SavePhotoResponse : IBinarySerializable
  {
    public long id { get; set; }

    public int aid
    {
      get
      {
        return this.album_id;
      }
      set
      {
        this.album_id = value;
      }
    }

    public int album_id { get; set; }

    public int owner_id { get; set; }

    public int date { get; set; }

    public string photo_130 { get; set; }

    public string photo_604 { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.Write(this.aid);
      writer.Write(this.owner_id);
      writer.Write(this.date);
      writer.WriteString(this.photo_130);
      writer.WriteString(this.photo_604);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.aid = reader.ReadInt32();
      this.owner_id = reader.ReadInt32();
      this.date = reader.ReadInt32();
      this.photo_130 = reader.ReadString();
      this.photo_604 = reader.ReadString();
    }
  }
}
