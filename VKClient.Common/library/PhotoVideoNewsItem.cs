using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PhotoVideoNewsItem : VirtualizableItemBase, IHaveUniqueKey, IHaveNewsfeedItemId, ICanHideFromNewsfeed
  {
    private readonly double _marginBetweenElements = 12.0;
    private readonly NewsItemDataWithUsersAndGroupsInfo _newsPost;
    private readonly bool _fullScreen;
    private double _fixedHeight;
    private Photo _photo;
    private VKClient.Common.Backend.DataObjects.Video _video;
    private readonly Action<long> _hideSourceItemsCallback;
    private readonly Action<NewsFeedIgnoreItemData> _ignoreNewsfeedItemCallback;
    private bool _showDivideLine;
    private readonly Func<List<MenuItem>> _getExtraMenuItemsFunc;
    private List<MenuItem> _menuItems;
    private UserOrGroupHeaderItem _headerItem;

    public bool IsGroup
    {
      get
      {
        if (this.NewsType == NewsFeedItemType.PhotoTag)
          return false;
        return this.SourceId < 0L;
      }
    }

    public NewsFeedItemType NewsType
    {
      get
      {
        if (this._newsPost.NewsItem.post_type == "photo")
          return NewsFeedItemType.Photo;
        return this._newsPost.NewsItem.post_type == "photo_tag" ? NewsFeedItemType.PhotoTag : NewsFeedItemType.Video;
      }
    }

    public User FromUser
    {
      get
      {
          return (User)Enumerable.FirstOrDefault<User>(this._newsPost.Profiles, (Func<User, bool>)(u => u.uid == this.SourceId));
      }
    }

    public Group FromGroup
    {
      get
      {
          return (Group)Enumerable.FirstOrDefault<Group>(this._newsPost.Groups, (Func<Group, bool>)(g => g.id == -this.SourceId));
      }
    }

    public long SourceId
    {
      get
      {
        return this._newsPost.NewsItem.source_id;
      }
    }

    public string ExtraText
    {
      get
      {
        User fromUser = this.FromUser;
        if (fromUser != null)
        {
          bool flag = fromUser.sex == 2;
          if (this.NewsType == NewsFeedItemType.PhotoTag)
          {
            string str1 = flag ? CommonResources.Photo_WasTaggedMale : CommonResources.Photo_WasTaggedFemale;
            if (this._newsPost.NewsItem.PhotoTagsCount <= 0)
              return "";
            string str2 = UIStringFormatterHelper.FormatNumberOfSomething(this._newsPost.NewsItem.PhotoTagsCount, CommonResources.Photo_OnOnePhotoFrm, CommonResources.Photo_OnFivePhotosFrm, CommonResources.Photo_OnFivePhotosFrm, true,  null, false);
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "kk")
              return str2 + " " + str1;
            return str1 + " " + str2;
          }
          if (this.NewsType == NewsFeedItemType.Photo && this._newsPost.NewsItem.PhotosCount > 0 && this._newsPost.NewsItem.PhotosCount > 0)
          {
            string str1 = flag ? CommonResources.Photo_AddedMale : CommonResources.Photo_AddedFemale;
            string str2 = UIStringFormatterHelper.FormatNumberOfSomething(this._newsPost.NewsItem.PhotosCount, CommonResources.Photo_OnePhotoAddedFrm, CommonResources.Photo_TwoFourPhotosAddedFrm, CommonResources.Photo_FivePhotosAddedFrm, true,  null, false);
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "kk")
              return str2 + " " + str1;
            return str1 + " " + str2;
          }
        }
        return "";
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight;
      }
    }

    public string NewsfeedItemId
    {
      get
      {
        NewsItemDataWithUsersAndGroupsInfo newsPost = this._newsPost;
        if (newsPost == null)
          return  null;
        NewsItem newsItem = newsPost.NewsItem;
        if (newsItem == null)
          return  null;
        return newsItem.post_type;
      }
    }

    public PhotoVideoNewsItem(double width, Thickness margin, NewsItemDataWithUsersAndGroupsInfo newsPost, bool fullScreen, Action<long> hideSourceItemsCallback = null, bool showDivideLine = true, Action<NewsFeedIgnoreItemData> ignoreNewsfeedItemCallback = null, Func<List<MenuItem>> getExtraMenuItemsFunc = null)
      : base(width, margin, new Thickness())
    {
      this._newsPost = newsPost;
      this._fullScreen = fullScreen;
      this._hideSourceItemsCallback = hideSourceItemsCallback;
      this._showDivideLine = showDivideLine;
      this._ignoreNewsfeedItemCallback = ignoreNewsfeedItemCallback;
      this._getExtraMenuItemsFunc = getExtraMenuItemsFunc;
      this.Configure();
      this.CreateMenu();
      this.GenerateVirtualizableChildren();
    }

    private void Configure()
    {
      if (this._newsPost.NewsItem.pid != 0L && this._newsPost.NewsItem.post_type == "photo")
      {
        long aid = this._newsPost.NewsItem.aid;
        this._photo = new Photo()
        {
          pid = this._newsPost.NewsItem.pid,
          aid = aid,
          src_big = this._newsPost.NewsItem.src_big,
          src_small = this._newsPost.NewsItem.src_small,
          src_xbig = this._newsPost.NewsItem.src_xbig,
          src_xxbig = this._newsPost.NewsItem.src_xxbig,
          width = this._newsPost.NewsItem.width,
          height = this._newsPost.NewsItem.height,
          owner_id = this._newsPost.NewsItem.owner_id
        };
      }
      if (this._newsPost.NewsItem.vid == 0L || !(this._newsPost.NewsItem.post_type == "video"))
        return;
      this._video = new VKClient.Common.Backend.DataObjects.Video()
      {
        vid = this._newsPost.NewsItem.vid,
        duration = this._newsPost.NewsItem.duration,
        image_big = this._newsPost.NewsItem.image_big,
        owner_id = this._newsPost.NewsItem.owner_id
      };
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this.CreateMenu();
      Rectangle rect = new Rectangle();
      double fixedHeight = this.FixedHeight;
      Thickness margin1 = this.Margin;
      // ISSUE: explicit reference operation
      double top = margin1.Top;
      double num1 = fixedHeight + top;
      ((FrameworkElement) rect).Height = num1;
      double num2 = 0.0;
      Thickness margin2 = this.Margin;
      // ISSUE: explicit reference operation
      double num3 = -((Thickness) @margin2).Top;
      double num4 = 0.0;
      double num5 = 0.0;
      Thickness thickness1 = new Thickness(num2, num3, num4, num5);
      ((FrameworkElement) rect).Margin = thickness1;
      double num6 = 480.0;
      ((FrameworkElement) rect).Width = num6;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
      ((Shape) rect).Fill = ((Brush) solidColorBrush1);
      List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
          this.Children.Add((FrameworkElement) enumerator.Current);
      }
      finally
      {
        enumerator.Dispose();
      }
      if (!this._showDivideLine)
        return;
      Rectangle rectangle = new Rectangle();
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneNewsDividerBrush"] as SolidColorBrush;
      ((Shape) rectangle).Fill = ((Brush) solidColorBrush2);
      double num7 = 480.0;
      ((FrameworkElement) rectangle).Width = num7;
      double num8 = 16.0;
      ((FrameworkElement) rectangle).Height = num8;
      Thickness thickness2 = new Thickness(0.0, this.FixedHeight - 16.0, 0.0, 0.0);
      ((FrameworkElement) rectangle).Margin = thickness2;
      this.Children.Add((FrameworkElement) rectangle);
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this.ReleaseMenu();
    }

    private void CreateMenu()
    {
      this._menuItems = new List<MenuItem>();
      if (this._hideSourceItemsCallback != null && this.SourceId != AppGlobalStateManager.Current.LoggedInUserId)
      {
        MenuItem menuItem1 = new MenuItem();
        string hideFromNews = CommonResources.HideFromNews;
        menuItem1.Header = hideFromNews;
        MenuItem menuItem2 = menuItem1;
        // ISSUE: method pointer
        menuItem2.Click += new RoutedEventHandler( this.hideFromNewsMenuItem_Click);
        this._menuItems.Add(menuItem2);
      }
      if (this._ignoreNewsfeedItemCallback != null && this.GetIgnoreItemData() != null)
      {
        MenuItem menuItem1 = new MenuItem();
        string lowerInvariant = CommonResources.HideThisPost.ToLowerInvariant();
        menuItem1.Header = lowerInvariant;
        MenuItem menuItem2 = menuItem1;
        // ISSUE: method pointer
        menuItem2.Click += new RoutedEventHandler( this.HidePostItem_OnClick);
        this._menuItems.Add(menuItem2);
      }
      if (this._getExtraMenuItemsFunc != null)
        this._menuItems.AddRange((IEnumerable<MenuItem>) this._getExtraMenuItemsFunc());
      UserOrGroupHeaderItem headerItem = this._headerItem;
      if (headerItem == null)
        return;
      List<MenuItem> menuItems = this._menuItems;
      headerItem.SetMenu(menuItems);
    }

    private void hideFromNewsMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this._hideSourceItemsCallback(this.SourceId);
    }

    public NewsFeedIgnoreItemData GetIgnoreItemData()
    {
      NewsItem newsItem = this._newsPost.NewsItem;
      string postType = newsItem.post_type;
      string type = "";
      long sourceId = newsItem.source_id;
      long itemId = 0;
      if (postType == "photo" || postType == "photo_tag" || postType == "wall_photo")
      {
        if (!(postType == "photo") && !(postType == "wall_photo"))
        {
          if (postType == "photo_tag")
            type = "tag";
        }
        else
          type = "photo";
        Photo photo = this._photo;
        itemId = photo != null ? photo.id : BaseFormatterHelper.GetLastMidnight((long) newsItem.date, false);
      }
      else if (postType == "video")
      {
        type = "video";
        VKClient.Common.Backend.DataObjects.Video video = this._video;
        itemId = video != null ? video.id : BaseFormatterHelper.GetLastMidnight((long) newsItem.date, false);
      }
      if (!string.IsNullOrEmpty(type) && sourceId != 0L && itemId != 0L)
        return new NewsFeedIgnoreItemData(type, sourceId, itemId);
      return  null;
    }

    private void HidePostItem_OnClick(object sender, RoutedEventArgs e)
    {
      this._ignoreNewsfeedItemCallback(this.GetIgnoreItemData());
    }

    private void ReleaseMenu()
    {
      ContextMenuService.SetContextMenu((DependencyObject) this._view,  null);
    }

    private void GenerateVirtualizableChildren()
    {
      int date = this._newsPost.NewsItem.created != 0 ? this._newsPost.NewsItem.created : this._newsPost.NewsItem.date;
      double width1;
      Thickness margin1;
      if (this._fullScreen)
      {
        width1 = this.Width - 48.0;
        // ISSUE: explicit reference operation
        margin1=new Thickness(24.0, 32.0, 24.0, 0.0);
      }
      else
      {
        width1 = this.Width;
        // ISSUE: explicit reference operation
        margin1 = new Thickness();
      }
      Action moreOptionsTapCallback =  null;
      if (this._menuItems != null && this._menuItems.Count > 0)
      {
        // ISSUE: method pointer
        moreOptionsTapCallback = new Action( this.OnMoreOptionsTap);
      }
      this._headerItem = new UserOrGroupHeaderItem(width1, margin1, this.IsGroup, date, this.FromUser, this.FromGroup, this.ExtraText, PostIconType.None, PostSourcePlatform.None, moreOptionsTapCallback,  null, "");
      base.VirtualizableChildren.Add((IVirtualizable) this._headerItem);
      Thickness thickness = new Thickness();
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      double num1 = thickness.Top + this._marginBetweenElements + this._headerItem.FixedHeight + 2.0;
      thickness.Top = num1;
      double width2 = this.Width;
      string str = this._newsPost.NewsItem.from_id == 0L || this._newsPost.NewsItem.id <= 0L ? "" : string.Format("{0}_{1}", this._newsPost.NewsItem.from_id, this._newsPost.NewsItem.id);
      if (this._photo != null)
      {
        double width3 = width2;
        Thickness margin2 = thickness;
        List<Attachment> attachments = new List<Attachment>();
        Attachment attachment = new Attachment();
        attachment.type = "photo";
        Photo photo = this._photo;
        attachment.photo = photo;
        attachments.Add(attachment);
        // ISSUE: variable of the null type
        
        string itemId = str;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        double horizontalWidth = 0.0;
        int num6 = 0;
        int num7 = 0;
        string hyperlinkId = "";
        // ISSUE: variable of the null type
        
        // ISSUE: variable of the null type
        
        int num8 = 0;
        AttachmentsItem attachmentsItem = new AttachmentsItem(width3, margin2, attachments, null, itemId, num2 != 0, num3 != 0, num4 != 0, num5 != 0, horizontalWidth, num6 != 0, num7 != 0, hyperlinkId, null, null, num8 != 0);
        base.VirtualizableChildren.Add((IVirtualizable) attachmentsItem);
        // ISSUE: explicit reference operation
        this._fixedHeight = ((Thickness) @thickness).Top + attachmentsItem.FixedHeight;
      }
      else if (this._video != null)
      {
        double width3 = width2;
        Thickness margin2 = thickness;
        List<Attachment> attachments = new List<Attachment>();
        Attachment attachment = new Attachment();
        attachment.type = "video";
        VKClient.Common.Backend.DataObjects.Video video = this._video;
        attachment.video = video;
        attachments.Add(attachment);
        // ISSUE: variable of the null type
        
        string itemId = str;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        double horizontalWidth = 0.0;
        int num6 = 0;
        int num7 = 0;
        string hyperlinkId = "";
        // ISSUE: variable of the null type
        
        // ISSUE: variable of the null type
        
        int num8 = 0;
        AttachmentsItem attachmentsItem = new AttachmentsItem(width3, margin2, attachments, null, itemId, num2 != 0, num3 != 0, num4 != 0, num5 != 0, horizontalWidth, num6 != 0, num7 != 0, hyperlinkId, null, null, num8 != 0);
        base.VirtualizableChildren.Add((IVirtualizable) attachmentsItem);
        // ISSUE: explicit reference operation
        this._fixedHeight = ((Thickness) @thickness).Top + attachmentsItem.FixedHeight;
      }
      else
      {
        bool flag = this._newsPost.NewsItem.post_type == "photo" || this._newsPost.NewsItem.post_type == "wall_photo";
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        ThumbsItem thumbsItem = new ThumbsItem(width2, new Thickness(((Thickness) @thickness).Left, ((Thickness) @thickness).Top, 0.0, 0.0), new NewsPhotosInfo() { SourceId = this._newsPost.NewsItem.source_id, Date = this._newsPost.NewsItem.date, NewsType = (NewsPhotosInfo.NewsPhotoType) (flag ? 0 : 1), Count = flag ? this._newsPost.NewsItem.PhotosCount : this._newsPost.NewsItem.PhotoTagsCount, Photos = flag ? this._newsPost.NewsItem.Photos : this._newsPost.NewsItem.Photo_tags });
        base.VirtualizableChildren.Add((IVirtualizable) thumbsItem);
        // ISSUE: explicit reference operation
        this._fixedHeight = ((Thickness) @thickness).Top + thumbsItem.FixedHeight;
        this._fixedHeight = this._fixedHeight + 31.0;
      }
    }

    public void OnMoreOptionsTap()
    {
      UserOrGroupHeaderItem headerItem = this._headerItem;
      // ISSUE: explicit non-virtual call
      ContextMenu contextMenu = ContextMenuService.GetContextMenu(headerItem != null ?  headerItem.View :  null);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    public string GetKey()
    {
      string str = "";
      if (this._video != null)
        str = string.Format("video{0}", this._video.id);
      else if (this._photo != null)
      {
        str = string.Format("photo{0}", this._photo.id);
      }
      else
      {
        string postType = this._newsPost.NewsItem.post_type;
        if (!(postType == "photo"))
        {
          if (!(postType == "wall_photo"))
          {
            if (postType == "photo_tag")
              str = string.Format("photo_tag{0}_{1}", this._newsPost.NewsItem.source_id, this._newsPost.NewsItem.PhotoTagsCount);
          }
          else
            str = string.Format("wall_photo{0}_{1}", this._newsPost.NewsItem.source_id, this._newsPost.NewsItem.PhotosCount);
        }
        else
          str = string.Format("photo{0}_{1}", this._newsPost.NewsItem.source_id, this._newsPost.NewsItem.PhotosCount);
      }
      return str;
    }
  }
}
