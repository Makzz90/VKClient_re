using System.Text.RegularExpressions;

namespace VKClient.Audio.Base.Social
{
  public static class TextPreprocessor
  {
    private static readonly Regex _userOrGrouprMentionRegex = new Regex("\\[((id|club|public|event)\\d+.*?)\\|(.*?)\\]");

    public static string PreprocessText(string text)
    {
      if (text == null)
        return "";
      return TextPreprocessor._userOrGrouprMentionRegex.Replace(text, "$3").Trim();
    }
  }
}
