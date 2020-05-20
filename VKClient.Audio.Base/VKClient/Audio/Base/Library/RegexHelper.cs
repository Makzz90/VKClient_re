using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VKClient.Audio.Base.Library
{
  public static class RegexHelper
  {
    public static string SubstituteInTopicCommentPostToExtended(string comment, long groupId, Dictionary<long, long> postToAuthodId)
    {
      if (string.IsNullOrWhiteSpace(comment))
        return "";
      return Regex.Replace(comment, "\\[post(\\d+)\\|(.*?)\\]", (MatchEvaluator) (m =>
      {
        long result = 0;
        long.TryParse(m.Groups[1].Value, out result);
        long num = 0;
        if (postToAuthodId.ContainsKey(result))
          num = postToAuthodId[result];
        string str1 = m.Groups[2].Value;
        string str2;
        if (num >= 0L)
          str2 = string.Format("[id{0}:bp-{1}_{2}|{3}]", num, groupId, result, str1);
        else
          str2 = string.Format("[club{0}:bp-{1}_{2}|{3}]", -num, groupId, result, str1);
        return str2;
      }));
    }

    public static string SubstituteInTopicCommentExtendedToPost(string comment)
    {
      return Regex.Replace(comment, "\\[(id|club)-?\\d+:bp-?\\d+_(\\d+)\\|(.+?)\\]", "[post$2|$3]");
    }
  }
}
