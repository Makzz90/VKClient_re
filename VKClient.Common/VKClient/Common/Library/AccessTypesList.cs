using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class AccessTypesList
  {
    private static AccessType _accessTypeAllUsers = new AccessType()
    {
      Id = 0,
      Name = CommonResources.Access_AllUsers
    };
    public static List<AccessType> AccessTypes = new List<AccessType>()
    {
      AccessTypesList._accessTypeAllUsers,
      new AccessType()
      {
        Id = 2,
        Name = CommonResources.Access_FriendsOfFriends
      },
      new AccessType()
      {
        Id = 1,
        Name = CommonResources.Access_FriendsOnly
      },
      new AccessType()
      {
        Id = 3,
        Name = CommonResources.Access_OnlyMe
      }
    };
    public static List<AccessType> AccessTypesGroupAlbums = new List<AccessType>()
    {
      AccessTypesList._accessTypeAllUsers,
      new AccessType()
      {
        Id = 0,
        Name = CommonResources.Access_GroupMembers
      }
    };
  }
}
