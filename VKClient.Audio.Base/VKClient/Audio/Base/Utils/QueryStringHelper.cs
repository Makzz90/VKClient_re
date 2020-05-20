using System.Collections.Generic;

namespace VKClient.Audio.Base.Utils
{
  public static class QueryStringHelper
  {
    public static Dictionary<string, string> GetParametersAsDict(string queryString)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      int num = queryString.IndexOf('?');
      if (num < 0 || num == queryString.Length - 1)
        return dictionary;
      string str1 = queryString.Substring(num + 1);
      char[] chArray1 = new char[1]{ '&' };
      foreach (string str2 in str1.Split(chArray1))
      {
        char[] chArray2 = new char[1]{ '=' };
        string[] strArray = str2.Split(chArray2);
        if (strArray.Length == 2)
          dictionary.Add(strArray[0], strArray[1]);
      }
      return dictionary;
    }
  }
}
