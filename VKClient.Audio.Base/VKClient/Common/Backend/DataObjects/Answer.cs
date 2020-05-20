using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Answer : IBinarySerializable
  {
    private string _text = "";

    public long id { get; set; }

    public string text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = (value ?? "").ForUI();
      }
    }

    public int votes { get; set; }

    public double rate { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.text);
      writer.Write(this.votes);
      writer.Write(this.rate);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.text = reader.ReadString();
      this.votes = reader.ReadInt32();
      this.rate = reader.ReadDouble();
    }
  }
}
