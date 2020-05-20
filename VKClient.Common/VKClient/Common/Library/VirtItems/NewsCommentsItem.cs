using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsCommentsItem : VirtualizableItemBase, ISupportChildHeightChange
  {
    private double _fixedHeight;
    private readonly NewsItemDataWithUsersAndGroupsInfo _newsItemData;
    private readonly Action<string> _unsubscribedCallback;
    private double _mainPostHeight;
    private string _type;
    private long _ownerId;
    private long _id;
    private VirtualizableItemBase _commentsCountItem;
    private List<CommentItem> _commentItems;
    private UserOrGroupHeaderItem _headerItem;
    private TextItem _topicTextItem;
    private PhotoVideoNewsItem _videoItem;
    private PhotoVideoNewsItem _photoItem;
    private UCItem _marketItem;
    private WallPostItem _wallPostItem;

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight + 16.0;
      }
    }

    public string ItemId { get; private set; }

    public NewsCommentsItem(double width, Thickness margin, NewsItemDataWithUsersAndGroupsInfo newsItemData, Action<string> unsubscribedCallback)
      : base(width, margin, new Thickness())
    {
      this._newsItemData = newsItemData;
      this._unsubscribedCallback = unsubscribedCallback;
      this.GenerateVirtualizableChildren();
      this._view.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._view_Tap);
    }

    private void _view_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      string postType = this._newsItemData.NewsItem.post_type;
      if (!(postType == "post"))
      {
        if (!(postType == "photo"))
        {
          if (!(postType == "video"))
          {
            if (!(postType == "topic"))
            {
              if (postType == "note" || !(postType == "market"))
                return;
              CurrentMarketItemSource.Source = MarketItemSource.feed;
              Navigator.Current.NavigateToProduct(this._newsItemData.NewsItem.source_id, this._newsItemData.NewsItem.id);
              e.Handled = true;
            }
            else
            {
              Navigator.Current.NavigateToGroupDiscussion(Math.Abs(this._newsItemData.NewsItem.source_id), this._newsItemData.NewsItem.post_id, this._newsItemData.NewsItem.text, this._newsItemData.NewsItem.comments.count, true, this._newsItemData.NewsItem.comments.can_post == 1);
              e.Handled = true;
            }
          }
          else
          {
              VKClient.Common.Backend.DataObjects.Video video = new VKClient.Common.Backend.DataObjects.Video()
            {
              vid = this._newsItemData.NewsItem.vid,
              duration = this._newsItemData.NewsItem.duration,
              image_big = this._newsItemData.NewsItem.image_big,
              owner_id = this._newsItemData.NewsItem.owner_id
            };
            Navigator.Current.NavigateToVideoWithComments(video, video.owner_id, video.vid, "");
            e.Handled = true;
          }
        }
        else
        {
          long aid = this._newsItemData.NewsItem.aid;
          Photo photo = new Photo()
          {
            pid = this._newsItemData.NewsItem.pid,
            aid = aid,
            src_big = this._newsItemData.NewsItem.src_big,
            src_small = this._newsItemData.NewsItem.src_small,
            src_xbig = this._newsItemData.NewsItem.src_xbig,
            src_xxbig = this._newsItemData.NewsItem.src_xxbig,
            width = this._newsItemData.NewsItem.width,
            height = this._newsItemData.NewsItem.height,
            owner_id = this._newsItemData.NewsItem.owner_id
          };
          Navigator.Current.NavigateToPhotoWithComments(photo, (PhotoWithFullInfo) null, this._newsItemData.NewsItem.owner_id, photo.pid, "", false, false);
          e.Handled = true;
        }
      }
      else
      {
        Navigator.Current.NavigateToWallPostComments(this._newsItemData.NewsItem.post_id, this._newsItemData.NewsItem.source_id, true, 0L, 0L, "");
        e.Handled = true;
      }
    }

    private void GenerateVirtualizableChildren()
    {
      NewsItem newsItem = this._newsItemData.NewsItem;
      this._type = newsItem.type;
      this._ownerId = newsItem.owner_id;
      this._id = newsItem.id;
      double topMargin = 0.0;
      string str = this._type;
      if (!(str == "post"))
      {
        if (!(str == "photo"))
        {
          if (!(str == "video"))
          {
            if (!(str == "market"))
            {
              if (!(str == "topic"))
              {
                if (str == "note")
                  topMargin = this.GenerateNote();
              }
              else
              {
                topMargin = this.GenerateTopic();
                this._id = newsItem.post_id;
              }
            }
            else
              topMargin = this.GenerateMarket();
          }
          else
            topMargin = this.GenerateVideo();
        }
        else
          topMargin = this.GeneratePhoto();
      }
      else
      {
        topMargin = this.GenerateWallPost();
        this._ownerId = newsItem.source_id;
        this._id = newsItem.post_id;
      }
      this.ItemId = string.Format("{0}{1}_{2}", (object) this._type, (object) this._ownerId, (object) this._id);
      this._mainPostHeight = topMargin;
      this.GenerateComments(topMargin);
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      if (this._headerItem != null)
        this._headerItem.SetMenu(this.GenerateMenuItems());
      Rectangle rect = new Rectangle();
      double fixedHeight = this.FixedHeight;
      Thickness margin = this.Margin;
      double top1 = margin.Top;
      double num1 = fixedHeight + top1;
      rect.Height = num1;
      double left = 0.0;
      margin = this.Margin;
      double top2 = -margin.Top;
      double right = 0.0;
      double bottom = 0.0;
      Thickness thickness1 = new Thickness(left, top2, right, bottom);
      rect.Margin = thickness1;
      double num2 = 480.0;
      rect.Width = num2;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
      rect.Fill = (Brush) solidColorBrush1;
      foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
        this.Children.Add(coverByRectangle);
      Rectangle rectangle = new Rectangle();
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneNewsDividerBrush"] as SolidColorBrush;
      rectangle.Fill = (Brush) solidColorBrush2;
      double num3 = 480.0;
      rectangle.Width = num3;
      double num4 = 16.0;
      rectangle.Height = num4;
      Thickness thickness2 = new Thickness(0.0, this.FixedHeight - 16.0, 0.0, 0.0);
      rectangle.Margin = thickness2;
      this.Children.Add((FrameworkElement) rectangle);
    }

    private void GenerateComments(double topMargin)
    {
      if (this._commentsCountItem == null)
      {
        this._commentsCountItem = CommentsItemsGeneratorHelper.CreateCommentsCountItem(this.Width, new Func<string>(this.GetCommentsHeaderText), 0.0, Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush);
        this.VirtualizableChildren.Add((IVirtualizable) this._commentsCountItem);
      }
      this._commentsCountItem.ViewMargin = new Thickness(0.0, topMargin, 0.0, 0.0);
      topMargin += this._commentsCountItem.FixedHeight;
      if (this._commentItems == null)
      {
        this._commentItems = new List<CommentItem>();
        Comments comments = this._newsItemData.NewsItem.comments;
        if ((comments != null ? comments.list : (List<Comment>) null) != null)
        {
          foreach (Comment comment1 in new List<Comment>((IEnumerable<Comment>) this._newsItemData.NewsItem.comments.list).OrderBy<Comment, int>((Func<Comment, int>) (c => c.date)).TakeLast<Comment>(1))
          {
            Comment comment = comment1;
            Stopwatch stopwatch = Stopwatch.StartNew();
            CommentItem commentItem = new CommentItem(448.0, new Thickness(), LikeObjectType.comment, (Action<CommentItem>) null, (Action<CommentItem>) null, (Action<CommentItem>) null, this._newsItemData.NewsItem.source_id, comment, this._newsItemData.Profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == comment.from_id)), (User) null, this._newsItemData.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == comment.from_id)), (Action<CommentItem>) null, "", "", (Action<CommentItem>) null, true, false, "");
            stopwatch.Stop();
            this.VirtualizableChildren.Add((IVirtualizable) commentItem);
            commentItem.ViewMargin = new Thickness(16.0, topMargin, 0.0, 0.0);
            topMargin += commentItem.FixedHeight + 16.0;
            this._commentItems.Add(commentItem);
          }
        }
      }
      else
      {
        foreach (CommentItem commentItem in this._commentItems)
        {
          commentItem.ViewMargin = new Thickness(16.0, topMargin, 0.0, 0.0);
          topMargin += commentItem.FixedHeight + 16.0;
        }
      }
      this._fixedHeight = topMargin;
    }

    private string GetCommentsHeaderText()
    {
      Comments comments = this._newsItemData.NewsItem.comments;
      if ((comments != null ? comments.list : (List<Comment>) null) == null || this._newsItemData.NewsItem.comments.count == 0)
        return string.Empty;
      if (this._newsItemData.NewsItem.comments.list.Count == this._newsItemData.NewsItem.comments.count)
        return UIStringFormatterHelper.FormatNumberOfSomething(this._newsItemData.NewsItem.comments.count, CommonResources.PostCommentPage_OneCommentFrm, CommonResources.PostCommentPage_TwoFourCommentsFrm, CommonResources.PostCommentPage_FiveCommentsFrm, true, null, false);
      return UIStringFormatterHelper.FormatNumberOfSomething(this._newsItemData.NewsItem.comments.count, CommonResources.LastOfOneCommentFrm, CommonResources.LastOfTwoFourCommentsFrm, CommonResources.LastOfFiveCommentsFrm, true, null, false);
    }

    private double GenerateNote()
    {
      return 0.0;
    }

    private double GenerateTopic()
    {
      double top1 = 0.0;
      if (this._headerItem == null)
      {
        this._headerItem = new UserOrGroupHeaderItem(this.Width, new Thickness(), true, this._newsItemData.NewsItem.date, null, this._newsItemData.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._newsItemData.NewsItem.source_id)), "", PostIconType.None, PostSourcePlatform.None, new Action(this.OnMoreOptionsTap), null, "");
        this._headerItem.SetMenu(this.GenerateMenuItems());
        this.VirtualizableChildren.Add((IVirtualizable) this._headerItem);
      }
      this._headerItem.ViewMargin = new Thickness(0.0, top1, 0.0, 0.0);
      double top2 = top1 + (this._headerItem.FixedHeight + 16.0);
      if (this._topicTextItem == null)
      {
        this._topicTextItem = new TextItem(this.Width - 32.0, new Thickness(), this._newsItemData.NewsItem.text, false, 32.0, "Segoe WP SemiLight", 0.0, Application.Current.Resources["PhoneContrastTitleBrush"] as SolidColorBrush, true, null);
        this.VirtualizableChildren.Add((IVirtualizable) this._topicTextItem);
      }
      this._topicTextItem.ViewMargin = new Thickness(16.0, top2, 16.0, 0.0);
      return top2 + (this._topicTextItem.FixedHeight + 16.0);
    }

    private double GenerateVideo()
    {
      if (this._videoItem == null)
      {
        this._videoItem = new PhotoVideoNewsItem(this.Width, new Thickness(), this._newsItemData, false, (Action<long>) null, false, (Action<NewsFeedIgnoreItemData>) null, new Func<List<MenuItem>>(this.GenerateMenuItems));
        this.VirtualizableChildren.Add((IVirtualizable) this._videoItem);
      }
      return this._videoItem.FixedHeight + 16.0;
    }

    private double GeneratePhoto()
    {
      if (this._photoItem == null)
      {
        this._photoItem = new PhotoVideoNewsItem(this.Width, new Thickness(), this._newsItemData, false, (Action<long>) null, false, (Action<NewsFeedIgnoreItemData>) null, new Func<List<MenuItem>>(this.GenerateMenuItems));
        this.VirtualizableChildren.Add((IVirtualizable) this._photoItem);
      }
      return this._photoItem.FixedHeight + 16.0;
    }

    private double GenerateMarket()
    {
      double num = 0.0;
      NewsItem newsItem = this._newsItemData.NewsItem;
      if (this._headerItem == null)
      {
        this._headerItem = new UserOrGroupHeaderItem(this.Width, new Thickness(), true, newsItem.date, (User) null, this._newsItemData.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._newsItemData.NewsItem.source_id)), "", PostIconType.None, PostSourcePlatform.None, new Action(this.OnMoreOptionsTap), null, "");
        this._headerItem.SetMenu(this.GenerateMenuItems());
        this.VirtualizableChildren.Add((IVirtualizable) this._headerItem);
      }
      this._headerItem.ViewMargin = new Thickness();
      double top = num + (this._headerItem.FixedHeight + 16.0);
      if (this._marketItem == null)
      {
        Product product = new Product()
        {
          owner_id = newsItem.owner_id,
          id = newsItem.id,
          title = newsItem.title,
          thumb_photo = newsItem.thumb_photo,
          availability = newsItem.availability,
          price = newsItem.price,
          category = newsItem.category
        };
        Link link1 = new Link();
        link1.title = newsItem.title;
        link1.caption = CommonResources.Product;
        link1.photo = new Photo()
        {
          photo_75 = newsItem.thumb_photo
        };
        LinkProduct linkProduct = new LinkProduct(product);
        link1.product = linkProduct;
        string str = string.Format("http://vk.com/product{0}_{1}", (object) product.owner_id, (object) product.id);
        link1.url = str;
        Link link = link1;
        NewsLinkUCBase tmpUC = (NewsLinkUCBase) new NewsLinkMediumUC();
        tmpUC.Initialize(link, this.Width);
        this._marketItem = new UCItem(this.Width - 16.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
        {
          NewsLinkMediumUC newsLinkMediumUc = new NewsLinkMediumUC();
          //Link link = link;
          double width = this.Width;
          newsLinkMediumUc.Initialize(link, width);
          return (UserControlVirtualizable) newsLinkMediumUc;
        }), (Func<double>) (() => tmpUC.CalculateTotalHeight()), (Action<UserControlVirtualizable>) null, 0.0, false);
        this.VirtualizableChildren.Add((IVirtualizable) this._marketItem);
      }
      this._marketItem.ViewMargin = new Thickness(8.0, top, 8.0, 0.0);
      return top + this._marketItem.FixedHeight + 16.0;
    }

    private double GenerateWallPost()
    {
      if (this._wallPostItem == null)
      {
        this._wallPostItem = new WallPostItem(this.Width, new Thickness(), true, this._newsItemData, (Action<WallPostItem>) null, true, (Action<long, User, Group>) null, false, false, false, false, (NewsFeedAdsItem) null, (Func<List<MenuItem>>) (() => new List<MenuItem>()
        {
          this.GetUnsubscribeMenuItem()
        }));
        this.VirtualizableChildren.Add((IVirtualizable) this._wallPostItem);
      }
      return this._wallPostItem.FixedHeight;
    }

    private List<MenuItem> GenerateMenuItems()
    {
      List<MenuItem> menuItemList = new List<MenuItem>();
      MenuItem menuItem1 = new MenuItem();
      string copyLink = CommonResources.CopyLink;
      menuItem1.Header = (object) copyLink;
      MenuItem menuItem2 = menuItem1;
      menuItem2.Click += new RoutedEventHandler(this.CopyLinkMenuItem_OnClick);
      MenuItem menuItem3 = menuItem2;
      menuItemList.Add(menuItem3);
      MenuItem unsubscribeMenuItem = this.GetUnsubscribeMenuItem();
      menuItemList.Add(unsubscribeMenuItem);
      return menuItemList;
    }

    private void CopyLinkMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
      NewsItem newsItem = this._newsItemData.NewsItem;
      string str = "";
      string type = newsItem.type;
      if (!(type == "photo"))
      {
        if (!(type == "video"))
        {
          if (!(type == "market"))
          {
            if (!(type == "topic"))
            {
              if (type == "note")
                str = "note";
            }
            else
              str = "topic";
          }
          else
            str = string.Format("market{0}?w=product", (object) newsItem.owner_id);
        }
        else
          str = "video";
      }
      else
        str = "photo";
      if (string.IsNullOrEmpty(str))
        return;
      Clipboard.SetText(string.Format("http://vk.com/{0}{1}_{2}", (object) str, (object) newsItem.owner_id, (object) newsItem.id));
    }

    private MenuItem GetUnsubscribeMenuItem()
    {
      MenuItem menuItem = new MenuItem();
      string unsubscribe = CommonResources.Unsubscribe;
      menuItem.Header = (object) unsubscribe;
      RoutedEventHandler routedEventHandler = (RoutedEventHandler) ((sender, args) =>
      {
        NewsFeedService.Current.Unsubscribe(this._type, this._ownerId, this._id, (Action<BackendResult<bool, ResultCode>>) (result => {}));
        Action<string> action = this._unsubscribedCallback;
        if (action == null)
          return;
        string itemId = this.ItemId;
        action(itemId);
      });
      menuItem.Click += routedEventHandler;
      return menuItem;
    }

    private void OnMoreOptionsTap()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this._headerItem.View);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    public void RespondToChildHeightChange(IVirtualizable child)
    {
      this.GenerateVirtualizableChildren();
      this.RegenerateChildren();
      this.NotifyHeightChanged();
    }
  }
}
