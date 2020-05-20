using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class DocPreviewGraffiti : IBinarySerializable
  {
    public string src { get; set; }

    public int width { get; set; }

    public int height { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.src);
      writer.Write(this.width);
      writer.Write(this.height);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.src = reader.ReadString();
      this.width = reader.ReadInt32();
      this.height = reader.ReadInt32();
    }
  }
}
