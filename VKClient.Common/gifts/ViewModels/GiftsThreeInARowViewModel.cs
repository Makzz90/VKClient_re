using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.CommonExtensions;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsThreeInARowViewModel
  {
      public GiftsSectionItemHeader Item1 { get; private set; }

      public GiftsSectionItemHeader Item2 { get; private set; }

      public GiftsSectionItemHeader Item3 { get; private set; }

    public Visibility Item1Visibility
    {
      get
      {
        return (this.Item1 != null).ToVisiblity();
      }
    }

    public Visibility Item2Visibility
    {
      get
      {
        return (this.Item2 != null).ToVisiblity();
      }
    }

    public Visibility Item3Visibility
    {
      get
      {
        return (this.Item3 != null).ToVisiblity();
      }
    }

    public GiftsThreeInARowViewModel(string categoryName, GiftsSectionItem section1, GiftsSectionItem section2 = null, GiftsSectionItem section3 = null, long userOrChatId = 0, bool isChat = false)
    {
      if (section1 != null)
        this.Item1 = new GiftsSectionItemHeader(section1, categoryName, userOrChatId, isChat);
      if (section2 != null)
        this.Item2 = new GiftsSectionItemHeader(section2, categoryName, userOrChatId, isChat);
      if (section3 == null)
        return;
      this.Item3 = new GiftsSectionItemHeader(section3, categoryName, userOrChatId, isChat);
    }
  }
}
