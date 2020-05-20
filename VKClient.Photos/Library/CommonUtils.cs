using VKClient.Common.Utils;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
  public static class CommonUtils
  {
    public static string FormatPhotosCountString(int PhotosCount)
    {
      if (PhotosCount == 0)
        return PhotoResources.PhotoAlbumPage_NoPhotos;
      return UIStringFormatterHelper.FormatNumberOfSomething(PhotosCount, PhotoResources.AlbumsPhotosCountOneFrm, PhotoResources.AlbumsPhotosCountTwoFrm, PhotoResources.AlbumsPhotosCountFiveFrm, true,  null, false);
    }
  }
}
