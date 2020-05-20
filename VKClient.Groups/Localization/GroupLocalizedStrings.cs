namespace VKClient.Groups.Localization
{
  public class GroupLocalizedStrings
  {
    private static GroupResources _localizedResources = new GroupResources();

    public GroupResources LocalizedResources
    {
      get
      {
        return GroupLocalizedStrings._localizedResources;
      }
    }
  }
}
