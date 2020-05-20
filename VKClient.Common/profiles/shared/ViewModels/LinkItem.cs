using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class LinkItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public string GroupImage { get; private set; }

    public double TitleMaxWidth { get; private set; }

    public LinkItem(GroupLink link)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = link.name.ForUI();
      this.Data = link.desc.ForUI();
      this.GroupImage = link.photo_100;
      this.NavigationAction = (Action) (() => Navigator.Current.NavigateToWebUri(link.url, false, false));
      if (!string.IsNullOrEmpty(this.GroupImage))
        this.TitleMaxWidth = 370.0;
      else
        this.TitleMaxWidth = 448.0;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }

    public static IEnumerable<LinkItem> GetLinkItems(List<GroupLink> links)
    {
      List<LinkItem> linkItemList = new List<LinkItem>();
      if (links.IsNullOrEmpty())
        return (IEnumerable<LinkItem>) linkItemList;
      linkItemList.AddRange((IEnumerable<LinkItem>)Enumerable.Select<GroupLink, LinkItem>(links, (Func<GroupLink, LinkItem>)(link => new LinkItem(link))));
      return (IEnumerable<LinkItem>) linkItemList;
    }
  }
}
