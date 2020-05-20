using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class NewsItem
  {
    private string _text = string.Empty;
    private string _post_type;

    public long id { get; set; }

    public User user { get; set; }

    public Group group { get; set; }

    public User[] profiles { get; set; }

    public string type { get; set; }

    public string next_from { get; set; }

    public int? account_import_block_pos { get; set; }

    public string post_type
    {
      get
      {
        return this._post_type ?? this.type;
      }
      set
      {
        this._post_type = value;
      }
    }

    public NewsItemType NewsItemType
    {
      get
      {
        string postType = this.post_type;
        if (postType == "post" || postType == "reply")
          return NewsItemType.post;
        if (postType == "photo")
          return NewsItemType.photo;
        return postType == "photo_tag" ? NewsItemType.photo_tag : NewsItemType.unknown;
      }
    }

    public long source_id { get; set; }

    public int date { get; set; }

    public List<object> friends { get; set; }

    public long post_id { get; set; }

    public long reply_post_id { get; set; }

    public int friends_only { get; set; }

    public long from_id { get; set; }

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

    public List<NewsItem> copy_history { get; set; }

    public List<Attachment> attachments { get; set; }

    public Comments comments { get; set; }

    public Likes likes { get; set; }

    public Reposts reposts { get; set; }

    public PostSource post_source { get; set; }

    public Geo geo { get; set; }

    public long? signer_id { get; set; }

    public int? marked_as_ads { get; set; }

    public VKList<Video> video { get; set; }

    public int PhotosCount
    {
      get
      {
        if (this.photos != null)
          return this.photos.count;
        return 0;
      }
    }

    public List<Photo> Photos
    {
      get
      {
        if (this.photos != null)
          return this.photos.items;
        return  null;
      }
    }

    public VKList<Photo> photos { get; set; }

    public int PhotoTagsCount
    {
      get
      {
        if (this.photo_tags != null)
          return this.photo_tags.count;
        return 0;
      }
    }

    public List<Photo> Photo_tags
    {
      get
      {
        if (this.photo_tags != null)
          return this.photo_tags.items;
        return  null;
      }
    }

    public VKList<Photo> photo_tags { get; set; }

    public long album_id { get; set; }

    public long pid
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

    public long aid
    {
      get
      {
        return this.album_id;
      }
      set
      {
        this.album_id = value;
      }
    }

    public int width { get; set; }

    public int height { get; set; }

    public long owner_id { get; set; }

    public string photo_75 { get; set; }

    public string photo_604 { get; set; }

    public string photo_807 { get; set; }

    public string photo_1280 { get; set; }

    public string src_big
    {
      get
      {
        return this.photo_604;
      }
      set
      {
        this.photo_604 = value;
      }
    }

    public string src_xbig
    {
      get
      {
        return this.photo_807;
      }
      set
      {
        this.photo_807 = value;
      }
    }

    public string src_small
    {
      get
      {
        return this.photo_75;
      }
      set
      {
        this.photo_75 = value;
      }
    }

    public string src_xxbig
    {
      get
      {
        return this.photo_1280;
      }
      set
      {
        this.photo_1280 = value;
      }
    }

    public int created { get; set; }

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

    public int duration { get; set; }

    public string image_big
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

    public string photo_320 { get; set; }

    public string ads_title { get; set; }

    public long ads_id1 { get; set; }

    public long ads_id2 { get; set; }

    public List<Ad> ads { get; set; }

    public List<AdStatistics> ads_statistics { get; set; }

    public string title { get; set; }

    public Price price { get; set; }

    public Category category { get; set; }

    public int availability { get; set; }

    public string thumb_photo { get; set; }

    public NewsActivity activity { get; set; }

    public NewsCaption caption { get; set; }

    public NewsItem()
    {
      this.ads_statistics = new List<AdStatistics>();
    }
  }
}
