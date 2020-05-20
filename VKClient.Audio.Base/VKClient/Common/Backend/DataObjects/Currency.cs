using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Currency : IBinarySerializable
  {
    public static readonly Currency RUB = new Currency()
    {
      id = 643,
      name = "RUB"
    };
    public static readonly Currency UAH = new Currency()
    {
      id = 980,
      name = "UAH"
    };
    public static readonly Currency KZT = new Currency()
    {
      id = 398,
      name = "KZT"
    };
    public static readonly Currency EUR = new Currency()
    {
      id = 978,
      name = "EUR"
    };
    public static readonly Currency USD = new Currency()
    {
      id = 840,
      name = "USD"
    };

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
