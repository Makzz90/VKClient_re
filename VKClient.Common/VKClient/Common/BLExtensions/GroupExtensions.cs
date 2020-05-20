using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.BLExtensions
{
  public static class GroupExtensions
  {
    public static bool IsModeratorOrHigher(this Group group)
    {
      if (group == null)
        return false;
      return group.admin_level >= 1;
    }

    public static bool IsEditorOrHigher(this Group group)
    {
      if (group == null)
        return false;
      return group.admin_level >= 2;
    }

    public static bool IsAdminOrHigher(this Group group)
    {
      if (group == null)
        return false;
      return group.admin_level >= 3;
    }
  }
}
