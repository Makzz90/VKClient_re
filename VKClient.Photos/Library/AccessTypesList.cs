using System.Collections.Generic;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
  public static class AccessTypesList
  {
    private static AccessType _accessTypeAllUsers = new AccessType()
    {
      Id = 0,
      Name = PhotoResources.CreateAlbumUC_Access_AllUsers
    };
    public static List<AccessType> AccessTypes = new List<AccessType>()
    {
      AccessTypesList._accessTypeAllUsers,
      new AccessType()
      {
        Id = 2,
        Name = PhotoResources.CreateAlbumUC_Access_FriendsOfFriends
      },
      new AccessType()
      {
        Id = 1,
        Name = PhotoResources.CreateAlbumUC_Access_FriendsOnly
      },
      new AccessType()
      {
        Id = 3,
        Name = PhotoResources.CreateAlbumUC_Access_OnlyMe
      }
    };
    public static List<AccessType> AccessTypesGroupAlbums = new List<AccessType>()
    {
      AccessTypesList._accessTypeAllUsers,
      new AccessType()
      {
        Id = 0,
        Name = PhotoResources.CreateAlbumUC_Access_GroupMembers
      }
    };
  }
}
