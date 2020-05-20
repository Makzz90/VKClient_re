using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class DocPreviewVideo : IBinarySerializable
  {
    public string src { get; set; }

    public int width { get; set; }

    public int height { get; set; }

    public long file_size { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.src);
      writer.Write(this.width);
      writer.Write(this.height);
      writer.Write(this.file_size);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.src = reader.ReadString();
      this.width = reader.ReadInt32();
      this.height = reader.ReadInt32();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.file_size = reader.ReadInt64();
    }
  }
}
