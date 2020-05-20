using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoAlbum
  {
    public static readonly long ADDED_ALBUM_ID = -2;
    public static readonly long UPLOADED_ALBUM_ID = -1;
    private string _title = "";

    public long owner_id { get; set; }

    public long album_id
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

    public long id { get; set; }

    public int updated_time { get; set; }

    public int count { get; set; }

    public string photo_320 { get; set; }

    public string photo_160 { get; set; }

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

    public List<string> privacy { get; set; }

    public PrivacyInfo PrivacyInfo
    {
      get
      {
        return new PrivacyInfo(this.privacy);
      }
    }

    public VideoAlbum()
    {
      this.privacy = new List<string>();
    }
  }
}
