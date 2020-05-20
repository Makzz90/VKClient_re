using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Utils
{
  public static class SexExtensions
  {
    public static string GetSexStr(this Sex sex)
    {
      switch (sex)
      {
        case Sex.NA:
          return CommonResources.Sex_Any;
        case Sex.Female:
          return CommonResources.Sex_Female;
        case Sex.Male:
          return CommonResources.Sex_Male;
        default:
          return "";
      }
    }
  }
}
