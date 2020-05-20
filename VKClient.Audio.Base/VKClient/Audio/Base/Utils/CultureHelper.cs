using System.Threading;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Utils
{
  public static class CultureHelper
  {
    public static CultureName GetCurrent()
    {
      string languageCultureString = ThemeSettingsManager.GetThemeSettings().LanguageCultureString;
      if (languageCultureString == "")
        languageCultureString = Thread.CurrentThread.CurrentUICulture.ToString();
      if (languageCultureString.StartsWith("en"))
        return CultureName.EN;
      if (languageCultureString.StartsWith("ru"))
        return CultureName.RU;
      if (languageCultureString.StartsWith("uk"))
        return CultureName.UK;
      if (languageCultureString.StartsWith("be"))
        return CultureName.BE;
      if (languageCultureString.StartsWith("pt"))
        return CultureName.PT;
      return languageCultureString.StartsWith("kk") ? CultureName.KZ : CultureName.NONE;
    }
  }
}
