using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Audio.Base.Events;

namespace VKClient.Common.Framework
{
  public static class BrowserNavigationService
  {
    private static string _searchFeedPrefix = "vk.com/feed?section=search&q=";
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BrowserNavigationService), new PropertyMetadata(null, new PropertyChangedCallback(BrowserNavigationService.OnTextChanged)));
    public static readonly DependencyProperty DisableHyperlinksProperty = DependencyProperty.RegisterAttached("DisableHyperlinks", typeof(bool), typeof(BrowserNavigationService), new PropertyMetadata(false));
    public static readonly DependencyProperty TextIdProperty = DependencyProperty.RegisterAttached("TextId", typeof(string), typeof(BrowserNavigationService), new PropertyMetadata(""));
    public static readonly DependencyProperty HideHyperlinksForegroundProperty = DependencyProperty.RegisterAttached("HideHyperlinksForeground", typeof(bool), typeof(BrowserNavigationService), new PropertyMetadata(false));
    private static List<string> _flagsPrefixes = new List<string>() { "D83CDDE6", "D83CDDE7", "D83CDDE8", "D83CDDE9", "D83CDDEA", "D83CDDEB", "D83CDDEC", "D83CDDED", "D83CDDEE", "D83CDDEF", "D83CDDF0", "D83CDDF1", "D83CDDF2", "D83CDDF3", "D83CDDF4", "D83CDDF5", "D83CDDF6", "D83CDDF7", "D83CDDF8", "D83CDDF9", "D83CDDFA", "D83CDDFB", "D83CDDFC", "D83CDDFD", "D83CDDFE", "D83CDDFF" };
    private static List<string> _modificatableSmiles = new List<string>() { "261D", "270A", "270B", "270C", "270D", "D83CDF85", "D83CDFC3", "D83CDFC4", "D83CDFC7", "D83CDFCA", "D83DDC4A", "D83DDC4B", "D83DDC4C", "D83DDC4D", "D83DDC4E", "D83DDC4F", "D83DDC6E", "D83DDC7C", "D83DDC42", "D83DDC43", "D83DDC46", "D83DDC47", "D83DDC48", "D83DDC49", "D83DDC50", "D83DDC66", "D83DDC67", "D83DDC68", "D83DDC69", "D83DDC70", "D83DDC71", "D83DDC72", "D83DDC73", "D83DDC74", "D83DDC75", "D83DDC76", "D83DDC77", "D83DDC78", "D83DDC81", "D83DDC82", "D83DDC83", "D83DDC85", "D83DDC86", "D83DDC87", "D83DDCAA", "D83DDD90", "D83DDD95", "D83DDD96", "D83DDE4B", "D83DDE4C", "D83DDE4D", "D83DDE4E", "D83DDE4F", "D83DDE45", "D83DDE46", "D83DDE47", "D83DDEA3", "D83DDEB4", "D83DDEB5", "D83DDEB6", "D83DDEC0", "D83EDD18" };
    private static List<string> _smilesModificators = new List<string>() { "D83CDFFB", "D83CDFFC", "D83CDFFD", "D83CDFFF", "D83CDFFE" };
    private static List<string> _domainsList = new List<string>() { "AC", "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AN", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ", "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BM", "BN", "BO", "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN", "CO", "CR", "CU", "CV", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", "EU", "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MG", "MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RU", "RS", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS", "ST", "SU", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO", "TP", "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UK", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI", "VN", "VU", "WF", "WS", "YE", "YT", "YU", "ZA", "ZM", "ZW", "ARPA", "AERO", "ASIA", "BIZ", "CAT", "COM", "COOP", "INFO", "INT", "JOBS", "MEDIA", "MOBI", "MUSEUM", "NAME", "NET", "ORG", "POST", "PRO", "TEL", "TRAVEL", "XXX", "CLUB", "ACADEMY", "EDU", "GOV", "MIL", "LOCAL", "INTERNATIONAL", "XN--LGBBAT1AD8J", "XN--54B7FTA0CC", "XN--FIQS8S", "XN--FIQZ9S", "XN--WGBH1C", "XN--NODE", "XN--J6W193G", "XN--H2BRJ9C", "XN--MGBBH1A71E", "XN--FPCRJ9C3D", "XN--GECRJ9C", "XN--S9BRJ9C", "XN--XKC2DL3A5EE0H", "XN--45BRJ9C", "XN--MGBA3A4F16A", "XN--MGBAYH7GPA", "XN--80AO21A", "XN--MGBX4CD0AB", "XN--L1ACC", "XN--MGBC0A9AZCG", "XN--MGB9AWBF", "XN--MGBAI9AZGQP6J", "XN--YGBI2AMMX", "XN--WGBL6A", "XN--P1AI", "XN--MGBERP4A5D4AR", "XN--90A3AC", "XN--YFRO4I67O", "XN--CLCHC0EA0B2G2A9GCD", "XN--3E0B707E", "XN--FZC2C9E2C", "XN--XKC2AL3HYE2A", "XN--MGBTF8FL", "XN--KPRW13D", "XN--KPRY57D", "XN--O3CW4H", "XN--PGBS0DH", "XN--J1AMH", "XN--MGBAAM7A8H", "XN--MGB2DDES", "XN--80ASEHDB", "XN--80ASWG", "XN--OGBPF8FL", "рф", "РФ", "укр", "УКР", "онлайн", "ОНЛАЙН", "сайт", "САЙТ", "срб", "СРБ" };
    public static Regex Regex_Mention = new Regex("\\[(id|club)(\\d+)(?:\\:([a-z0-9_\\-]+))?\\|([^\\$]+?)\\]", RegexOptions.IgnoreCase);
    public static Regex Regex_DomainMention = new Regex("(\\*|@)((id|club|event|public)\\d+)\\s*\\((.+?)\\)", RegexOptions.IgnoreCase);
    private static Regex _regex_Uri = new Regex("(?<![A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј\\-_@])(https?:\\/\\/)?((?:[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј](?:[A-Za-z\\$0-9\\-_А-Яа-яёЁєЄҐґЇїІіЈј]*[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј])?\\.){1,5}[A-Za-z\\$рфуконлайстбРФУКОНЛАЙСТБ\\-\\d]{2,22}(?::\\d{2,5})?)((?:\\/(?:(?:\\&|\\!|,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]+|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\))*(?:,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]*[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј_@#%?+\\/\\$~=]|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\)))?)?)", RegexOptions.IgnoreCase);
    private static Regex _regex_Email = new Regex("([^#]|^)(?<![a-zA-Z\\-_\\.0-9])([a-zA-Z\\-_\\.0-9]+@[a-zA-Z\\-_0-9]+\\.[a-zA-Z\\-_\\.0-9]+[a-zA-Z\\-_0-9])", RegexOptions.IgnoreCase);
    private static Regex _regex_Domain = new Regex("((?:[a-z0-9_]*[a-z0-9])?(?:(?:\\.[a-z](?:[a-z0-9_]+[a-z0-9])?)*\\.[a-z][a-z0-9_]{2,40}[a-z0-9])?)", RegexOptions.IgnoreCase);
    private static Regex _regex_MatchedTag = new Regex("(?:)?(#[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’][a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]+|#[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]+[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’](?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*)(?:)?", RegexOptions.IgnoreCase);
    private const string _fatherSmile = "D83DDC68";
    private const string _motherSmile = "D83DDC69";
    private const string _daughterSmile = "D83DDC67";
    private const string _sonSmile = "D83DDC66";
    private const string _heartSmile = "2764";
    private const string _kissSmile = "D83DDC8B";

    public static Regex Regex_Tag = new Regex("(^|[\\s.,:'\";>\\)\\(]?)(" + BrowserNavigationService._regex_MatchedTag + ")(@" + BrowserNavigationService._regex_Domain + ")?(?=$|[\\s\\.,:'\"&;?<\\)\\(]?)", RegexOptions.IgnoreCase);

    public static List<string> DomainsList
    {
      get
      {
        return BrowserNavigationService._domainsList;
      }
    }

    public static string GetText(DependencyObject d)
    {
      return d.GetValue(BrowserNavigationService.TextProperty) as string;
    }

    public static void SetText(DependencyObject d, string value)
    {
      d.SetValue(BrowserNavigationService.TextProperty, value);
    }

    public static bool GetDisableHyperlinks(DependencyObject d)
    {
      return (bool) d.GetValue(BrowserNavigationService.DisableHyperlinksProperty);
    }

    public static void SetDisableHyperlinks(DependencyObject d, bool value)
    {
      d.SetValue(BrowserNavigationService.DisableHyperlinksProperty, value);
    }

    public static string GetTextId(DependencyObject d)
    {
      return (string) d.GetValue(BrowserNavigationService.TextIdProperty);
    }

    public static void SetTextId(DependencyObject d, string value)
    {
      d.SetValue(BrowserNavigationService.TextIdProperty, value);
    }

    private static bool GetHideHyperlinksForeground(DependencyObject d)
    {
      return (bool) d.GetValue(BrowserNavigationService.HideHyperlinksForegroundProperty);
    }

    public static void SetHideHyperlinksForeground(DependencyObject d, bool value)
    {
      d.SetValue(BrowserNavigationService.HideHyperlinksForegroundProperty, value);
    }

    public static Run GetRunWithStyle(string text, RichTextBox richTextBox)
    {
      Run run = new Run();
      FontFamily fontFamily = ((Control) richTextBox).FontFamily;
      ((TextElement) run).FontFamily = fontFamily;
      double fontSize = ((Control) richTextBox).FontSize;
      ((TextElement) run).FontSize = fontSize;
      Brush foreground = ((Control) richTextBox).Foreground;
      ((TextElement) run).Foreground = foreground;
      string str = text;
      run.Text = str;
      return run;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      RichTextBox text_block = d as RichTextBox;
      if (text_block == null)
        return;
      bool disableHyperlinks = BrowserNavigationService.GetDisableHyperlinks((DependencyObject) text_block);
      string textId = BrowserNavigationService.GetTextId((DependencyObject) text_block);
      bool hyperlinksForeground = BrowserNavigationService.GetHideHyperlinksForeground((DependencyObject) text_block);
      ((PresentationFrameworkCollection<Block>) text_block.Blocks).Clear();
      Paragraph par = new Paragraph();
      // ISSUE: explicit reference operation
      string newValue = (string) e.NewValue;
      if (string.IsNullOrEmpty(newValue))
        return;
      foreach (string str1 in BrowserNavigationService.ParseText(BrowserNavigationService.PreprocessTextForGroupBoardMentions(newValue)))
      {
        string[] innerSplit = str1.Split('\b');
        if (innerSplit.Length == 1)
          BrowserNavigationService.AddRawText(text_block, par, innerSplit[0]);
        else if (innerSplit.Length > 1)
        {
          if (disableHyperlinks)
          {
            BrowserNavigationService.AddRawText(text_block, par, innerSplit[1]);
          }
          else
          {
            if (innerSplit[0].Contains(BrowserNavigationService._searchFeedPrefix))
            {
              int num = innerSplit[0].IndexOf(BrowserNavigationService._searchFeedPrefix) + BrowserNavigationService._searchFeedPrefix.Length;
              string str2 = innerSplit[0].Substring(num);
              innerSplit[0] = innerSplit[0].Substring(0, num) + WebUtility.UrlEncode(str2);
            }
            Hyperlink hyperlink = HyperlinkHelper.GenerateHyperlink(innerSplit[1], innerSplit[0], (Action<Hyperlink, string>) ((h, navstr) =>
            {
              EventAggregator.Current.Publish(new HyperlinkClickedEvent()
              {
                HyperlinkOwnerId = textId
              });
              if (!string.IsNullOrEmpty(textId))
              {
                string str = navstr;
                if (innerSplit.Length > 2)
                  str = str.Replace("https://", "vkontakte://");
                EventAggregator.Current.Publish(new PostInteractionEvent()
                {
                  PostId = textId,
                  Action = PostInteractionAction.link_click,
                  Link = str
                });
              }
              BrowserNavigationService.NavigateOnHyperlink(navstr);
            }), ((Control) text_block).Foreground, hyperlinksForeground ? HyperlinkState.MatchForeground : HyperlinkState.Normal);
            ((PresentationFrameworkCollection<Inline>) par.Inlines).Add((Inline) hyperlink);
          }
        }
      }
      ((PresentationFrameworkCollection<Block>) text_block.Blocks).Add((Block) par);
    }

    private static void AddRawText(RichTextBox text_block, Paragraph par, string raw_text)
    {
      TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(raw_text);
      StringBuilder stringBuilder = new StringBuilder();
      bool flag1 = elementEnumerator.MoveNext();
      while (flag1)
      {
        string textElement1 = elementEnumerator.GetTextElement();
        string hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
        if (hexString1 == "")
        {
          flag1 = elementEnumerator.MoveNext();
        }
        else
        {
          bool flag2 = true;
          bool flag3 = BrowserNavigationService._flagsPrefixes.Contains(hexString1);
          int elementIndex = elementEnumerator.ElementIndex;
          if ((flag3 || BrowserNavigationService._modificatableSmiles.Contains(hexString1)) && elementEnumerator.MoveNext())
          {
            string textElement2 = elementEnumerator.GetTextElement();
            string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            if (hexString2 == "" && elementEnumerator.MoveNext())
            {
              textElement2 = elementEnumerator.GetTextElement();
              hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            }
            if (hexString2 != "" && (flag3 || BrowserNavigationService._smilesModificators.Contains(hexString2)))
            {
              flag2 = false;
              hexString1 += hexString2;
              textElement1 += textElement2;
            }
            else
            {
              elementEnumerator.Reset();
              elementEnumerator.MoveNext();
              while (elementEnumerator.ElementIndex != elementIndex)
                elementEnumerator.MoveNext();
            }
          }
          if (flag2)
            BrowserNavigationService.CheckRelationsSmiles(ref hexString1, ref elementEnumerator, ref textElement1);
          string name;
          Emoji.Dict.TryGetValue(hexString1, out name);
          if (name != null)
          {
            string text = stringBuilder.ToString();
            stringBuilder = stringBuilder.Clear();
            if (text != string.Empty)
              ((PresentationFrameworkCollection<Inline>) par.Inlines).Add((Inline) BrowserNavigationService.GetRunWithStyle(text, text_block));
            ((PresentationFrameworkCollection<Inline>) par.Inlines).Add((Inline) BrowserNavigationService.GetImage(name));
          }
          else
            stringBuilder = stringBuilder.Append(textElement1);
          flag1 = elementEnumerator.MoveNext();
        }
      }
      string text1 = stringBuilder.ToString();
      if (!(text1 != string.Empty))
        return;
      ((PresentationFrameworkCollection<Inline>) par.Inlines).Add((Inline) BrowserNavigationService.GetRunWithStyle(text1, text_block));
    }

    public static bool ContainsEmoji(string str)
    {
      if (string.IsNullOrEmpty(str))
        return false;
      TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(str);
      bool flag1 = elementEnumerator.MoveNext();
      while (flag1)
      {
        string textElement1 = elementEnumerator.GetTextElement();
        string hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
        if (hexString1 == "")
        {
          flag1 = elementEnumerator.MoveNext();
        }
        else
        {
          bool flag2 = true;
          bool flag3 = BrowserNavigationService._flagsPrefixes.Contains(hexString1);
          int elementIndex = elementEnumerator.ElementIndex;
          if ((flag3 || BrowserNavigationService._modificatableSmiles.Contains(hexString1)) && elementEnumerator.MoveNext())
          {
            string textElement2 = elementEnumerator.GetTextElement();
            string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            if (hexString2 == "" && elementEnumerator.MoveNext())
            {
              textElement2 = elementEnumerator.GetTextElement();
              hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            }
            if (hexString2 != "" && (flag3 || BrowserNavigationService._smilesModificators.Contains(hexString2)))
            {
              flag2 = false;
              hexString1 += hexString2;
              textElement1 += textElement2;
            }
            else
            {
              elementEnumerator.Reset();
              elementEnumerator.MoveNext();
              while (elementEnumerator.ElementIndex != elementIndex)
                elementEnumerator.MoveNext();
            }
          }
          if (flag2)
            BrowserNavigationService.CheckRelationsSmiles(ref hexString1, ref elementEnumerator, ref textElement1);
          if (Emoji.Dict.ContainsKey(hexString1))
            return true;
          flag1 = elementEnumerator.MoveNext();
        }
      }
      return false;
    }

    private static void CheckRelationsSmiles(ref string bytesStr, ref TextElementEnumerator textEnumerator, ref string text)
    {
      bool flag1 = true;
      int elementIndex1 = textEnumerator.ElementIndex;
      bool flag2 = bytesStr == "D83DDC68";
      if ((flag2 || bytesStr == "D83DDC69") && textEnumerator.MoveNext())
      {
        string textElement1 = textEnumerator.GetTextElement();
        string hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
        if (hexString1 == "" && textEnumerator.MoveNext())
        {
          textElement1 = textEnumerator.GetTextElement();
          hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
        }
        if (hexString1 == "2764" && textEnumerator.MoveNext())
        {
          string textElement2 = textEnumerator.GetTextElement();
          string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
          if (hexString2 == "" && textEnumerator.MoveNext())
          {
            textElement2 = textEnumerator.GetTextElement();
            hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
          }
          string str1 = "";
          string str2 = "";
          if (hexString2 == "D83DDC8B" && textEnumerator.MoveNext())
          {
            str1 = textElement2;
            str2 = hexString2;
            textElement2 = textEnumerator.GetTextElement();
            hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            if (hexString2 == "" && textEnumerator.MoveNext())
            {
              textElement2 = textEnumerator.GetTextElement();
              hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
            }
          }
          if (flag2 && hexString2 == "D83DDC68" || !flag2 && hexString2 == "D83DDC69")
          {
            flag1 = false;
            bytesStr = bytesStr + hexString1 + str2 + hexString2;
            text = text + textElement1 + str1 + textElement2;
          }
        }
        else if ((hexString1 == "D83DDC68" || hexString1 == "D83DDC69") && (!(bytesStr == "D83DDC69") || !(hexString1 == "D83DDC68")) && textEnumerator.MoveNext())
        {
          string textElement2 = textEnumerator.GetTextElement();
          string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
          if (hexString2 == "" && textEnumerator.MoveNext())
          {
            textElement2 = textEnumerator.GetTextElement();
            hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
          }
          if (hexString2 == "D83DDC67" || hexString2 == "D83DDC66")
          {
            bool flag3 = false;
            int elementIndex2 = textEnumerator.ElementIndex;
            flag1 = false;
            if (textEnumerator.MoveNext())
            {
              string textElement3 = textEnumerator.GetTextElement();
              string hexString3 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement3));
              if (hexString3 == "" && textEnumerator.MoveNext())
              {
                textElement3 = textEnumerator.GetTextElement();
                hexString3 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement3));
              }
              if ((hexString3 == "D83DDC67" || hexString3 == "D83DDC66") && (!(hexString2 == "D83DDC66") || !(hexString3 == "D83DDC67")))
              {
                flag3 = true;
                bytesStr = bytesStr + hexString1 + hexString2 + hexString3;
                text = text + textElement1 + textElement2 + textElement3;
              }
              else
              {
                textEnumerator.Reset();
                textEnumerator.MoveNext();
                while (textEnumerator.ElementIndex != elementIndex2)
                  textEnumerator.MoveNext();
              }
            }
            if (!flag3)
            {
              bytesStr = bytesStr + hexString1 + hexString2;
              text = text + textElement1 + textElement2;
            }
          }
        }
      }
      if (!(textEnumerator.ElementIndex != elementIndex1 & flag1))
        return;
      textEnumerator.Reset();
      textEnumerator.MoveNext();
      while (textEnumerator.ElementIndex != elementIndex1)
        textEnumerator.MoveNext();
    }

    private static string ConvertToHexString(byte[] bytes)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < bytes.Length; ++index)
        stringBuilder = stringBuilder.Append(Convert.ToString(bytes[index], 16).PadLeft(2, '0'));
      return stringBuilder.ToString().ToUpperInvariant().Replace("FE0F", "").Replace("200D", "");
    }

    private static InlineUIContainer GetImage(string name)
    {
      Image image1 = new Image();
      image1.Source = ((ImageSource) new BitmapImage(new Uri(string.Format("/Resources/Emoji/{0}.png", name), UriKind.RelativeOrAbsolute)));
      ((FrameworkElement) image1).Height = 25.0;
      ((FrameworkElement) image1).Width = 25.0;
      if (name == "00A9" || name == "00AE")
        ((FrameworkElement) image1).Margin=(new Thickness(0.0, 4.0, -4.0, -9.0));
      else
        ((FrameworkElement) image1).Margin=(new Thickness(0.0, 0.0, 4.0, -5.0));
      InlineUIContainer inlineUiContainer = new InlineUIContainer();
      Image image2 = image1;
      inlineUiContainer.Child = ((UIElement) image2);
      return inlineUiContainer;
    }

    private static void NavigateOnHyperlink(string navstr)
    {
      if (string.IsNullOrEmpty(navstr))
        return;
      if (!navstr.Contains("@") || navstr.Contains("#"))
      {
        Navigator.Current.NavigateToWebUri(navstr, false, false);
      }
      else
      {
        EmailComposeTask emailComposeTask = new EmailComposeTask();
        if (navstr.StartsWith("http://"))
          navstr = navstr.Remove(0, 7);
        string str = navstr;
        emailComposeTask.To = str;
        emailComposeTask.Show();
      }
    }

    public static string PreprocessTextForGroupBoardMentions(string s)
    {
      s = Regex.Replace(s, "\\[(id|club)(\\d+):bp\\-(\\d+)_(\\d+)\\|([^\\]]+)\\]", "[$1$2|$5]");
      return s;
    }

    public static string CutTextGently(string text, int preferredMaxLength)
    {
      if (text == null || text.Length <= preferredMaxLength)
        return text ?? "";
      MatchCollection matchCollection1 = BrowserNavigationService._regex_Uri.Matches(text);
      MatchCollection matchCollection2 = BrowserNavigationService._regex_Email.Matches(text);
      MatchCollection matchCollection3 = BrowserNavigationService.Regex_DomainMention.Matches(text);
      MatchCollection matchCollection4 = BrowserNavigationService.Regex_Mention.Matches(text);
      MatchCollection matchCollection5 = BrowserNavigationService.Regex_Tag.Matches(text);
      int val1 = preferredMaxLength - 1;
      int num1 = text.IndexOf(' ', val1 + 1);
      if (num1 > 0 && num1 < preferredMaxLength + 30)
        val1 = num1;
      int num2 = text.IndexOfAny(new char[3]{ '.', '!', '?' }, val1 + 1);
      if (num2 > 0 && num2 < preferredMaxLength + 200)
        val1 = num2;
      if (text.Length - val1 < 20)
        return text;
      foreach (Match match in matchCollection1)
      {
        string str = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty;
        if (BrowserNavigationService.DomainsList.Contains((str.Contains(".") ? str.Remove(0, str.LastIndexOf(".") + 1) : string.Empty).ToUpper()) && match.Index <= val1 && match.Index + match.Length - 1 >= val1)
          val1 = Math.Max(val1, match.Index + match.Length - 1);
      }
      foreach (Match match in matchCollection2)
      {
        if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
          val1 = Math.Max(val1, match.Index + match.Length - 1);
      }
      foreach (Match match in matchCollection3)
      {
        if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
          val1 = Math.Max(val1, match.Index + match.Length - 1);
      }
      foreach (Match match in matchCollection4)
      {
        if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
          val1 = Math.Max(val1, match.Index + match.Length - 1);
      }
      foreach (Match match in matchCollection5)
      {
        if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
          val1 = Math.Max(val1, match.Index + match.Length - 1);
      }
      return text.Substring(0, val1 + 1);
    }

    public static List<string> ParseText(string text)
    {
      text = text.Replace("\n", " \n ");
      text = BrowserNavigationService._regex_Uri.Replace(text, (MatchEvaluator) (m =>
      {
        string str1 = m.Groups.Count > 2 ? m.Groups[2].Value : string.Empty;
        if (!BrowserNavigationService.DomainsList.Contains((str1.Contains(".") ? str1.Remove(0, str1.LastIndexOf(".") + 1) : string.Empty).ToUpper()) && (m.Groups.Count <= 1 || m.Groups[1].Value.ToLower() != "http://" && m.Groups[1].Value.ToLower() != "https://"))
          return WebUtility.UrlDecode(m.Value);
        string str2 = m.Groups.Count > 1 ? m.Groups[1].Value : "http://";
        if (str2 == string.Empty)
          str2 = "http://";
        string str3 = m.Groups.Count > 2 ? m.Groups[2].Value : string.Empty;
        if (m.Groups.Count > 3)
          str3 += m.Groups[3].Value;
        string str4 = WebUtility.UrlDecode(m.Value);
        if (str4.Length > 38)
          str4 = str4.Substring(0, 35) + "...";
        return string.Format("\a{0}\b{1}\a", (str2 + str3), str4).Replace("#", "\0");
      }));
      text = text.ReplaceByRegex(BrowserNavigationService._regex_Email, "\amailto:$2\b$2\a").ReplaceByRegex(BrowserNavigationService.Regex_DomainMention, "[$2|$4]").ReplaceByRegex(BrowserNavigationService.Regex_Mention, "\ahttps://vk.com/$1$2$3\b$4\b\a").ReplaceByRegex(BrowserNavigationService.Regex_Tag, string.Format("$1\ahttps://{0}$3$4\b$3$4\a", BrowserNavigationService._searchFeedPrefix)).Replace("\n ", "\n").Replace(" \n", "\n").Replace("\0", "#");
      if (text.StartsWith(" "))
        text = text.Remove(0, 1);
      return (List<string>) Enumerable.ToList<string>(text.Split('\a'));
    }
  }
}
