using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class AudioAlbumCreatedEditedDeleted
  {
    public bool? Created { get; set; }

    public AudioAlbum Album { get; set; }
  }
}
