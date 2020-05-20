namespace VKClient.Common.Localization
{
  public class CommonLocalizedStrings
  {
    private static CommonResources _localizedResources = new CommonResources();

    public CommonResources LocalizedResources
    {
      get
      {
        return CommonLocalizedStrings._localizedResources;
      }
    }
  }
}
