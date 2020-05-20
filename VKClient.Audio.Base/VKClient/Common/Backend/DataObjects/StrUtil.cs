using VKClient.Audio.Base.Utils;

namespace VKClient.Common.Backend.DataObjects
{
  internal static class StrUtil
  {
    public static string ForUI(this string backendTextString)
    {
      return ExtensionsBase.ForUI(backendTextString);
    }
  }
}
