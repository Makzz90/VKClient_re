using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend.DataObjects
{
  public class WallPost : IBinarySerializable
  {
    private string _text = "";
    private Likes _likes = new Likes();

    public string GloballyUniqueId
    {
      get
      {
        return this.to_id.ToString() + "_" + (this.id == 0L ? (long) this.date : this.id).ToString();
      }
    }

    public string PostId
    {
      get
      {
        return this.GloballyUniqueId;
      }
    }

    public List<string> CopyPostIds
    {
      get
      {
        List<string> stringList = new List<string>();
        if (!this.copy_history.IsNullOrEmpty())
        {
          foreach (WallPost wallPost in this.copy_history)
            stringList.Add(wallPost.owner_id.ToString() + "_" + this.id);
        }
        return stringList;
      }
    }

    public long owner_id { get; set; }

    public long WallPostOrReplyPostId
    {
      get
      {
        if (!(this.post_type == "post"))
          return this.reply_post_id;
        return this.id;
      }
    }

    public long id { get; set; }

    public long to_id
    {
      get
      {
        return this.owner_id;
      }
      set
      {
        this.owner_id = value;
      }
    }

    public long post_id { get; set; }

    public long reply_owner_id { get; set; }

    public long reply_post_id { get; set; }

    public int is_pinned { get; set; }

    public long from_id { get; set; }

    public int date { get; set; }

    public int friends_only { get; set; }

    public int fixed_post
    {
      get
      {
        return this.is_pinned;
      }
      set
      {
        this.is_pinned = value;
      }
    }

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

    public string post_type { get; set; }

    public Comments comments { get; set; }

    public Likes likes
    {
      get
      {
        return this._likes;
      }
      set
      {
        this._likes = value;
      }
    }

    public Reposts reposts { get; set; }

    public List<Attachment> attachments { get; set; }

    public Geo geo { get; set; }

    public PostSource post_source { get; set; }

    public long signer_id { get; set; }

    public int marked_as_ads { get; set; }

    public int can_publish { get; set; }

    public bool IsSuggestedPostponed
    {
      get
      {
        if (!this.IsSuggested)
          return this.IsPostponed;
        return true;
      }
    }

    public bool IsSuggested
    {
      get
      {
        return this.post_type == "suggest";
      }
    }

    public bool IsPostponed
    {
      get
      {
        return this.post_type == "postpone";
      }
    }

    public bool IsReply
    {
      get
      {
        return this.post_type == "reply";
      }
    }

    public bool IsMarkedAsAds
    {
      get
      {
        return this.marked_as_ads == 1;
      }
    }

    public List<WallPost> copy_history { get; set; }

    public NewsActivity activity { get; set; }

    public NewsCaption caption { get; set; }

    public bool IsNotExist
    {
      get
      {
        return this.PostId == "0_0";
      }
    }

    public WallPost()
    {
      this.reposts = new Reposts();
      this.comments = new Comments();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write(this.id);
      writer.Write(this.to_id);
      writer.Write(this.from_id);
      writer.Write(this.date);
      writer.WriteList<Attachment>((IList<Attachment>) this.attachments, 10000);
      writer.Write(this.signer_id);
      writer.WriteList<WallPost>((IList<WallPost>) this.copy_history, 10000);
      writer.Write(this.reply_post_id);
      writer.Write(this.marked_as_ads);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      int num2 = 1;
      if (num1 >= num2)
      {
        this.id = reader.ReadInt64();
        this.to_id = reader.ReadInt64();
        this.from_id = reader.ReadInt64();
        this.date = reader.ReadInt32();
        this.attachments = reader.ReadList<Attachment>();
        this.signer_id = reader.ReadInt64();
      }
      int num3 = 2;
      if (num1 >= num3)
      {
        this.copy_history = reader.ReadList<WallPost>();
        this.reply_post_id = reader.ReadInt64();
      }
      int num4 = 3;
      if (num1 < num4)
        return;
      this.marked_as_ads = reader.ReadInt32();
    }

    public static WallPost CreateFromNewsItem(NewsItem newsItem)
    {
      WallPost wallPost = new WallPost();
      wallPost.id = WallPost.GetZeroIfNull(new long?(newsItem.post_id));
      if (newsItem.post_id == 0L)
        wallPost.id = newsItem.id;
      wallPost.to_id = WallPost.GetZeroIfNull(new long?(newsItem.source_id));
      if (newsItem.owner_id != 0L)
        wallPost.to_id = newsItem.owner_id;
      wallPost.from_id = newsItem.from_id == 0L ? WallPost.GetZeroIfNull(new long?(newsItem.source_id)) : newsItem.from_id;
      wallPost.date = newsItem.date;
      wallPost.text = newsItem.text;
      wallPost.comments = newsItem.comments;
      wallPost.likes = newsItem.likes;
      wallPost.reposts = newsItem.reposts;
      wallPost.attachments = newsItem.attachments;
      wallPost.geo = newsItem.geo;
      wallPost.post_source = newsItem.post_source;
      wallPost.signer_id = WallPost.GetZeroIfNull(newsItem.signer_id);
      wallPost.marked_as_ads = WallPost.GetZeroIfNull(newsItem.marked_as_ads);
      wallPost.reply_post_id = newsItem.reply_post_id;
      wallPost.friends_only = newsItem.friends_only;
      wallPost.post_type = newsItem.post_type;
      if (wallPost.attachments == null)
        wallPost.attachments = new List<Attachment>();
      if (newsItem.copy_history != null)
      {
        List<WallPost> wallPostList = new List<WallPost>();
        foreach (NewsItem newsItem1 in newsItem.copy_history)
          wallPostList.Add(WallPost.CreateFromNewsItem(newsItem1));
        wallPost.copy_history = wallPostList;
      }
      if (newsItem.Photos != null)
      {
        foreach (Photo photo in newsItem.Photos)
          wallPost.attachments.Add(new Attachment()
          {
            photo = photo,
            type = "photo"
          });
      }
      if (newsItem.Photo_tags != null)
      {
        foreach (Photo photoTag in newsItem.Photo_tags)
          wallPost.attachments.Add(new Attachment()
          {
            photo = photoTag,
            type = "photo"
          });
      }
      wallPost.activity = newsItem.activity;
      wallPost.caption = newsItem.caption;
      return wallPost;
    }

    private static long GetZeroIfNull(long? val)
    {
      if (!val.HasValue)
        return 0;
      return val.Value;
    }

    private static int GetZeroIfNull(int? val)
    {
      if (!val.HasValue)
        return 0;
      return val.Value;
    }
  }
}
