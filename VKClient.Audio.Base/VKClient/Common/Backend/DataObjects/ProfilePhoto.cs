using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class ProfilePhoto : IBinarySerializable
  {
    public string photo_hash { get; set; }

    public string photo_src { get; set; }

    public string photo_200 { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.WriteString(this.photo_hash);
      writer.WriteString(this.photo_src);
    }

    public void Read(BinaryReader reader)
    {
      this.photo_hash = reader.ReadString();
      this.photo_src = reader.ReadString();
    }
  }
}
