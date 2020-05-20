using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class Section : IBinarySerializable
  {
    public long id { get; set; }

    public string name { get; set; }

    public List<Section> subtypes_list { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.id);
      writer.WriteString(this.name);
      writer.WriteList<Section>((IList<Section>) this.subtypes_list, 10000);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.name = reader.ReadString();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.subtypes_list = reader.ReadList<Section>();
    }

    public override string ToString()
    {
      return this.name;
    }
  }
}
