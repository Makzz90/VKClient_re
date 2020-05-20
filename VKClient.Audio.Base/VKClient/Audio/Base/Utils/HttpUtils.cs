using System.Collections.Generic;

namespace VKClient.Audio.Base.Utils
{
  public static class HttpUtils
  {
    public static Dictionary<string, string> ParseQueryString(string uri)
    {
      string[] strArray1 = uri.Substring(uri.LastIndexOf('?') == -1 ? 0 : uri.LastIndexOf('?') + 1).Split('&');
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      foreach (string str in strArray1)
      {
        char[] chArray = new char[1]{ '=' };
        string[] strArray2 = str.Split(chArray);
        if (strArray2.Length > 1)
          dictionary.Add(strArray2[0], strArray2[1]);
      }
      return dictionary;
    }
  }
}
