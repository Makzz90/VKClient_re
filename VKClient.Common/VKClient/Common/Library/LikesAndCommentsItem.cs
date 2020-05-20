using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

using VKClient.Common.BLExtensions;
using VKClient.Audio.Base.BLExtensions;

namespace VKClient.Common.Library
{
  public class LikesAndCommentsItem : VirtualizableItemBase, IHandle<WallCommentIsAddedDeleted>, IHandle, IHandle<WallCommentsLikesUpdated>, IHandle<ObjectLikedUnlikedEvent>, IHandle<VideoCommentIsAddedDeleted>, IHandle<VideoCommentsLikesUpdated>
  {
    private readonly LikesAndCommentsItem.ItemType _itemType;
    private const string IMAGE_PATH_LIKES = "/Resources/WallPost/PostLike.png";
    private const string IMAGE_PATH_COMMENTS = "/Resources/WallPost/PostComment.png";
    private const string IMAGE_PATH_SHARES = "/Resources/WallPost/PostShare.png";
    private const int ITEM_HEIGHT = 64;
    private readonly WallPost _wallPost;
    private readonly Action _commentsTappedAction;
    private readonly bool _suppressRepostButton;
    private readonly bool _displaySeparator;
    private Border _imageLike;
    private Border _imageComment;
    private Border _imageRepost;
    private DialogService _sharePopup;
    private SharePostUC _sharePostUC;
    private readonly VKClient.Common.Backend.DataObjects.Video _video;
    private const int MARGIN_LEFT_IMAGE = 16;
    private const int MARGIN_RIGHT_IMAGE = 16;
    private const int MARGIN_LEFT_TEXT = 8;
    private const int MARGIN_RIGHT_TEXT = 20;
    private const int IMAGE_WIDTH_HEIGHT = 32;
    private const int MARGIN_LEFT_ABSOLUTE_TEXT = 56;

    private bool CanPost
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.comments.can_post == 1;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.can_comment == 1;
          default:
            return false;
        }
      }
    }

    private int CommentsCount
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.comments.count;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.comments;
          default:
            return 0;
        }
      }
      set
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            this._wallPost.comments.count = value;
            break;
          case LikesAndCommentsItem.ItemType.Video:
            this._video.comments = value;
            break;
        }
      }
    }

    private bool CanLike
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.likes.can_like == 1;
          case LikesAndCommentsItem.ItemType.Video:
            return true;
          default:
            return false;
        }
      }
      set
      {
        if (this._itemType != LikesAndCommentsItem.ItemType.Post)
          return;
        this._wallPost.likes.can_like = value ? 1 : 0;
      }
    }

    private int LikesCount
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.likes.count;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.likes.count;
          default:
            return 0;
        }
      }
      set
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            this._wallPost.likes.count = value;
            break;
          case LikesAndCommentsItem.ItemType.Video:
            this._video.likes.count = value;
            break;
        }
      }
    }

    private bool UserLikes
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.likes.user_likes == 1;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.likes.user_likes == 1;
          default:
            return false;
        }
      }
      set
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            this._wallPost.likes.user_likes = value ? 1 : 0;
            break;
          case LikesAndCommentsItem.ItemType.Video:
            this._video.likes.user_likes = value ? 1 : 0;
            break;
        }
      }
    }

    private bool UserReposted
    {
      get
      {
        if (this._itemType == LikesAndCommentsItem.ItemType.Post)
          return this._wallPost.reposts.user_reposted == 1;
        return false;
      }
      set
      {
        if (this._itemType != LikesAndCommentsItem.ItemType.Post)
          return;
        this._wallPost.reposts.user_reposted = value ? 1 : 0;
      }
    }

    private int RepostsCount
    {
      get
      {
        if (this._itemType == LikesAndCommentsItem.ItemType.Post)
          return this._wallPost.reposts.count;
        return 0;
      }
      set
      {
        if (this._itemType != LikesAndCommentsItem.ItemType.Post)
          return;
        this._wallPost.reposts.count = value;
      }
    }

    private long ItemId
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.id;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.id;
          default:
            return 0;
        }
      }
    }

    private long OwnerId
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.owner_id;
          case LikesAndCommentsItem.ItemType.Video:
            return this._video.owner_id;
          default:
            return 0;
        }
      }
    }

    private LikeObjectType LikeObjType
    {
      get
      {
        switch (this._itemType)
        {
            case LikesAndCommentsItem.ItemType.Post:
                {
                    return this._wallPost.GetLikeObjectType();
                }
          case LikesAndCommentsItem.ItemType.Video:
            return LikeObjectType.video;
          default:
            return LikeObjectType.post;
        }
      }
    }

    private bool CanPublish
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.likes.can_publish == 1;
          case LikesAndCommentsItem.ItemType.Video:
            return true;
          default:
            return false;
        }
      }
    }

    private bool CanRepostToCommunity
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.CanRepostToCommunity();
          case LikesAndCommentsItem.ItemType.Video:
            return true;
          default:
            return false;
        }
      }
    }

    private RepostObject RepostObjectType
    {
      get
      {
        switch (this._itemType)
        {
          case LikesAndCommentsItem.ItemType.Post:
            return this._wallPost.GetRepostObjectType();
          case LikesAndCommentsItem.ItemType.Video:
            return RepostObject.video;
          default:
            return RepostObject.wall;
        }
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 64.0;
      }
    }

    public LikesAndCommentsItem(double width, Thickness margin, WallPost wallPost, Action commentsTapped, bool suppressRepostButton, bool displaySeparator)
      : base(width, margin, new Thickness())
    {
      if (wallPost == null)
        Debugger.Break();
      this._wallPost = wallPost;
      if (this._wallPost != null)
      {
        if (this._wallPost.likes == null)
          this._wallPost.likes = new Likes();
        if (this._wallPost.comments == null)
          this._wallPost.comments = new Comments();
        if (this._wallPost.reposts == null)
          this._wallPost.reposts = new Reposts();
      }
      this._itemType = LikesAndCommentsItem.ItemType.Post;
      this._commentsTappedAction = commentsTapped;
      this._suppressRepostButton = suppressRepostButton;
      this._displaySeparator = displaySeparator;
      this.Initialize();
    }

    public LikesAndCommentsItem(double width, Thickness margin, VKClient.Common.Backend.DataObjects.Video video, Action commentsTapped)
      : base(width, margin, new Thickness())
    {
      this._video = video;
      if (this._video.likes == null)
        this._video.likes = new Likes();
      this._itemType = LikesAndCommentsItem.ItemType.Video;
      this._commentsTappedAction = commentsTapped;
      this.Initialize();
    }

    private void Initialize()
    {
      EventAggregator.Current.Subscribe((object) this);
      this.CreateImages();
    }

    private void CreateImages()
    {
      this._imageLike = LikesAndCommentsItem.GetImageBorder("/Resources/WallPost/PostLike.png");
      this._imageComment = LikesAndCommentsItem.GetImageBorder("/Resources/WallPost/PostComment.png");
      this._imageRepost = LikesAndCommentsItem.GetImageBorder("/Resources/WallPost/PostShare.png");
    }

    private static Border GetImageBorder(string imageUri)
    {
      Border border = new Border();
      double num1 = 32.0;
      border.Width = num1;
      double num2 = 32.0;
      border.Height = num2;
      SolidColorBrush iconBackground = LikesAndCommentsItem.GetIconBackground(false);
      border.Background = (Brush) iconBackground;
      double length = 16.0;
      Canvas.SetTop((UIElement) border, length);
      BitmapImage bitmapImage = new BitmapImage(new Uri(MultiResolutionHelper.Instance.AppendResolutionSuffix(imageUri, true, ""), UriKind.Relative));
      border.OpacityMask = (Brush) new ImageBrush()
      {
        ImageSource = (ImageSource) bitmapImage
      };
      return border;
    }

    protected override void GenerateChildren()
    {
      Canvas canvas1 = this._imageComment.Parent as Canvas;
      if (canvas1 != null)
        canvas1.Children.Clear();
      Canvas canvas2 = this._imageLike.Parent as Canvas;
      if (canvas2 != null)
        canvas2.Children.Clear();
      Canvas canvas3 = this._imageRepost.Parent as Canvas;
      if (canvas3 != null)
        canvas3.Children.Clear();
      Canvas itemContainer1 = LikesAndCommentsItem.GetItemContainer("like", new EventHandler<GestureEventArgs>(this.ButtonLikes_OnTap));
      TextBlock textBlock1 = LikesAndCommentsItem.GetTextBlock();
      itemContainer1.Children.Add((UIElement) this._imageLike);
      itemContainer1.Children.Add((UIElement) textBlock1);
      this.Children.Add((FrameworkElement) itemContainer1);
      Canvas itemContainer2 = LikesAndCommentsItem.GetItemContainer("comment", new EventHandler<GestureEventArgs>(this.ButtonComments_OnTap));
      TextBlock textBlock2 = LikesAndCommentsItem.GetTextBlock();
      itemContainer2.Children.Add((UIElement) this._imageComment);
      itemContainer2.Children.Add((UIElement) textBlock2);
      this.Children.Add((FrameworkElement) itemContainer2);
      this.UpdateCommentsVisibility((UIElement) this._imageComment, textBlock2, itemContainer2);
      Canvas itemContainer3 = LikesAndCommentsItem.GetItemContainer("repost", new EventHandler<GestureEventArgs>(this.RepostsCanvas_OnTap));
      TextBlock textBlock3 = LikesAndCommentsItem.GetTextBlock();
      itemContainer3.Children.Add((UIElement) this._imageRepost);
      itemContainer3.Children.Add((UIElement) textBlock3);
      this.Children.Add((FrameworkElement) itemContainer3);
      if (this._displaySeparator)
        this.Children.Add((FrameworkElement) this.GetSeparator());
      this.UpdateViews(this._imageLike, textBlock1, itemContainer1, this._imageComment, textBlock2, itemContainer2, this._imageRepost, textBlock3, itemContainer3);
    }

    private static Canvas GetItemContainer(string tag, EventHandler<GestureEventArgs> tapHandler)
    {
      Canvas canvas = new Canvas();
      string str = tag;
      canvas.Tag = (object) str;
      double num1 = 64.0;
      canvas.Height = num1;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      canvas.Background = (Brush) solidColorBrush;
      EventHandler<GestureEventArgs> eventHandler = tapHandler;
      canvas.Tap += eventHandler;
      double num2 = 3.0;
      MetroInMotion.SetTilt((DependencyObject) canvas, num2);
      return canvas;
    }

    private static TextBlock GetTextBlock()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 20.0;
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush counterForeground = LikesAndCommentsItem.GetIconCounterForeground(false);
      textBlock.Foreground = (Brush) counterForeground;
      double length = 17.0;
      Canvas.SetTop((UIElement) textBlock, length);
      return textBlock;
    }

    private Rectangle GetSeparator()
    {
      Rectangle rectangle = new Rectangle();
      double num1 = 1.0;
      rectangle.Height = num1;
      double num2 = this.Width - 32.0;
      rectangle.Width = num2;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneWallPostLikesSeparatorBrush"];
      rectangle.Fill = (Brush) solidColorBrush;
      double length = 16.0;
      Canvas.SetLeft((UIElement) rectangle, length);
      return rectangle;
    }

    private void ButtonLikes_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      if (!this.CanLike && !this.UserLikes)
        return;
      LikesService.Current.AddRemoveLike(!this.UserLikes, this.OwnerId, this.ItemId, this.LikeObjType, (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}), "");
      this.FireLikedEvent();
    }

    private void FireLikedEvent()
    {
      if (this._itemType != LikesAndCommentsItem.ItemType.Post)
        return;
      EventAggregator current = EventAggregator.Current;
      WallItemLikedUnliked itemLikedUnliked = new WallItemLikedUnliked();
      itemLikedUnliked.OwnerId = this._wallPost.to_id;
      itemLikedUnliked.WallPostId = this._wallPost.id;
      int num = this._wallPost.likes.user_likes == 0 ? 1 : 0;
      itemLikedUnliked.Liked = num != 0;
      current.Publish((object) itemLikedUnliked);
    }

    private void ButtonComments_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      Action action = this._commentsTappedAction;
      if (action == null)
        return;
      action();
    }

    private void UpdateViews(Border imageLike, TextBlock textBlockLikesCount, Canvas likesCanvas, Border imageComment, TextBlock textBlockCommentsCount, Canvas commentsCanvas, Border imageRepost, TextBlock textBlockRepostCount, Canvas repostsCanvas)
    {
      bool userLikes = this.UserLikes;
      imageLike.Background = (Brush) LikesAndCommentsItem.GetIconBackground(userLikes);
      textBlockLikesCount.Foreground = (Brush) LikesAndCommentsItem.GetIconCounterForeground(userLikes);
      textBlockLikesCount.Text = LikesAndCommentsItem.GetTextForCount(this.LikesCount);
      double actualWidth1 = textBlockLikesCount.ActualWidth;
      if (actualWidth1 > 0.0)
        likesCanvas.Width = 56.0 + actualWidth1 + 20.0;
      else
        likesCanvas.Width = 64.0;
      Canvas.SetLeft((UIElement) imageLike, 16.0);
      Canvas.SetLeft((UIElement) textBlockLikesCount, 56.0);
      Canvas.SetLeft((UIElement) commentsCanvas, likesCanvas.Width);
      textBlockCommentsCount.Text = LikesAndCommentsItem.GetTextForCount(this.CommentsCount);
      double actualWidth2 = textBlockCommentsCount.ActualWidth;
      if (actualWidth2 > 0.0)
        commentsCanvas.Width = 56.0 + actualWidth2 + 20.0;
      else
        commentsCanvas.Width = 64.0;
      Canvas.SetLeft((UIElement) imageComment, 16.0);
      Canvas.SetLeft((UIElement) textBlockCommentsCount, 56.0);
      Visibility visibility = !this._suppressRepostButton ? Visibility.Visible : Visibility.Collapsed;
      imageRepost.Visibility = visibility;
      textBlockRepostCount.Visibility = visibility;
      bool userReposted = this.UserReposted;
      imageRepost.Background = (Brush) LikesAndCommentsItem.GetIconBackground(userReposted);
      textBlockRepostCount.Foreground = (Brush) LikesAndCommentsItem.GetIconCounterForeground(userReposted);
      textBlockRepostCount.Text = LikesAndCommentsItem.GetTextForCount(this.RepostsCount);
      double actualWidth3 = textBlockRepostCount.ActualWidth;
      double num;
      if (actualWidth3 > 0.0)
      {
        num = 56.0 + actualWidth3 + 20.0;
        Canvas.SetLeft((UIElement) imageRepost, 16.0);
      }
      else
      {
        num = 64.0;
        Canvas.SetLeft((UIElement) imageRepost, 16.0);
      }
      repostsCanvas.Width = num;
      Canvas.SetLeft((UIElement) textBlockRepostCount, 56.0);
      Canvas.SetLeft((UIElement) repostsCanvas, this.Width - num);
    }

    private static SolidColorBrush GetIconBackground(bool isActive)
    {
      return Application.Current.Resources[isActive ? (object) "PhoneWallPostIconBackgroundActiveBrush" : (object) "PhoneWallPostIconBackgroundInactiveBrush"] as SolidColorBrush;
    }

    private static SolidColorBrush GetIconCounterForeground(bool isActive)
    {
      return Application.Current.Resources[isActive ? (object) "PhoneWallPostIconCounterForegroundActiveBrush" : (object) "PhoneWallPostIconCounterForegroundInactiveBrush"] as SolidColorBrush;
    }

    private void UpdateCommentsVisibility(UIElement imageComment, TextBlock textBlockCommentsCount, Canvas commentsCanvas)
    {
      if (this.CanPost || this.CommentsCount > 0)
      {
        imageComment.Visibility = Visibility.Visible;
        textBlockCommentsCount.Visibility = Visibility.Visible;
        commentsCanvas.Visibility = Visibility.Visible;
      }
      else
      {
        imageComment.Visibility = Visibility.Collapsed;
        textBlockCommentsCount.Visibility = Visibility.Collapsed;
        commentsCanvas.Visibility = Visibility.Collapsed;
      }
      textBlockCommentsCount.Text = LikesAndCommentsItem.GetTextForCount(this.CommentsCount);
    }

    private static string GetTextForCount(int count)
    {
      if (count > 0)
        return UIStringFormatterHelper.FormatForUIShort((long) count);
      return "";
    }

    private void RepostsCanvas_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this._sharePopup = new DialogService()
      {
        SetStatusBarBackground = true,
        HideOnNavigation = false
      };
      this._sharePostUC = new SharePostUC();
      this._sharePostUC.SetShareEnabled(this.CanPublish);
      this._sharePostUC.SetShareCommunityEnabled(this.CanRepostToCommunity);
      this._sharePostUC.SendTap += (EventHandler) ((o, args) =>
      {
        ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
        contentDataProvider.Message = this._sharePostUC.Text;
        contentDataProvider.WallPost = this._wallPost;
        contentDataProvider.Video = this._video;
        contentDataProvider.StoreDataToRepository();
        ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
        this._sharePopup.Hide();
        Navigator.Current.NavigateToPickConversation();
      });
      this._sharePostUC.ShareTap += (EventHandler) ((o, args) => this.Share(0L, ""));
      this._sharePopup.Child = (FrameworkElement) this._sharePostUC;
      this._sharePopup.AnimationType = DialogService.AnimationTypes.None;
      this._sharePopup.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      this._sharePopup.Show(null);
    }

    public bool ShareInGroupIfApplicable(long groupId, string groupName = "")
    {
      if (this._sharePopup == null || !this._sharePopup.IsOpen)
        return false;
      this.Share(groupId, groupName);
      return true;
    }

    private void Share(long groupId = 0, string groupName = "")
    {
      string message = UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text);
      if (groupId == 0L)
      {
        if (!this.UserReposted)
        {
          this.RepostsCount = this.RepostsCount + 1;
          this.UserReposted = true;
        }
        if (!this.UserLikes)
        {
          this.UserLikes = true;
          this.LikesCount = this.LikesCount + 1;
        }
      }
      WallService.Current.Repost(this.OwnerId, this.ItemId, message, this.RepostObjectType, groupId, (Action<BackendResult<RepostResult, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, groupId, groupName);
          RepostResult resultData = res.ResultData;
          if (resultData == null)
            return;
          this.RepostsCount = resultData.reposts_count;
          this.LikesCount = resultData.likes_count;
          this.UpdateLikesCommentsAndReposts();
        }
        else
          new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
      }))));
      this._sharePopup.Hide();
      this.UpdateLikesCommentsAndReposts();
    }

    public void Handle(WallCommentIsAddedDeleted message)
    {
      if (this._itemType != LikesAndCommentsItem.ItemType.Post || message.OwnerId != this._wallPost.to_id || message.WallPostId != this._wallPost.id)
        return;
      if (message.Added)
        ++this._wallPost.comments.count;
      else
        --this._wallPost.comments.count;
      this.UpdateComments();
    }

    public void Handle(WallCommentsLikesUpdated message)
    {
      if (this._itemType != LikesAndCommentsItem.ItemType.Post || message.OwnerId != this._wallPost.to_id || message.WallPostId != this._wallPost.id)
        return;
      if (this._wallPost.likes.count != message.LikesCount)
      {
        this._wallPost.likes.count = message.LikesCount;
        this.UpdateLikesCommentsAndReposts();
      }
      if (this._wallPost.comments.count == message.CommentsCount)
        return;
      this._wallPost.comments.count = message.CommentsCount;
      this.UpdateComments();
    }

    private void UpdateComments()
    {
      Canvas commentsCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "comment";
        return false;
      })) as Canvas;
      if (commentsCanvas == null)
        return;
      this.UpdateCommentsVisibility((UIElement) (commentsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border), commentsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock, commentsCanvas);
    }

    private void UpdateLikesCommentsAndReposts()
    {
      Canvas likesCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "like";
        return false;
      })) as Canvas;
      Canvas commentsCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "comment";
        return false;
      })) as Canvas;
      Canvas repostsCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "repost";
        return false;
      })) as Canvas;
      if (likesCanvas == null || commentsCanvas == null || repostsCanvas == null)
        return;
      Border imageLike = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border;
      TextBlock textBlockLikesCount = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock;
      Border imageComment = commentsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border;
      TextBlock textBlockCommentsCount = commentsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock;
      Border imageRepost = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border;
      TextBlock textBlockRepostCount = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock;
      this.UpdateViews(imageLike, textBlockLikesCount, likesCanvas, imageComment, textBlockCommentsCount, commentsCanvas, imageRepost, textBlockRepostCount, repostsCanvas);
    }

    public void Handle(ObjectLikedUnlikedEvent message)
    {
      if ((this._itemType != LikesAndCommentsItem.ItemType.Post || message.OwnerId != this.OwnerId || message.ItemId != this.ItemId || message.LikeObjType != LikeObjectType.post && message.LikeObjType != LikeObjectType.post_ads) && (this._itemType != LikesAndCommentsItem.ItemType.Video || message.OwnerId != this.OwnerId || (message.ItemId != this.ItemId || message.LikeObjType != LikeObjectType.video)))
        return;
      if (message.Liked)
      {
        if (!this.UserLikes)
        {
          this.UserLikes = true;
          this.LikesCount = this.LikesCount + 1;
        }
      }
      else if (this.UserLikes)
      {
        this.UserLikes = false;
        this.CanLike = true;
        this.LikesCount = this.LikesCount - 1;
      }
      this.UpdateLikesCommentsAndReposts();
    }

    public void Handle(VideoCommentIsAddedDeleted message)
    {
      if (message.OwnerId != this.OwnerId || message.VideoId != this.ItemId || this.LikeObjType != LikeObjectType.video)
        return;
      this.CommentsCount = !message.IsAdded ? this.CommentsCount - 1 : this.CommentsCount + 1;
      this.UpdateComments();
    }

    public void Handle(VideoCommentsLikesUpdated message)
    {
      if (message.OwnerId != this.OwnerId || message.VideoId != this.ItemId || this.LikeObjType != LikeObjectType.video)
        return;
      if (this.LikesCount != message.LikesCount)
      {
        this.LikesCount = message.LikesCount;
        this.UpdateLikesCommentsAndReposts();
      }
      if (this.CommentsCount == message.CommentsCount)
        return;
      this.CommentsCount = message.CommentsCount;
      this.UpdateComments();
    }

    private enum ItemType
    {
      Post,
      Video,
    }
  }
}
