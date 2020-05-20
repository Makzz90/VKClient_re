using System.Text.RegularExpressions;

namespace VKClient.Common.Stickers.ViewModels
{
  public static class StickerReferrer
  {
    public const string STORE = "store";
    public const string KEYBOARD = "keyboard";
    public const string MESSAGE = "message";
    public const string LINK = "link";

    public static string FromKeyword(string keyword)
    {
      keyword = Regex.Replace(keyword, "\\s+", "");
      return string.Format("suggestion_{0}", (object) keyword);
    }
  }
}
