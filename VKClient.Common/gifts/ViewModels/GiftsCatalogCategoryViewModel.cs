using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsCatalogCategoryViewModel
  {
    private readonly GiftsSection _section;

    public long UserOrChatId { get; private set; }

    public bool IsChat { get; private set; }

    public string Title
    {
      get
      {
        GiftsSection section = this._section;
        if (section == null)
          return  null;
        return section.title;
      }
    }

    public string CategoryName
    {
      get
      {
        GiftsSection section = this._section;
        if (section == null)
          return  null;
        return section.name;
      }
    }

    public List<GiftsSectionItemHeader> Gifts { get; private set; }

    public GiftsCatalogCategoryViewModel(GiftsSection section, long userOrChatId = 0, bool isChat = false)
    {
      this._section = section;
      this.UserOrChatId = userOrChatId;
      this.IsChat = isChat;
      List<GiftsSectionItem> items = this._section.items;
      this.Gifts = items != null ? Enumerable.ToList<GiftsSectionItemHeader>(Enumerable.Select<GiftsSectionItem, GiftsSectionItemHeader>(Enumerable.Where<GiftsSectionItem>(items, (Func<GiftsSectionItem, bool>)(item => !item.IsDisabled)), (Func<GiftsSectionItem, GiftsSectionItemHeader>)(item => new GiftsSectionItemHeader(item, this.CategoryName, this.UserOrChatId, this.IsChat)))) : null;
    }
  }
}
