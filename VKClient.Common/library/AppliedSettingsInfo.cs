namespace VKClient.Common.Library
{
  public class AppliedSettingsInfo
  {
    private static int _appliedBGSetting;
    private static int _appliedLanguageSetting;

    public static int AppliedBGSetting
    {
      get
      {
        return AppliedSettingsInfo._appliedBGSetting;
      }
      set
      {
        AppliedSettingsInfo._appliedBGSetting = value;
      }
    }

    public static int AppliedLanguageSetting
    {
      get
      {
        return AppliedSettingsInfo._appliedLanguageSetting;
      }
      set
      {
        AppliedSettingsInfo._appliedLanguageSetting = value;
      }
    }
  }
}
