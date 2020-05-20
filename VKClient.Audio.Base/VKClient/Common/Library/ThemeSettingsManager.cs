using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public static class ThemeSettingsManager
  {
    private static ThemeSettings _settings;

    public static ThemeSettings GetThemeSettings()
    {
      if (ThemeSettingsManager._settings == null)
      {
        ThemeSettingsManager._settings = new ThemeSettings();
        CacheManager.TryDeserialize((IBinarySerializable) ThemeSettingsManager._settings, "ThemeSettings", CacheManager.DataType.CachedData);
      }
      return ThemeSettings.CreateNew(ThemeSettingsManager._settings);
    }

    public static void SetThemeSettings(ThemeSettings updatedSettings)
    {
      if (!CacheManager.TrySerialize((IBinarySerializable) updatedSettings, "ThemeSettings", false, CacheManager.DataType.CachedData))
        return;
      ThemeSettingsManager._settings = updatedSettings;
    }
  }
}
