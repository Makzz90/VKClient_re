using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class FriendsRecommendationsNewsItem : VirtualizableItemBase, IHaveUniqueKey, IHaveNewsfeedItemId
  {
    private readonly NewsItemDataWithUsersAndGroupsInfo _newsItem;

    public override double FixedHeight
    {
      get
      {
        return 386.0;
      }
    }

    public string NewsfeedItemId
    {
      get
      {
        return "friends_recomm";
      }
    }

    public FriendsRecommendationsNewsItem(NewsItemDataWithUsersAndGroupsInfo newsItem)
      : base(VKConstants.GenericWidth, new Thickness(), new Thickness())
    {
      this._newsItem = newsItem;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this.Children.Add((FrameworkElement) new FriendsRecommendationsNewsItemUC(this._newsItem));
      if (this.Parent == null)
        return;
      ViewBlockEvent message = new ViewBlockEvent();
      message.ItemType = "friends_recomm";
      int num = this.Parent.VirtualizableItems.IndexOf((IVirtualizable) this);
      message.Position = num;
      StatsEventsTracker.Instance.Handle(message);
    }

    public string GetKey()
    {
      return "FriendsRecommendationsInstance";
    }
  }
}
