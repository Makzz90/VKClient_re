using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
  public class VideosNewsItem : VirtualizableItemBase, ICanHideFromNewsfeed
  {
    private const double MARGIN_BETWEEN_ELEMENTS = 12.0;
    public const double DIVIDER_HEIGHT = 16.0;
    private readonly NewsItemDataWithUsersAndGroupsInfo _newsItemWithInfo;
    private readonly Action<NewsFeedIgnoreItemData> _ignoreNewsfeedItemCallback;
    private readonly Group _fromGroup;
    private readonly User _fromUser;
    private ThumbsItem _thumbsItem;
    private UserOrGroupHeaderItem _header;
    private double _height;
    private VKList<VKClient.Common.Backend.DataObjects.Video> _videos;

    public Action<long, User, Group> HideSourceItemsCallback { get; set; }

    public LikesAndCommentsItem LikesAndCommentsItem { get; private set; }

    public long OwnerId
    {
      get
      {
        return this._newsItemWithInfo.NewsItem.source_id;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public VideosNewsItem(double width, Thickness margin, NewsItemDataWithUsersAndGroupsInfo newsItemWithInfo, Action<long, User, Group> hideFromNewsCallback, Action<NewsFeedIgnoreItemData> ignoreNewsfeedItemCallback)
      : base(width, margin, new Thickness())
    {
      this._newsItemWithInfo = newsItemWithInfo;
      this._ignoreNewsfeedItemCallback = ignoreNewsfeedItemCallback;
      this.HideSourceItemsCallback = hideFromNewsCallback;
      List<User> profiles = newsItemWithInfo.Profiles;
      List<Group> groups = newsItemWithInfo.Groups;
      if (this._newsItemWithInfo.NewsItem.source_id < 0L)
        this._fromGroup = groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this.OwnerId)) ?? GroupsService.Current.GetCachedGroup(-this.OwnerId) ?? new Group();
      else
        this._fromUser = profiles.FirstOrDefault<User>((Func<User, bool>) (p => p.uid == this.OwnerId)) ?? new User();
      this.GenerateLayout();
    }

    private void CreateMenu()
    {
      this._header.SetMenu(this.GenerateMenuItems());
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this.UnHookEvents();
      this.ReleaseMenu();
    }

    private void ReleaseMenu()
    {
      this._header.ResetMenu();
    }

    private List<MenuItem> GenerateMenuItems()
    {
      List<MenuItem> menuItemList = new List<MenuItem>();
      if (this.HideSourceItemsCallback != null)
      {
        MenuItem menuItem1 = new MenuItem();
        string hideFromNews = CommonResources.HideFromNews;
        menuItem1.Header = (object) hideFromNews;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.hideFromNewsMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (this._ignoreNewsfeedItemCallback != null && this.GetIgnoreItemData() != null)
      {
        MenuItem menuItem1 = new MenuItem();
        string lowerInvariant = CommonResources.HideThisPost.ToLowerInvariant();
        menuItem1.Header = (object) lowerInvariant;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.HidePostItem_OnClick);
        menuItemList.Add(menuItem2);
      }
      return menuItemList;
    }

    public NewsFeedIgnoreItemData GetIgnoreItemData()
    {
      NewsItem newsItem = this._newsItemWithInfo.NewsItem;
      List<VKClient.Common.Backend.DataObjects.Video> videoList1;
      if (newsItem == null)
      {
          videoList1 = (List<VKClient.Common.Backend.DataObjects.Video>)null;
      }
      else
      {
          VKList<VKClient.Common.Backend.DataObjects.Video> video = newsItem.video;
        videoList1 = video != null ? video.items : (List<VKClient.Common.Backend.DataObjects.Video>)null;
      }
      List<VKClient.Common.Backend.DataObjects.Video> videoList2 = videoList1;
      if (videoList2 == null || videoList2.Count <= 0)
        return (NewsFeedIgnoreItemData) null;
      VKClient.Common.Backend.DataObjects.Video video1 = videoList2[0];
      long itemId = videoList2.Count == 1 ? video1.id : BaseFormatterHelper.GetLastMidnight((long) newsItem.date, false);
      return new NewsFeedIgnoreItemData("video", video1.owner_id, itemId);
    }

    private void HidePostItem_OnClick(object sender, RoutedEventArgs e)
    {
      this._ignoreNewsfeedItemCallback(this.GetIgnoreItemData());
    }

    private void hideFromNewsMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.HideSourceItemsCallback(this.OwnerId, this._fromUser, this._fromGroup);
    }

    private void GenerateLayout()
    {
      Thickness margin = new Thickness();
      Action moreOptionsTapCallback = new Action(this.OnMoreOptionsTap);
      NewsItem newsItem1 = this._newsItemWithInfo.NewsItem;
      this._videos = (newsItem1 != null ? newsItem1.video : (VKList<VKClient.Common.Backend.DataObjects.Video>)null) ?? new VKList<VKClient.Common.Backend.DataObjects.Video>();
      string extraText = UIStringFormatterHelper.FormatNumberOfSomething(this._videos.count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true, null, false);
      this._header = new UserOrGroupHeaderItem(this.Width, margin, this.OwnerId < 0L, this._newsItemWithInfo.NewsItem.date, this._fromUser, this._fromGroup, extraText, PostIconType.None, PostSourcePlatform.None, moreOptionsTapCallback, new Action(this.onNavigatedToUserOrGroup), "");
      this.VirtualizableChildren.Add((IVirtualizable) this._header);
      margin.Top += 12.0 + this._header.FixedHeight + 2.0;
      List<Attachment> list = this._videos.items.Take<VKClient.Common.Backend.DataObjects.Video>(5).Select<VKClient.Common.Backend.DataObjects.Video, Attachment>((Func<VKClient.Common.Backend.DataObjects.Video, Attachment>)(v => new Attachment()
      {
        type = "video",
        video = v
      })).ToList<Attachment>();
      double desiredHeight = list.Count != 1 ? (list.Count < 2 || list.Count > 4 ? 580.0 : 430.0) : 280.0;
      NewsItem newsItem2 = this._newsItemWithInfo.NewsItem;
      string itemId = newsItem2.source_id != 0L ? newsItem2.source_id.ToString() : "";
      this._thumbsItem = new ThumbsItem(this.Width, new Thickness(0.0, margin.Top, 0.0, 0.0), list, false, itemId, false, false, false, 0.0, desiredHeight);
      this.VirtualizableChildren.Add((IVirtualizable) this._thumbsItem);
      margin.Top += this._thumbsItem.FixedHeight;
      if (this._videos.items.Count > 5)
      {
        UCItem ucItem = new UCItem(this.Width, margin, (Func<UserControlVirtualizable>) (() =>
        {
          return (UserControlVirtualizable) new ShowMoreCommentsUC()
          {
            Height = 64.0,
            Text = CommonResources.ShowAll,
            OnClickAction = new Action(this.HandleShowAll)
          };
        }), (Func<double>) (() => 64.0), (Action<UserControlVirtualizable>) null, 0.0, false);
        this.VirtualizableChildren.Add((IVirtualizable) ucItem);
        margin.Top += ucItem.FixedHeight;
      }
      else if (this._videos.items.Count == 1)
      {
        this.LikesAndCommentsItem = new LikesAndCommentsItem(this.Width, margin, this._videos.items[0], new Action(this.CommentsTapped));
        this.VirtualizableChildren.Add((IVirtualizable) this.LikesAndCommentsItem);
        margin.Top += this.LikesAndCommentsItem.FixedHeight;
      }
      else
        margin.Top += 12.0;
      this._height = margin.Top + 16.0;
    }

    private void HandleShowAll()
    {
      VKList<VideoCatalogItem> catalogItems = new VKList<VideoCatalogItem>();
      foreach (VKClient.Common.Backend.DataObjects.Video video in this._videos.items)
        catalogItems.items.Add(new VideoCatalogItem(video));
      catalogItems.profiles = this._newsItemWithInfo.Profiles;
      catalogItems.groups = this._newsItemWithInfo.Groups;
      catalogItems.count = this._videos.items.Count;
      Navigator.Current.NavigateToVideoList(catalogItems, 2, "", Guid.NewGuid().ToString(), "", "");
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this.HookUpEvents();
      this.CreateMenu();
      Rectangle rect = new Rectangle();
      double num1 = this.FixedHeight + this.Margin.Top;
      rect.Height = num1;
      Thickness thickness1 = new Thickness(0.0, -this.Margin.Top, 0.0, 0.0);
      rect.Margin = thickness1;
      double width1 = this.Width;
      rect.Width = width1;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
      rect.Fill = (Brush) solidColorBrush1;
      foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
        this.Children.Add(coverByRectangle);
      Rectangle rectangle = new Rectangle();
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneNewsDividerBrush"] as SolidColorBrush;
      rectangle.Fill = (Brush) solidColorBrush2;
      double width2 = this.Width;
      rectangle.Width = width2;
      double num2 = 16.0;
      rectangle.Height = num2;
      Thickness thickness2 = new Thickness(0.0, this.FixedHeight - 16.0, 0.0, 0.0);
      rectangle.Margin = thickness2;
      this.Children.Add((FrameworkElement) rectangle);
    }

    private void HookUpEvents()
    {
      this._view.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.View_OnTap);
    }

    private void UnHookEvents()
    {
      this._view.Tap -= new EventHandler<System.Windows.Input.GestureEventArgs>(this.View_OnTap);
    }

    private void View_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.CommentsTapped();
      e.Handled = true;
    }

    private void CommentsTapped()
    {
        VKClient.Common.Backend.DataObjects.Video video = this._videos.items[0];
      Navigator.Current.NavigateToVideoWithComments(video, video.owner_id, video.vid, video.access_key);
    }

    private void onNavigatedToUserOrGroup()
    {
    }

    private void OnMoreOptionsTap()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this._header.View);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }
  }
}
