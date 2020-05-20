using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class PhotosListWithCount
  {
    public int photosCount { get; set; }

    public List<Photo> response { get; set; }

    public Album album { get; set; }

    public PhotosListWithCount()
    {
      this.response = new List<Photo>();
    }
  }
}
