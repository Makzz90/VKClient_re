using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class Amount : IBinarySerializable
  {
    public string text { get; set; }

    public int amount { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.text);
      writer.Write(this.amount);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.text = reader.ReadString();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.amount = reader.ReadInt32();
    }
  }
}
