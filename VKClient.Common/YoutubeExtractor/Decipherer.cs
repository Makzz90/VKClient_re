using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
  internal static class Decipherer
  {
    public static string DecipherWithVersion(string cipher, string cipherVersion)
    {
      string input = HttpHelper.DownloadString(string.Format("http://s.ytimg.com/yts/jsbin/player-{0}.js", cipherVersion));
      string pattern1 = "\\.sig\\s*\\|\\|([a-zA-Z0-9\\$]+)\\(";
      string str1 = Regex.Match(input, pattern1).Groups[1].Value;
      if (str1.Contains("$"))
        str1 = "\\" + str1;
      string pattern2 = str1 + "=function\\(\\w+\\)\\{.*?\\},";
      string[] strArray = Regex.Match(input, pattern2, RegexOptions.Singleline).Value.Split(';');
      string str2 = "";
      string str3 = "";
      string str4 = "";
      string str5 = "";
      IEnumerator<string> enumerator1 = ((IEnumerable<string>) Enumerable.Take<string>(Enumerable.Skip<string>(strArray, 1), strArray.Length - 2)).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          string current = enumerator1.Current;
          if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3))
          {
            if (!string.IsNullOrEmpty(str4))
              break;
          }
          string functionFromLine = Decipherer.GetFunctionFromLine(current);
          string pattern3 = string.Format("{0}:\\bfunction\\b\\(\\w+\\)", functionFromLine);
          string pattern4 = string.Format("{0}:\\bfunction\\b\\([a],b\\).(\\breturn\\b)?.?\\w+\\.", functionFromLine);
          string pattern5 = string.Format("{0}:\\bfunction\\b\\(\\w+\\,\\w\\).\\bvar\\b.\\bc=a\\b", functionFromLine);
          if (Regex.Match(input, pattern3).Success)
            str2 = functionFromLine;
          if (Regex.Match(input, pattern4).Success)
            str3 = functionFromLine;
          if (Regex.Match(input, pattern5).Success)
            str4 = functionFromLine;
        }
      }
      finally
      {
        if (enumerator1 != null)
          ((IDisposable) enumerator1).Dispose();
      }
      IEnumerator<string> enumerator2 = ((IEnumerable<string>) Enumerable.Take<string>(Enumerable.Skip<string>(strArray, 1), strArray.Length - 2)).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator2).MoveNext())
        {
          string current = enumerator2.Current;
          string functionFromLine = Decipherer.GetFunctionFromLine(current);
          string pattern3 = "\\(\\w+,(?<index>\\d+)\\)";
          Match match1;
          if ((match1 = Regex.Match(current, pattern3)).Success && functionFromLine == str4)
            str5 = str5 + "w" + match1.Groups["index"].Value + " ";
          string pattern4 = "\\(\\w+,(?<index>\\d+)\\)";
          Match match2;
          if ((match2 = Regex.Match(current, pattern4)).Success && functionFromLine == str3)
            str5 = str5 + "s" + match2.Groups["index"].Value + " ";
          if (functionFromLine == str2)
            str5 += "r ";
        }
      }
      finally
      {
        if (enumerator2 != null)
          ((IDisposable) enumerator2).Dispose();
      }
      string operations = str5.Trim();
      return Decipherer.DecipherWithOperations(cipher, operations);
    }

    private static string ApplyOperation(string cipher, string op)
    {
      switch (op[0])
      {
        case 'r':
          return new string((char[]) Enumerable.ToArray<char>(Enumerable.Reverse<char>(cipher.ToCharArray())));
        case 's':
          int opIndex1 = Decipherer.GetOpIndex(op);
          return cipher.Substring(opIndex1);
        case 'w':
          int opIndex2 = Decipherer.GetOpIndex(op);
          return Decipherer.SwapFirstChar(cipher, opIndex2);
        default:
          throw new NotImplementedException("Couldn't find cipher operation.");
      }
    }

    private static string DecipherWithOperations(string cipher, string operations)
    {
        return Enumerable.Aggregate<string, string>(operations.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries), cipher, new Func<string, string, string>(Decipherer.ApplyOperation));
    }

    private static string GetFunctionFromLine(string currentLine)
    {
      return new Regex("\\w+\\.(?<functionID>\\w+)\\(").Match(currentLine).Groups["functionID"].Value;
    }

    private static int GetOpIndex(string op)
    {
      return int.Parse(new Regex(".(\\d+)").Match(op).Result("$1"));
    }

    private static string SwapFirstChar(string cipher, int index)
    {
      StringBuilder stringBuilder = new StringBuilder(cipher);
      int index1 = 0;
      int num1 = (int) cipher[index];
      stringBuilder[index1] = (char) num1;
      int index2 = index;
      int num2 = (int) cipher[0];
      stringBuilder[index2] = (char) num2;
      return (stringBuilder).ToString();
    }
  }
}
