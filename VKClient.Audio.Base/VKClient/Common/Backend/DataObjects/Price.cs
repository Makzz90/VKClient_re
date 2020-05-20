using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Price : IBinarySerializable
  {
    public long amount { get; set; }

    public Currency currency { get; set; }

    public string text { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<Currency>(this.currency, false);
      writer.WriteString(this.text);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.currency = reader.ReadGeneric<Currency>();
      this.text = reader.ReadString();
    }
  }
}
