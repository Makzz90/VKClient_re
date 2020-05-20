using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Link : IBinarySerializable
  {
    private string _title = "";
    private string _desc = "";
    private string _caption;

    public string url { get; set; }

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

    public string description
    {
      get
      {
        return this._desc;
      }
      set
      {
        this._desc = (value ?? "").ForUI();
      }
    }

    public string photo_100 { get; set; }

    public string image_big { get; set; }

    public string image_src { get; set; }

    public string caption
    {
      get
      {
        return this._caption;
      }
      set
      {
        this._caption = (value ?? "").ForUI();
      }
    }

    public Photo photo { get; set; }

    public LinkProduct product { get; set; }

    public LinkButton button { get; set; }

    public LinkApplication application { get; set; }

    public LinkRating rating { get; set; }

    public MoneyTransfer money_transfer { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(4);
      writer.WriteString(this.url);
      writer.WriteString(this.title);
      writer.WriteString(this.description);
      writer.WriteString(this.photo_100);
      writer.WriteString(this.image_big);
      writer.WriteString(this.image_src);
      writer.WriteString(this.caption);
      writer.Write<Photo>(this.photo, false);
      writer.Write<LinkProduct>(this.product, false);
      writer.Write<LinkButton>(this.button, false);
      writer.Write<LinkApplication>(this.application, false);
      writer.Write<LinkRating>(this.rating, false);
      writer.Write<MoneyTransfer>(this.money_transfer, false);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.url = reader.ReadString();
      this.title = reader.ReadString();
      this.description = reader.ReadString();
      this.photo_100 = reader.ReadString();
      int num2 = 2;
      if (num1 >= num2)
      {
        this.image_big = reader.ReadString();
        this.image_src = reader.ReadString();
      }
      int num3 = 3;
      if (num1 >= num3)
      {
        this.caption = reader.ReadString();
        this.photo = reader.ReadGeneric<Photo>();
        this.product = reader.ReadGeneric<LinkProduct>();
        this.button = reader.ReadGeneric<LinkButton>();
        this.application = reader.ReadGeneric<LinkApplication>();
        this.rating = reader.ReadGeneric<LinkRating>();
      }
      int num4 = 4;
      if (num1 < num4)
        return;
      this.money_transfer = reader.ReadGeneric<MoneyTransfer>();
    }
  }
}
