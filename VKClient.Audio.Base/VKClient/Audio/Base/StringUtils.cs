using System;

namespace VKClient.Audio.Base
{
  public static class StringUtils
  {
    public static string MakeIntoOneLine(string str)
    {
      if (str == null)
        return "";
      str = str.Replace(Environment.NewLine, " ");
      str = str.Replace("\n", " ");
      return str;
    }
  }
}
