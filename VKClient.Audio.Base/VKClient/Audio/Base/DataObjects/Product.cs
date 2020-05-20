using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class Product : IHaveUniqueKey, IBinarySerializable
  {
    private string _title;
    private string _description;

    public long id { get; set; }

    public long owner_id { get; set; }

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
        return this._description;
      }
      set
      {
        this._description = (value ?? "").ForUI();
      }
    }

    public Price price { get; set; }

    public Category category { get; set; }

    public string thumb_photo { get; set; }

    public long date { get; set; }

    public int availability { get; set; }

    public List<Photo> photos { get; set; }

    public int can_comment { get; set; }

    public int can_repost { get; set; }

    public Likes likes { get; set; }

    public int views_count { get; set; }

    public bool CanComment
    {
      get
      {
        return this.can_comment == 1;
      }
    }

    public bool CanRepost
    {
      get
      {
        return this.can_repost == 1;
      }
    }

    public bool IsAvailable
    {
      get
      {
        return this.availability == 0;
      }
    }

    public override string ToString()
    {
      return string.Format("market{0}_{1}", this.owner_id, this.id);
    }

    public string GetKey()
    {
      return this.ToString();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.WriteString(this.description);
      writer.Write<Price>(this.price, false);
      writer.Write<Category>(this.category, false);
      writer.WriteString(this.thumb_photo);
      writer.Write(this.date);
      writer.Write(this.availability);
      writer.WriteList<Photo>((IList<Photo>) this.photos, 10000);
      writer.Write(this.can_comment);
      writer.Write(this.can_repost);
      writer.Write<Likes>(this.likes, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.description = reader.ReadString();
      this.price = reader.ReadGeneric<Price>();
      this.category = reader.ReadGeneric<Category>();
      this.thumb_photo = reader.ReadString();
      this.date = reader.ReadInt64();
      this.availability = reader.ReadInt32();
      this.photos = reader.ReadList<Photo>();
      this.can_comment = reader.ReadInt32();
      this.can_repost = reader.ReadInt32();
      this.likes = reader.ReadGeneric<Likes>();
    }
  }
}
