using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Geo : IBinarySerializable
  {
    public string type { get; set; }

    public string coordinates { get; set; }

    public Place place { get; set; }

    public string AttachmentTitle { get; set; }

    public string AttachmentSubtitle { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.type);
      writer.WriteString(this.coordinates);
      writer.WriteString(this.AttachmentTitle);
      writer.WriteString(this.AttachmentSubtitle);
    }

    public void Read(BinaryReader reader)
    {
      try
      {
        reader.ReadInt32();
        this.type = reader.ReadString();
        this.coordinates = reader.ReadString();
        this.AttachmentTitle = reader.ReadString();
        this.AttachmentSubtitle = reader.ReadString();
      }
      catch
      {
        this.type = reader.ReadString();
        this.coordinates = reader.ReadString();
      }
    }
  }
}
