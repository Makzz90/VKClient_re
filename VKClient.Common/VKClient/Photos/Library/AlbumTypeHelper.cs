namespace VKClient.Photos.Library
{
  public class AlbumTypeHelper
  {
    public static AlbumType GetAlbumType(long albumId)
    {
      if (albumId == -15L)
        return AlbumType.SavedPhotos;
      if (albumId == -7L)
        return AlbumType.WallPhotos;
      return albumId == -6L ? AlbumType.ProfilePhotos : AlbumType.NormalAlbum;
    }

    public static long GetAlbumIdLong(string albumId)
    {
      if (albumId == "0")
        return -6;
      if (albumId == "00")
        return -7;
      if (albumId == "000")
        return -15;
      long result;
      if (long.TryParse(albumId, out result))
        return result;
      return 0;
    }

    public static AlbumType GetAlbumType(string albumIdStr)
    {
      return AlbumTypeHelper.GetAlbumType(AlbumTypeHelper.GetAlbumIdLong(albumIdStr));
    }
  }
}
