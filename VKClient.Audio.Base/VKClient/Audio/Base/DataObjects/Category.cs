using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class Category : IBinarySerializable
  {
    public long id { get; set; }

    public string name { get; set; }

    public Section section { get; set; }

    public string Name
    {
      get
      {
        return this.name.ForUI();
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.name);
      writer.Write<Section>(this.section, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.name = reader.ReadString();
      this.section = reader.ReadGeneric<Section>();
    }
  }
}
