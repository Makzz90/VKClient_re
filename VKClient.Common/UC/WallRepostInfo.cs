using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class WallRepostInfo : IBinarySerializable
  {
    public string Pic { get; set; }

    public string Name { get; set; }

    public string Subtitle { get; set; }

    public PostSourcePlatform PostSourcePlatform { get; set; }

    public double Width { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.Pic);
      writer.WriteString(this.Name);
      writer.WriteString(this.Subtitle);
      writer.Write((int) this.PostSourcePlatform);
    }

    public void Read(BinaryReader reader)
    {
      if (reader.ReadInt32() < 1)
        return;
      this.Pic = reader.ReadString();
      this.Name = reader.ReadString();
      this.Subtitle = reader.ReadString();
      this.PostSourcePlatform = (PostSourcePlatform) reader.ReadInt32();
    }
  }
}
