using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkStore : IBinarySerializable
  {
    public int id { get; set; }

    public string name { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.name);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt32();
      this.name = reader.ReadString();
    }
  }
}
