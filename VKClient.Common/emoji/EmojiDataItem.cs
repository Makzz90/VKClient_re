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
      char ch1 = BitConverter.ToChar(bytes, startIndex1);
      int startIndex2 = 4;
      char ch2 = BitConverter.ToChar(bytes, startIndex2);
      int startIndex3 = 2;
      char ch3 = BitConverter.ToChar(bytes, startIndex3);
      int startIndex4 = 0;
      char ch4 = BitConverter.ToChar(bytes, startIndex4);
      string str;
      if ((int) ch1 != 0)
        str = new string(new char[4]{ ch1, ch2, ch3, ch4 });
      else if ((int) ch3 != 0)
        str = new string(new char[2]{ ch3, ch4 });
      else
        str = ch4.ToString();
      return str;
    }

    public static Uri BuildUri(string string2)
    {
      string.Format("{0:X}", (short) string2[0]);
      switch (string2.Length)
      {
        case 2:
          return new Uri(string.Format("Resources/Emoji/{0}.png", string.Format("{0:X}", ((uint) (0 | (int) string2[0] << 16) | (uint) string2[1]))), UriKind.Relative);
        case 4:
          return new Uri(string.Format("Resources/Emoji/{0:X}{1:X}.png", ((uint) (0 | (int) string2[0] << 16) | (uint) string2[1]), (0UL | (ulong) ((uint) string2[2] << 16) | (ulong) string2[3])), UriKind.Relative);
        default:
          return new Uri(string.Format("Resources/Emoji/{0:X}.png", (short) string2[0]), UriKind.Relative);
      }
    }

    public static List<EmojiDataItem> GetAllDataItems()
    {
      List<EmojiDataItem> emojiDataItemList = new List<EmojiDataItem>();
      foreach (Smiles.SmileInformation smileInformation in Smiles.Collection)
      {
        if (smileInformation.IsAvailable)
        {
          EmojiDataItem emojiDataItem = new EmojiDataItem() { String = smileInformation.Symbol, Uri = EmojiDataItem.BuildUri(smileInformation.Symbol) };
          emojiDataItemList.Add(emojiDataItem);
        }
      }
      return emojiDataItemList;
    }
  }
}
