using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Country : IBinarySerializable
  {
    private string _title = "";

    public long id { get; set; }

    public string name
    {
      get
      {
        return this.title;
      }
      set
      {
        this.title = value;
      }
    }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public override string ToString()
    {
      return this.name;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.title);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.title = reader.ReadString();
    }
  }
}
