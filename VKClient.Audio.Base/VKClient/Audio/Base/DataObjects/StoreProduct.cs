using System;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class StoreProduct : IBinarySerializable
  {
    private string _type;
    private string _title;
    private string _description;
    private string _author;

    public int id { get; set; }

    public string type
    {
      get
      {
        return this._type;
      }
      set
      {
        this._type = value;
        StoreProductType result;
        if (!Enum.TryParse<StoreProductType>(this._type, true, out result))
          return;
        this.Type = new StoreProductType?(result);
      }
    }

    public StoreProductType? Type { get; set; }

    public int purchased { get; set; }

    public int active { get; set; }

    public int promoted { get; set; }

    public int purchase_date { get; set; }

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

    public string base_url { get; set; }

    public StoreStickers stickers { get; set; }

    public string description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = (value ?? "").ForUI();
      }
    }

    public string author
    {
      get
      {
        return this._author;
      }
      set
      {
        this._author = (value ?? "").ForUI();
      }
    }

    public string photo_140 { get; set; }

    public int votes { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(4);
      writer.Write(this.id);
      writer.WriteString(this.type);
      writer.Write(this.purchased);
      writer.Write(this.active);
      writer.Write(this.purchase_date);
      writer.WriteString(this.title);
      writer.WriteString(this.base_url);
      writer.Write<StoreStickers>(this.stickers, false);
      writer.WriteString(this.description);
      writer.WriteString(this.author);
      writer.WriteString(this.photo_140);
      writer.Write(this.votes);
      writer.Write(this.promoted);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      int num2 = 1;
      if (num1 >= num2)
      {
        this.id = reader.ReadInt32();
        this.type = reader.ReadString();
        this.purchased = reader.ReadInt32();
        this.active = reader.ReadInt32();
        this.purchase_date = reader.ReadInt32();
        this.title = reader.ReadString();
        this.base_url = reader.ReadString();
        this.stickers = reader.ReadGeneric<StoreStickers>();
      }
      int num3 = 2;
      if (num1 >= num3)
      {
        this.description = reader.ReadString();
        this.author = reader.ReadString();
        this.photo_140 = reader.ReadString();
      }
      int num4 = 3;
      if (num1 >= num4)
        this.votes = reader.ReadInt32();
      int num5 = 4;
      if (num1 < num5)
        return;
      this.promoted = reader.ReadInt32();
    }
  }
}
