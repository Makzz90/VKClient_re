using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Framework
{
  public static class PostSourcePlatformEx
  {
    public static string GetIconUri(this PostSourcePlatform postSourcePlatform)
    {
      string str = "";
      switch (postSourcePlatform)
      {
        case PostSourcePlatform.Mobile:
          str = "PostAppMobile";
          break;
        case PostSourcePlatform.Windows:
          str = "PostAppWindows";
          break;
        case PostSourcePlatform.IOS:
          str = "PostAppIOS";
          break;
        case PostSourcePlatform.Android:
          str = "PostAppAndroid";
          break;
        case PostSourcePlatform.Snapster:
          str = "PostAppSnapster";
          break;
        case PostSourcePlatform.Instagram:
          str = "PostAppInstagram";
          break;
        case PostSourcePlatform.Prisma:
          str = "PostAppPrisma";
          break;
        case PostSourcePlatform.Vinci:
          str = "PostAppVinci";
          break;
      }
      if (!string.IsNullOrEmpty(str))
        return "/VKClient.Common;component/Resources/PostAppIcons/" + str + ".png";
      return "";
    }
  }
}
