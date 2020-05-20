using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkProduct : IBinarySerializable
  {
    public Price price { get; set; }

    public string description { get; set; }

    public long owner_id { get; set; }

    public long id { get; set; }

    public LinkProduct(Product product)
    {
      this.price = product.price;
      this.description = product.description;
      this.owner_id = product.owner_id;
      this.id = product.id;
    }

    public LinkProduct()
    {
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write<Price>(this.price, false);
      writer.WriteString(this.description);
      writer.Write(this.owner_id);
      writer.Write(this.id);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.price = reader.ReadGeneric<Price>();
      int num2 = 3;
      if (num1 < num2)
        return;
      this.description = reader.ReadString();
      this.owner_id = reader.ReadInt64();
      this.id = reader.ReadInt64();
    }
  }
}
