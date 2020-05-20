using System.Collections.Generic;
using VKClient.Photos.Library;

namespace VKClient.Photos
{
  public class PhotosToMoveInfo
  {
    public string albumId { get; set; }

    public string albumName { get; set; }

    public List<long> photos { get; set; }

    public PhotoAlbumViewModel.PhotoAlbumViewModelInput TargetAlbumInputData { get; set; }
  }
}
