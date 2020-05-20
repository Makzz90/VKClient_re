using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class AlbumsData
  {
    public int allPhotosCount
    {
      get
      {
        if (this.AllPhotos != null)
          return this.AllPhotos.count;
        return 0;
      }
    }

    public List<Photo> allPhotos
    {
      get
      {
        if (this.AllPhotos != null)
          return this.AllPhotos.items;
        return  null;
      }
    }

    public VKList<Photo> AllPhotos { get; set; }

    public int profilePhotosCount
    {
      get
      {
        if (this.ProfilePhotos != null)
          return this.ProfilePhotos.count;
        return 0;
      }
    }

    public List<Photo> profilePhotos
    {
      get
      {
        if (this.ProfilePhotos != null)
          return this.ProfilePhotos.items;
        return  null;
      }
    }

    public VKList<Photo> ProfilePhotos { get; set; }

    public int userPhotosCount
    {
      get
      {
        if (this.UserPhotos != null)
          return this.UserPhotos.count;
        return 0;
      }
    }

    public List<Photo> userPhotos
    {
      get
      {
        if (this.UserPhotos != null)
          return this.UserPhotos.items;
        return  null;
      }
    }

    public VKList<Photo> UserPhotos { get; set; }

    public int wallPhotosCount
    {
      get
      {
        if (this.WallPhotos != null)
          return this.WallPhotos.count;
        return 0;
      }
    }

    public List<Photo> wallPhotos
    {
      get
      {
        if (this.WallPhotos != null)
          return this.WallPhotos.items;
        return  null;
      }
    }

    public VKList<Photo> WallPhotos { get; set; }

    public int savedPhotosCount
    {
      get
      {
        if (this.SavedPhotos != null)
          return this.SavedPhotos.count;
        return 0;
      }
    }

    public List<Photo> savedPhotos
    {
      get
      {
        if (this.SavedPhotos != null)
          return this.SavedPhotos.items;
        return  null;
      }
    }

    public VKList<Photo> SavedPhotos { get; set; }

    public List<Album> albums
    {
      get
      {
        if (this.Albums != null)
          return this.Albums.items;
        return  null;
      }
    }

    public VKList<Album> Albums { get; set; }

    public List<Photo> covers { get; set; }

    public User userIns { get; set; }

    public User userGen { get; set; }

    public AlbumsData()
    {
      this.AllPhotos = new VKList<Photo>();
      this.ProfilePhotos = new VKList<Photo>();
      this.UserPhotos = new VKList<Photo>();
      this.WallPhotos = new VKList<Photo>();
      this.SavedPhotos = new VKList<Photo>();
      this.Albums = new VKList<Album>();
      this.covers = new List<Photo>();
    }
  }
}
