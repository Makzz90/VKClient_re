using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class ShowByDefaultTypesList
  {
    private static List<BGType> _showByDefaultTypesList;

    public static List<BGType> GetShowByDefaultTypesList()
    {
      if (ShowByDefaultTypesList._showByDefaultTypesList == null)
      {
        ShowByDefaultTypesList._showByDefaultTypesList = new List<BGType>();
        ShowByDefaultTypesList._showByDefaultTypesList.Add(new BGType()
        {
          id = 0,
          name = CommonResources.Settings_Account_AllPosts
        });
        ShowByDefaultTypesList._showByDefaultTypesList.Add(new BGType()
        {
          id = 1,
          name = CommonResources.Settings_Account_YourPosts
        });
      }
      return ShowByDefaultTypesList._showByDefaultTypesList;
    }
  }
}
