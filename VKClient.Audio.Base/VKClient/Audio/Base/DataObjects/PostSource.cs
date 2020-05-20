namespace VKClient.Audio.Base.DataObjects
{
  public class PostSource
  {
    private const string TYPE_API = "api";
    private const string TYPE_MOBILE = "mvk";
    private const string PLATFORM_IOS = "ios";
    private const string PLATFORM_IPHONE = "iphone";
    private const string PLATFORM_IPAD = "ipad";
    private const string PLATFORM_ANDROID = "android";
    private const string PLATFORM_WINDOWS = "windows";
    private const string PLATFORM_WINPHONE = "wphone";
    private const string PLATFORM_SNAPSTER = "chronicle";
    private const string PLATFORM_INSTAGRAM = "instagram";
    private const string PLATFORM_PRISMA = "prisma";
    private const string PLATFORM_VINCI = "vinci";

    public string type { get; set; }

    public string data { get; set; }

    public string platform { get; set; }
      /*
    public PostSourcePlatform GetPlatform()
    {
      if (this.type == "api")
      {
        string platform = this.platform;
        // ISSUE: reference to a compiler-generated method
        uint stringHash = <PrivateImplementationDetails>.ComputeStringHash(platform);
        if (stringHash <= 2019413958U)
        {
          if (stringHash <= 1250711001U)
          {
            if ((int) stringHash != 255682822)
            {
              if ((int) stringHash != 1250711001 || !(platform == "ipad"))
                goto label_27;
            }
            else
            {
              if (platform == "vinci")
                return PostSourcePlatform.Vinci;
              goto label_27;
            }
          }
          else if ((int) stringHash != 1564925794)
          {
            if ((int) stringHash != 1701601664)
            {
              if ((int) stringHash != 2019413958 || !(platform == "wphone"))
                goto label_27;
              else
                goto label_22;
            }
            else if (!(platform == "iphone"))
              goto label_27;
          }
          else
          {
            if (platform == "chronicle")
              return PostSourcePlatform.Snapster;
            goto label_27;
          }
        }
        else if (stringHash <= 3308327609U)
        {
          if ((int) stringHash != -2132042778)
          {
            if ((int) stringHash == -986639687 && platform == "prisma")
              return PostSourcePlatform.Prisma;
            goto label_27;
          }
          else if (!(platform == "ios"))
            goto label_27;
        }
        else if ((int) stringHash != -711514390)
        {
          if ((int) stringHash != -393367173)
          {
            if ((int) stringHash == -98890964 && platform == "android")
              return PostSourcePlatform.Android;
            goto label_27;
          }
          else
          {
            if (platform == "instagram")
              return PostSourcePlatform.Instagram;
            goto label_27;
          }
        }
        else if (platform == "windows")
          goto label_22;
        else
          goto label_27;
        return PostSourcePlatform.IOS;
label_22:
        return PostSourcePlatform.Windows;
label_27:
        return PostSourcePlatform.ThirdParty;
      }
      return this.type == "mvk" ? PostSourcePlatform.Mobile : PostSourcePlatform.None;
    }*/
    public PostSourcePlatform GetPlatform()
    {
        if (this.type == "api")
        {
            if (this.platform == "ios" || this.platform == "iphone" || this.platform == "ipad")
                return PostSourcePlatform.IOS;
            else if (this.platform == "vinci")
                return PostSourcePlatform.Vinci;
            else if (this.platform == "android")
                return PostSourcePlatform.Android;
            else if (this.platform == "windows" || this.platform == "wphone")
                return PostSourcePlatform.Windows;
            else if (this.platform == "chronicle")
                return PostSourcePlatform.Snapster;
            else if (this.platform == "prisma")
                return PostSourcePlatform.Prisma;
            return this.platform == "instagram" ? PostSourcePlatform.Instagram : PostSourcePlatform.ThirdParty;
        }
        return this.type == "mvk" ? PostSourcePlatform.Mobile : PostSourcePlatform.None;
    }
  }
}
