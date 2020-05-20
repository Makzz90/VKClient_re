using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class VideoCatalogItem : IBinarySerializable
  {
    public const string TYPE_VIDEO = "video";
    public const string TYPE_ALBUM = "album";

    public long id { get; set; }

    public long owner_id { get; set; }

    public string title { get; set; }

    public int count { get; set; }

    public string photo_320 { get; set; }

    public string photo_160 { get; set; }

    public int updated_time { get; set; }

    public int is_system { get; set; }

    public string type { get; set; }

    public int duration { get; set; }

    public string description { get; set; }

    public int date { get; set; }

    public int views { get; set; }

    public int comments { get; set; }

    public string photo_130 { get; set; }

    public string photo_800 { get; set; }

    public int live { get; set; }

    public int watched { get; set; }

    public VideoCatalogItem()
    {
    }

    public VideoCatalogItem(Video video)
    {
      this.id = video.id;
      this.owner_id = video.owner_id;
      this.title = video.title;
      this.photo_320 = video.photo_320;
      this.type = "video";
      this.duration = video.duration;
      this.description = video.description;
      this.date = video.date;
      this.comments = video.comments;
      this.photo_130 = video.photo_130;
      this.live = video.live;
      this.watched = video.watched;
      this.views = video.views;
    }

    public VideoCatalogItem(VideoAlbum album)
    {
      this.id = album.id;
      this.owner_id = album.owner_id;
      this.title = album.title;
      this.count = album.count;
      this.updated_time = album.updated_time;
      this.type = "album";
      this.photo_160 = album.photo_160;
      this.photo_320 = album.photo_320;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.Write(this.count);
      writer.WriteString(this.photo_320);
      writer.WriteString(this.photo_160);
      writer.Write(this.updated_time);
      writer.Write(this.is_system);
      writer.WriteString(this.type);
      writer.Write(this.duration);
      writer.WriteString(this.description);
      writer.Write(this.date);
      writer.Write(this.views);
      writer.Write(this.comments);
      writer.WriteString(this.photo_130);
      writer.WriteString(this.photo_800);
      writer.Write(this.live);
      writer.Write(this.watched);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.count = reader.ReadInt32();
      this.photo_320 = reader.ReadString();
      this.photo_160 = reader.ReadString();
      this.updated_time = reader.ReadInt32();
      this.is_system = reader.ReadInt32();
      this.type = reader.ReadString();
      this.duration = reader.ReadInt32();
      this.description = reader.ReadString();
      this.date = reader.ReadInt32();
      this.views = reader.ReadInt32();
      this.comments = reader.ReadInt32();
      this.photo_130 = reader.ReadString();
      this.photo_800 = reader.ReadString();
      this.live = reader.ReadInt32();
      this.watched = reader.ReadInt32();
    }
  }
}
