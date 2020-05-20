using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.Social
{
  public class PhotosList : IBinarySerializable
  {
    public static readonly string SaveKey = "PhotosListSocialIntegration";
    private List<Photo> _photos = new List<Photo>();

    public List<Photo> Photos
    {
      get
      {
        return this._photos;
      }
      set
      {
        this._photos = value;
        this.CleanupPhotos();
      }
    }

    private void CleanupPhotos()
    {
      if (this._photos == null)
        return;
      foreach (Photo photo in this._photos)
        photo.text = "";
    }

    public void Write(BinaryWriter writer)
    {
      writer.WriteString(this.Photos.Count.ToString());
      foreach (Photo photo in this.Photos)
      {
        BinaryWriter binaryWriter1 = writer;
        long num = photo.pid;
        string str1 = num.ToString();
        binaryWriter1.Write(str1);
        BinaryWriter binaryWriter2 = writer;
        num = photo.owner_id;
        string str2 = num.ToString();
        binaryWriter2.Write(str2);
        writer.WriteString(photo.access_key);
        if (!string.IsNullOrEmpty(photo.src_xbig))
          writer.WriteString(photo.src_xbig);
        else
          writer.WriteString(photo.src_big);
        BinaryWriter binaryWriter3 = writer;
        num = photo.aid;
        string str3 = num.ToString();
        binaryWriter3.Write(str3);
        writer.Write(photo.can_comment.ToString());
      }
    }

    public void Read(BinaryReader reader)
    {
      int num = int.Parse(reader.ReadString());
      this.Photos = new List<Photo>();
      for (int index = 0; index < num; ++index)
        this.Photos.Add(new Photo()
        {
          pid = long.Parse(reader.ReadString()),
          owner_id = long.Parse(reader.ReadString()),
          access_key = reader.ReadString(),
          src_xbig = reader.ReadString(),
          aid = long.Parse(reader.ReadString()),
          can_comment = int.Parse(reader.ReadString())
        });
    }
  }
}
