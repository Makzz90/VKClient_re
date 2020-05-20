using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Video : IBinarySerializable
  {
    private string _title = "";
    private string _description = "";

    public long id { get; set; }

    public string GloballyUniqueId
    {
      get
      {
        return this.owner_id.ToString() + "_" + this.vid;
      }
    }

    public long vid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

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

    public int duration { get; set; }

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

    public int date { get; set; }

    public int views { get; set; }

    public string image
    {
      get
      {
        return this.photo_130;
      }
      set
      {
        this.photo_130 = value;
      }
    }

    public string image_big
    {
      get
      {
        return this.photo_640;
      }
      set
      {
        this.photo_640 = value;
      }
    }

    public string image_medium
    {
      get
      {
        return this.photo_320;
      }
      set
      {
        this.photo_320 = value;
      }
    }

    public string photo_130 { get; set; }

    public string photo_320 { get; set; }

    public string photo_640 { get; set; }

    public string access_key { get; set; }

    public string link { get; set; }

    public int can_comment { get; set; }

    public int can_repost { get; set; }

    public int can_edit { get; set; }

    public int can_add { get; set; }

    public int comments { get; set; }

    public string player { get; set; }

    public Dictionary<string, string> files { get; set; }

    public int processing { get; set; }

    public Guid guid { get; set; }

    public List<string> privacy_view { get; set; }

    public List<string> privacy_comment { get; set; }

    public int live { get; set; }

    public int watched { get; set; }

    public PrivacyInfo PrivacyViewInfo
    {
      get
      {
        return new PrivacyInfo(this.privacy_view);
      }
    }

    public PrivacyInfo PrivacyCommentInfo
    {
      get
      {
        return new PrivacyInfo(this.privacy_comment);
      }
    }

    public Likes likes { get; set; }

    public Video()
    {
      this.can_comment = 1;
      this.privacy_view = new List<string>();
      this.privacy_comment = new List<string>();
      this.likes = new Likes();
    }

    public override string ToString()
    {
      return string.Format("video{0}_{1}", this.owner_id, this.id);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(6);
      writer.Write(this.vid);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.Write(this.duration);
      writer.WriteString(this.description);
      writer.Write(this.date);
      writer.Write(this.views);
      writer.WriteString(this.image);
      writer.WriteString(this.image_big);
      writer.WriteString(this.image_medium);
      writer.WriteString(this.access_key);
      writer.WriteString(this.link);
      writer.WriteDictionary(this.files);
      writer.Write(this.comments);
      writer.WriteString(this.player);
      Guid guid = this.guid;
      string str = this.guid.ToString();
      writer.WriteString(str);
      writer.Write(this.can_edit);
      writer.Write(this.can_repost);
      writer.Write(this.can_comment);
      writer.Write(this.live);
      writer.Write(this.watched);
      writer.Write<Likes>(this.likes, false);
      writer.Write(this.can_add);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.vid = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.duration = reader.ReadInt32();
      this.description = reader.ReadString();
      this.date = reader.ReadInt32();
      this.views = reader.ReadInt32();
      this.image = reader.ReadString();
      this.image_big = reader.ReadString();
      this.image_medium = reader.ReadString();
      this.access_key = reader.ReadString();
      this.link = reader.ReadString();
      this.files = reader.ReadDictionary();
      this.comments = reader.ReadInt32();
      this.player = reader.ReadString();
      int num2 = 2;
      if (num1 >= num2)
      {
        string g = reader.ReadString();
        if (!string.IsNullOrEmpty(g))
          this.guid = new Guid(g);
      }
      int num3 = 3;
      if (num1 >= num3)
      {
        this.can_edit = reader.ReadInt32();
        this.can_repost = reader.ReadInt32();
        this.can_comment = reader.ReadInt32();
      }
      int num4 = 4;
      if (num1 >= num4)
      {
        this.live = reader.ReadInt32();
        this.watched = reader.ReadInt32();
      }
      int num5 = 5;
      if (num1 >= num5)
        this.likes = reader.ReadGeneric<Likes>();
      int num6 = 6;
      if (num1 < num6)
        return;
      this.can_add = reader.ReadInt32();
    }
  }
}
