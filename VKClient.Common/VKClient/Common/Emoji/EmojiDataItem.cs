using System;
using System.Collections.Generic;

namespace VKClient.Common.Emoji
{
  public class EmojiDataItem
  {
    public string String { get; set; }

    public ulong Code { get; set; }

    public Uri Uri { get; set; }

    public EmojiDataItem()
    {
    }

    public EmojiDataItem(string string2, ulong code)
    {
      this.String = string2;
      this.Code = code;
    }

    public static string BuildString(ulong code)
    {
      byte[] bytes = BitConverter.GetBytes(code);
      int startIndex1 = 6;
      char char1 = BitConverter.ToChar(bytes, startIndex1);
      int startIndex2 = 4;
      char char2 = BitConverter.ToChar(bytes, startIndex2);
      int startIndex3 = 2;
      char char3 = BitConverter.ToChar(bytes, startIndex3);
      int startIndex4 = 0;
      char char4 = BitConverter.ToChar(bytes, startIndex4);
      string str;
      if ((int) char1 != 0)
        str = new string(new char[4]
        {
          char1,
          char2,
          char3,
          char4
        });
      else if ((int) char3 != 0)
        str = new string(new char[2]{ char3, char4 });
      else
        str = char4.ToString();
      return str;
    }

    public static Uri BuildUri(string string2)
    {
      string.Format("{0:X}", (object) (short) string2[0]);
      switch (string2.Length)
      {
        case 2:
          return new Uri(string.Format("Resources/Emoji/{0}.png", (object) string.Format("{0:X}", (object) ((uint) (0 | (int) string2[0] << 16) | (uint) string2[1]))), UriKind.Relative);
        case 4:
          return new Uri(string.Format("Resources/Emoji/{0:X}{1:X}.png", (object) ((uint) (0 | (int) string2[0] << 16) | (uint) string2[1]), (object) (0UL | (ulong) ((uint) string2[2] << 16) | (ulong) string2[3])), UriKind.Relative);
        default:
          return new Uri(string.Format("Resources/Emoji/{0:X}.png", (object) (short) string2[0]), UriKind.Relative);
      }
    }

    public static List<EmojiDataItem> GetAllDataItems()
    {
      List<EmojiDataItem> emojiDataItemList = new List<EmojiDataItem>();
      foreach (KeyValuePair<string, string> keyValuePair in Smiles.DictData)
      {
        EmojiDataItem emojiDataItem = new EmojiDataItem()
        {
          String = keyValuePair.Value,
          Uri = EmojiDataItem.BuildUri(keyValuePair.Value)
        };
        emojiDataItemList.Add(emojiDataItem);
      }
      return emojiDataItemList;
    }
  }
}
