using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class StockItem : IBinarySerializable
  {
    private string _description;
    private string _author;
    private string _paymentType;
    private string _priceStr;

    public StoreProduct product { get; set; }

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

    public int can_purchase { get; set; }

    public bool CanPurchase
    {
      get
      {
        return this.can_purchase == 1;
      }
      set
      {
        this.can_purchase = value ? 1 : 0;
      }
    }

    public int can_purchase_for { get; set; }

    public bool CanPurchaseFor
    {
      get
      {
        return this.can_purchase_for == 1;
      }
      set
      {
        this.can_purchase_for = value ? 1 : 0;
      }
    }

    public int free { get; set; }

    public bool IsFree
    {
      get
      {
        return this.free == 1;
      }
      set
      {
        this.free = value ? 1 : 0;
      }
    }

    public string payment_type
    {
      get
      {
        return this._paymentType;
      }
      set
      {
        this._paymentType = value;
        VKClient.Audio.Base.DataObjects.PaymentType result;
        if (!Enum.TryParse<VKClient.Audio.Base.DataObjects.PaymentType>(this._paymentType, true, out result))
          return;
        this.PaymentType = new VKClient.Audio.Base.DataObjects.PaymentType?(result);
      }
    }

    public VKClient.Audio.Base.DataObjects.PaymentType? PaymentType { get; set; }

    public int price { get; set; }

    public string price_str
    {
      get
      {
        return this._priceStr;
      }
      set
      {
        this._priceStr = (value ?? "").ForUI();
      }
    }

    public string merchant_product_id { get; set; }

    public string photo_35 { get; set; }

    public string photo_70 { get; set; }

    public string photo_140 { get; set; }

    public string photo_296 { get; set; }

    public string photo_592 { get; set; }

    public string background { get; set; }

    public List<string> demo_photos_560 { get; set; }

    public int @new { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write<StoreProduct>(this.product, false);
      writer.WriteString(this.description);
      writer.WriteString(this.author);
      writer.Write(this.can_purchase);
      writer.Write(this.free);
      writer.WriteString(this.payment_type);
      writer.Write(this.price);
      writer.WriteString(this.price_str);
      writer.WriteString(this.merchant_product_id);
      writer.WriteString(this.photo_35);
      writer.WriteString(this.photo_70);
      writer.WriteString(this.photo_140);
      writer.WriteString(this.photo_296);
      writer.WriteString(this.photo_592);
      writer.WriteString(this.background);
      writer.WriteList(this.demo_photos_560);
      writer.Write(this.@new);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.product = reader.ReadGeneric<StoreProduct>();
      this.description = reader.ReadString();
      this.author = reader.ReadString();
      this.can_purchase = reader.ReadInt32();
      this.free = reader.ReadInt32();
      this.payment_type = reader.ReadString();
      this.price = reader.ReadInt32();
      this.price_str = reader.ReadString();
      this.merchant_product_id = reader.ReadString();
      this.photo_35 = reader.ReadString();
      this.photo_70 = reader.ReadString();
      this.photo_140 = reader.ReadString();
      this.photo_296 = reader.ReadString();
      this.photo_592 = reader.ReadString();
      this.background = reader.ReadString();
      this.demo_photos_560 = reader.ReadList();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.@new = reader.ReadInt32();
    }
  }
}
