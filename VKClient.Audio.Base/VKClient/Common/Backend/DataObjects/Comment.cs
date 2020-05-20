using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Comment : IBinarySerializable
  {
    private long _from_id;

    public long TopicId { get; set; }

    public long GroupId { get; set; }

    public long PostId { get; set; }

    public long PhotoId { get; set; }

    public long VideoId { get; set; }

    public int sticker_id { get; set; }

    public long cid
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

    public long from_id
    {
      get
      {
        return this._from_id;
      }
      set
      {
        this._from_id = value;
      }
    }

    public long id { get; set; }

    public int date { get; set; }

    public long post_id { get; set; }

    public long owner_id { get; set; }

    public WallPost post { get; set; }

    public Photo photo { get; set; }

    public Video video { get; set; }

    public Topic topic { get; set; }

    public Product market { get; set; }

    private string _text { get; set; }

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

    public Likes likes { get; set; }

    public long reply_to_uid
    {
      get
      {
        return this.reply_to_user;
      }
      set
      {
        this.reply_to_user = value;
      }
    }

    public long reply_to_user { get; set; }

    public long reply_to_cid
    {
      get
      {
        return this.reply_to_comment;
      }
      set
      {
        this.reply_to_comment = value;
      }
    }

    public long reply_to_comment { get; set; }

    public string message
    {
      get
      {
        return this.text;
      }
      set
      {
        this.text = value;
      }
    }

    public List<Attachment> Attachments { get; set; }

    public Comment()
    {
      this.likes = new Likes();
    }

    void IBinarySerializable.Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write(this.TopicId);
      writer.Write(this.GroupId);
      writer.Write(this.PostId);
      writer.Write(this.PhotoId);
      writer.Write(this.VideoId);
      writer.Write(this.cid);
      writer.Write(0L);
      writer.Write(this._from_id);
      writer.Write(this.date);
      writer.WriteString(this._text);
      writer.Write<Likes>(this.likes, false);
      writer.Write(this.reply_to_uid);
      writer.Write(this.reply_to_cid);
      writer.WriteList<Attachment>((IList<Attachment>) this.Attachments, 10000);
      writer.Write(this.sticker_id);
      writer.Write(this.post_id);
      writer.Write(this.owner_id);
    }

    void IBinarySerializable.Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.TopicId = reader.ReadInt64();
      this.GroupId = reader.ReadInt64();
      this.PostId = reader.ReadInt64();
      this.PhotoId = reader.ReadInt64();
      this.VideoId = reader.ReadInt64();
      this.cid = reader.ReadInt64();
      reader.ReadInt64();
      this._from_id = reader.ReadInt64();
      this.date = reader.ReadInt32();
      this._text = reader.ReadString();
      this.likes = reader.ReadGeneric<Likes>();
      this.reply_to_uid = reader.ReadInt64();
      this.reply_to_cid = reader.ReadInt64();
      this.Attachments = reader.ReadList<Attachment>();
      int num2 = 2;
      if (num1 >= num2)
        this.sticker_id = reader.ReadInt32();
      int num3 = 3;
      if (num1 < num3)
        return;
      this.post_id = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
    }
  }
}
