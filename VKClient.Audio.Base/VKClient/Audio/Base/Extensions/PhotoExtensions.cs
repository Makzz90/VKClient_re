using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Extensions
{
  public static class PhotoExtensions
  {
    public static bool IsProfilePhoto(this Photo photo, PostSource postSource)
    {
      if (photo != null && photo.aid == -6L && postSource != null)
        return postSource.data == "profile_photo";
      return false;
    }
  }
}
