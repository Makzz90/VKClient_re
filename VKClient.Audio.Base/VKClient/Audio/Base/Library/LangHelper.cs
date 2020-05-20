using VKClient.Audio.Base.Utils;

namespace VKClient.Audio.Base.Library
{
  public static class LangHelper
  {
    public static string GetLang()
    {
      CultureName current = CultureHelper.GetCurrent();
      string str = "";
      switch (current)
      {
        case CultureName.EN:
          str = "en";
          break;
        case CultureName.RU:
          str = "ru";
          break;
        case CultureName.UK:
          str = "ua";
          break;
        case CultureName.BE:
          str = "be";
          break;
        case CultureName.PT:
          str = "pt";
          break;
        case CultureName.KZ:
          str = "kz";
          break;
      }
      return str;
    }
  }
}
