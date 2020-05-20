using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class SaveVideoResponse : IBinarySerializable
  {
    public string upload_url { get; set; }

    public long video_id { get; set; }

    public string title { get; set; }

    public string description { get; set; }

    public long owner_id { get; set; }

    public string access_key { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.upload_url);
      writer.Write(this.video_id);
      writer.WriteString(this.title);
      writer.WriteString(this.description);
      writer.Write(this.owner_id);
      writer.Write(this.access_key);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.upload_url = reader.ReadString();
      this.video_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.description = reader.ReadString();
      this.owner_id = reader.ReadInt64();
      this.access_key = reader.ReadString();
    }
  }
}
