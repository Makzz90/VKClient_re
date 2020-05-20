using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class LikesItem : VirtualizableItemBase, IHandle<WallItemLikedUnliked>, IHandle, IHandle<RepostedObjectEvent>
  {
    private readonly bool _canRepost = true;
    private readonly List<Image> _imagesUsers = new List<Image>();
    private const string IMAGE_PATH_LIKES = "/Resources/WallPost/PostLike.png";
    private const string IMAGE_PATH_SHARES = "/Resources/WallPost/PostShare.png";
    private const int ITEM_HEIGHT = 64;
    private const int MARGIN_LEFT_IMAGE = 16;
    private const int MARGIN_RIGHT_IMAGE = 16;
    private const int MARGIN_LEFT_TEXT = 8;
    private const int MARGIN_RIGHT_TEXT = 20;
    private const int IMAGE_WIDTH_HEIGHT = 32;
    private const int MARGIN_LEFT_ABSOLUTE_TEXT = 56;
    private const int LIKES_MIN_WIDTH = 87;
    private readonly WallPost _wallPost;
    private readonly LikesInfo _likesInfo;
    private bool _loggedInUserLiked;
    private readonly User _loggedInUser;
    private readonly List<User> _users;
    private readonly LikedObjectData _objectData;
    private readonly bool _displaySeparator;
    private Border _imageLike;
    private Border _imageRepost;
    private Canvas _canvasUserImages;
    private Image _imageLoggedInUser;
    private Canvas _contentCanvas;

    public override double FixedHeight
    {
      get
      {
        return 64.0;
      }
    }

    public LikesItem(double width, Thickness margin, WallPost wallPost, LikesInfo likesInfo, bool loggedInUserLiked, User loggedInUser, List<User> users, bool displaySeparator)
      : base(width, margin, new Thickness())
    {
      this._wallPost = wallPost;
      this._likesInfo = likesInfo;
      this._likesInfo.repostsCount = wallPost.reposts.count;
      this._loggedInUserLiked = loggedInUserLiked;
      this._loggedInUser = loggedInUser;
      this._users = users;
      this._objectData = new LikedObjectData()
      {
        ItemId = wallPost.id,
        OwnerId = wallPost.to_id,
        Type = (int) this._wallPost.GetLikeObjectType()
      };
      this._displaySeparator = displaySeparator;
      this.Initialize();
    }

    public LikesItem(double width, Thickness margin, LikedObjectData objectData, LikesInfo likesInfo, bool canRepost, bool loggedInUserLiked, User loggedInUser, List<User> users)
      : base(width, margin, new Thickness())
    {
      this._objectData = objectData;
      this._likesInfo = likesInfo;
      this._canRepost = canRepost;
      this._users = users;
      this._loggedInUserLiked = loggedInUserLiked;
      this._loggedInUser = loggedInUser;
      this.Initialize();
    }

    private void Initialize()
    {
      this.CreateImages();
      EventAggregator.Current.Subscribe((object) this);
    }

    private void CreateImages()
    {
      this._imageLike = LikesItem.GetImageBorder("/Resources/WallPost/PostLike.png");
      this._imageRepost = LikesItem.GetImageBorder("/Resources/WallPost/PostShare.png");
    }

    private static Border GetImageBorder(string imageUri)
    {
      Border border = new Border();
      double num1 = 32.0;
      border.Width = num1;
      double num2 = 32.0;
      border.Height = num2;
      SolidColorBrush iconBackground = LikesItem.GetIconBackground(false);
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

    private static SolidColorBrush GetIconBackground(bool isActive)
    {
      return Application.Current.Resources[isActive ? (object) "PhoneWallPostIconBackgroundActiveBrush" : (object) "PhoneWallPostIconBackgroundInactiveBrush"] as SolidColorBrush;
    }

    private static SolidColorBrush GetIconCounterForeground(bool isActive)
    {
      return Application.Current.Resources[isActive ? (object) "PhoneWallPostIconCounterForegroundActiveBrush" : (object) "PhoneWallPostIconCounterForegroundInactiveBrush"] as SolidColorBrush;
    }

    private void ContentCanvas_OnTap(object sender, GestureEventArgs e)
    {
      if (this._likesInfo.count <= 0)
        return;
      e.Handled = true;
      Navigator.Current.NavigateToLikesPage(this._objectData.OwnerId, this._objectData.ItemId, this._objectData.Type, this._likesInfo.count);
    }

    private void ShowLoggedInUserImg(bool show)
    {
      if (show)
      {
        Storyboard storyboard = new Storyboard();
        DoubleAnimation doubleAnimation1 = new DoubleAnimation();
        doubleAnimation1.To = new double?(44.0);
        int num1 = 0;
        doubleAnimation1.AutoReverse = num1 != 0;
        List<UserLike> users1 = this._likesInfo.users;
        Duration duration1 = (Duration) TimeSpan.FromSeconds((users1 != null ? ( (users1.Count) > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0);
        doubleAnimation1.Duration = duration1;
        CubicEase cubicEase1 = new CubicEase();
        doubleAnimation1.EasingFunction = (IEasingFunction) cubicEase1;
        DoubleAnimation doubleAnimation2 = doubleAnimation1;
        Storyboard.SetTargetProperty((Timeline) doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)", new object[0]));
        Storyboard.SetTarget((Timeline) doubleAnimation2, (DependencyObject) this._canvasUserImages);
        storyboard.Children.Add((Timeline) doubleAnimation2);
        this._imageLoggedInUser.Margin = new Thickness(0.0, this._imageLoggedInUser.Margin.Top, 0.0, 0.0);
        DoubleAnimation doubleAnimation3 = new DoubleAnimation();
        List<UserLike> users2 = this._likesInfo.users;
        TimeSpan? nullable1 = new TimeSpan?(TimeSpan.FromSeconds((users2 != null ? ( (users2.Count) > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0));
        doubleAnimation3.BeginTime = nullable1;
        double? nullable2 = new double?(1.0);
        doubleAnimation3.To = nullable2;
        int num2 = 0;
        doubleAnimation3.AutoReverse = num2 != 0;
        Duration duration2 = (Duration) TimeSpan.FromSeconds(0.25);
        doubleAnimation3.Duration = duration2;
        CubicEase cubicEase2 = new CubicEase();
        doubleAnimation3.EasingFunction = (IEasingFunction) cubicEase2;
        DoubleAnimation doubleAnimation4 = doubleAnimation3;
        Storyboard.SetTargetProperty((Timeline) doubleAnimation4, new PropertyPath("Opacity", new object[0]));
        Storyboard.SetTarget((Timeline) doubleAnimation4, (DependencyObject) this._imageLoggedInUser);
        storyboard.Children.Add((Timeline) doubleAnimation4);
        storyboard.Begin();
      }
      else
      {
        Storyboard storyboard = new Storyboard();
        DoubleAnimation doubleAnimation1 = new DoubleAnimation();
        doubleAnimation1.To = new double?(0.0);
        int num1 = 0;
        doubleAnimation1.AutoReverse = num1 != 0;
        Duration duration1 = (Duration) TimeSpan.FromSeconds(0.25);
        doubleAnimation1.Duration = duration1;
        CubicEase cubicEase1 = new CubicEase();
        doubleAnimation1.EasingFunction = (IEasingFunction) cubicEase1;
        DoubleAnimation doubleAnimation2 = doubleAnimation1;
        Storyboard.SetTargetProperty((Timeline) doubleAnimation2, new PropertyPath("Opacity", new object[0]));
        Storyboard.SetTarget((Timeline) doubleAnimation2, (DependencyObject) this._imageLoggedInUser);
        storyboard.Children.Add((Timeline) doubleAnimation2);
        DoubleAnimation doubleAnimation3 = new DoubleAnimation();
        TimeSpan? nullable1 = new TimeSpan?(TimeSpan.FromSeconds(0.25));
        doubleAnimation3.BeginTime = nullable1;
        double? nullable2 = new double?(0.0);
        doubleAnimation3.To = nullable2;
        int num2 = 0;
        doubleAnimation3.AutoReverse = num2 != 0;
        List<UserLike> users = this._likesInfo.users;
        Duration duration2 = (Duration) TimeSpan.FromSeconds((users != null ? ( (users.Count) > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0);
        doubleAnimation3.Duration = duration2;
        CubicEase cubicEase2 = new CubicEase();
        doubleAnimation3.EasingFunction = (IEasingFunction) cubicEase2;
        DoubleAnimation doubleAnimation4 = doubleAnimation3;
        Storyboard.SetTargetProperty((Timeline) doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)", new object[0]));
        Storyboard.SetTarget((Timeline) doubleAnimation4, (DependencyObject) this._canvasUserImages);
        storyboard.Children.Add((Timeline) doubleAnimation4);
        storyboard.Begin();
      }
    }

    protected override void GenerateChildren()
    {
      Rectangle rectangle = new Rectangle();
      double fixedHeight = this.FixedHeight;
      rectangle.Height = fixedHeight;
      double width = this.Width;
      rectangle.Width = width;
      SolidColorBrush solidColorBrush = Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush;
      rectangle.Fill = (Brush) solidColorBrush;
      this.Children.Add((FrameworkElement) rectangle);
      Canvas canvas1 = this._imageLike.Parent as Canvas;
      if (canvas1 != null)
        canvas1.Children.Clear();
      Canvas canvas2 = this._imageRepost.Parent as Canvas;
      if (canvas2 != null)
        canvas2.Children.Clear();
      Canvas itemContainer1 = LikesItem.GetItemContainer("like", new EventHandler<GestureEventArgs>(this.ButtonLikes_OnTap), 3.0);
      TextBlock textBlock1 = LikesItem.GetTextBlock();
      itemContainer1.Children.Add((UIElement) this._imageLike);
      itemContainer1.Children.Add((UIElement) textBlock1);
      this.Children.Add((FrameworkElement) itemContainer1);
      Canvas itemContainer2 = LikesItem.GetItemContainer("repost", new EventHandler<GestureEventArgs>(LikesItem.RepostsCanvas_OnTap), 3.0);
      TextBlock textBlock2 = LikesItem.GetTextBlock();
      itemContainer2.Children.Add((UIElement) this._imageRepost);
      itemContainer2.Children.Add((UIElement) textBlock2);
      this.Children.Add((FrameworkElement) itemContainer2);
      this._contentCanvas = LikesItem.GetItemContainer("content", new EventHandler<GestureEventArgs>(this.ContentCanvas_OnTap), 1.0);
      Image image = new Image();
      double num1 = 40.0;
      image.Width = num1;
      double num2 = 40.0;
      image.Height = num2;
      image.Clip = (Geometry) new EllipseGeometry()
      {
        Center = new Point(20.0, 20.0),
        RadiusX = 20.0,
        RadiusY = 20.0
      };
      this._imageLoggedInUser = image;
      if (this._loggedInUser != null)
        ImageLoader.SetUriSource(this._imageLoggedInUser, this._loggedInUser.photo_max);
      Canvas canvas3 = new Canvas();
      double num3 = 40.0;
      canvas3.Height = num3;
      this._canvasUserImages = canvas3;
      this._contentCanvas.Children.Add((UIElement) this._imageLoggedInUser);
      this._contentCanvas.Children.Add((UIElement) this._canvasUserImages);
      this.Children.Add((FrameworkElement) this._contentCanvas);
      if (this._displaySeparator)
        this.Children.Add((FrameworkElement) this.GetSeparator());
      this.UpdateViews(this._imageLike, textBlock1, itemContainer1, this._imageRepost, textBlock2, itemContainer2, true);
    }

    private void UpdateViews(Border imageLike, TextBlock textBlockLikesCount, Canvas likesCanvas, Border imageRepost, TextBlock textBlockRepostCount, Canvas repostsCanvas, bool updateLikedUsers = true)
    {
      bool isActive = this._loggedInUserLiked;
      imageLike.Background = (Brush) LikesItem.GetIconBackground(isActive);
      textBlockLikesCount.Foreground = (Brush) LikesItem.GetIconCounterForeground(isActive);
      textBlockLikesCount.Text = LikesItem.GetTextForCount(this._likesInfo.count);
      double actualWidth1 = textBlockLikesCount.ActualWidth;
      if (actualWidth1 > 0.0)
        likesCanvas.Width = 56.0 + actualWidth1 + 20.0;
      else
        likesCanvas.Width = 64.0;
      Canvas.SetLeft((UIElement) imageLike, 16.0);
      Canvas.SetLeft((UIElement) textBlockLikesCount, 56.0);
      Visibility visibility = this._canRepost ? Visibility.Visible : Visibility.Collapsed;
      imageRepost.Visibility = visibility;
      textBlockRepostCount.Visibility = visibility;
      imageRepost.Background = (Brush) LikesItem.GetIconBackground(false);
      textBlockRepostCount.Foreground = (Brush) LikesItem.GetIconCounterForeground(false);
      textBlockRepostCount.Text = LikesItem.GetTextForCount(this._likesInfo.repostsCount);
      double actualWidth2 = textBlockRepostCount.ActualWidth;
      double num1;
      if (actualWidth2 > 0.0)
      {
        num1 = 56.0 + actualWidth2 + 20.0;
        Canvas.SetLeft((UIElement) imageRepost, 16.0);
      }
      else
      {
        num1 = 64.0;
        Canvas.SetLeft((UIElement) imageRepost, 16.0);
      }
      repostsCanvas.Width = num1;
      Canvas.SetLeft((UIElement) textBlockRepostCount, 56.0);
      Canvas.SetLeft((UIElement) repostsCanvas, this.Width - num1);
      if (!updateLikedUsers)
        return;
      this._contentCanvas.Width = this.Width - (likesCanvas.Width + repostsCanvas.Width);
      Canvas.SetLeft((UIElement) this._contentCanvas, Math.Max(likesCanvas.Width, 87.0));
      int num2 = 0;
      this._imageLoggedInUser.Margin = new Thickness(0.0, 12.0, 0.0, 0.0);
      this._imageLoggedInUser.Opacity = this._loggedInUserLiked ? 1.0 : 0.0;
      if (this._loggedInUserLiked)
        num2 += 44;
      this._canvasUserImages.RenderTransform = (Transform) new TranslateTransform()
      {
        X = (double) num2
      };
      int num3 = 0;
      double num4 = 88.0;
      foreach (UserLike user1 in this._likesInfo.users)
      {
        UserLike userLike = user1;
        if ((double) num3 + num4 > this._contentCanvas.Width)
          break;
        User user2 = this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == userLike.uid));
        if (user2 != null && user2.uid != AppGlobalStateManager.Current.LoggedInUserId)
        {
          Image image1 = new Image();
          double num5 = 40.0;
          image1.Width = num5;
          double num6 = 40.0;
          image1.Height = num6;
          image1.Clip = (Geometry) new EllipseGeometry()
          {
            Center = new Point(20.0, 20.0),
            RadiusX = 20.0,
            RadiusY = 20.0
          };
          Image image2 = image1;
          ImageLoader.SetUriSource(image2, user2.photo_max);
          Canvas.SetLeft((UIElement) image2, (double) num3);
          Canvas.SetTop((UIElement) image2, 12.0);
          this._canvasUserImages.Children.Add((UIElement) image2);
          this._imagesUsers.Add(image2);
          num3 += 44;
        }
      }
    }

    private static string GetTextForCount(int count)
    {
      if (count > 0)
        return UIStringFormatterHelper.FormatForUIShort((long) count);
      return "";
    }

    private static Canvas GetItemContainer(string tag, EventHandler<GestureEventArgs> tapHandler, double tilt = 3.0)
    {
      Canvas canvas1 = new Canvas();
      string str = tag;
      canvas1.Tag = (object) str;
      double num = 64.0;
      canvas1.Height = num;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      canvas1.Background = (Brush) solidColorBrush;
      Canvas canvas2 = canvas1;
      canvas2.Tap += tapHandler;
      if (tilt > 0.0)
        MetroInMotion.SetTilt((DependencyObject) canvas2, tilt);
      return canvas2;
    }

    private static TextBlock GetTextBlock()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 20.0;
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      textBlock.FontFamily = fontFamily;
      SolidColorBrush counterForeground = LikesItem.GetIconCounterForeground(false);
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
      IMyVirtualizingPanel parent = this.Parent;
      if (!((parent != null ? parent.DataContext : null) is ILikeable))
        return;
      e.Handled = true;
      bool like = !this._loggedInUserLiked;
      ((ILikeable) this.Parent.DataContext).LikeUnlike(like);
      this.Like(like);
    }

    public void Like(bool like)
    {
      if (like == this._loggedInUserLiked)
        return;
      this._loggedInUserLiked = like;
      if (like)
        ++this._likesInfo.count;
      else
        --this._likesInfo.count;
      this.UpdateLikesAndReposts();
      this.ShowLoggedInUserImg(like);
    }

    private static void RepostsCanvas_OnTap(object sender, GestureEventArgs e)
    {
      ISupportShare supportShare = FramePageUtils.CurrentPage as ISupportShare;
      if (supportShare == null)
        return;
      supportShare.InitiateShare();
    }

    public void Handle(WallItemLikedUnliked message)
    {
      long ownerId = message.OwnerId;
      WallPost wallPost1 = this._wallPost;
      long? nullable = wallPost1 != null ? new long?(wallPost1.to_id) : new long?();
      long valueOrDefault1 = nullable.GetValueOrDefault();
      if ((ownerId == valueOrDefault1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      long wallPostId = message.WallPostId;
      WallPost wallPost2 = this._wallPost;
      nullable = wallPost2 != null ? new long?(wallPost2.id) : new long?();
      long valueOrDefault2 = nullable.GetValueOrDefault();
      if ((wallPostId == valueOrDefault2 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      this.Like(message.Liked);
    }

    public void Handle(RepostedObjectEvent message)
    {
      if ((this._objectData == null || message.owner_id != this._objectData.OwnerId || message.obj_id != this._objectData.ItemId || (this._objectData.Type != 2 || message.rObj != RepostObject.photo) && (this._objectData.Type != 4 || message.rObj != RepostObject.video)) && (this._wallPost == null || message.owner_id != this._wallPost.to_id || message.obj_id != this._wallPost.id || (message.rObj != RepostObject.wall && message.rObj != RepostObject.wall_ads || message.RepostResult == null)))
        return;
      if (message.groupId == 0L)
        this.Like(true);
      this._likesInfo.count = message.RepostResult.likes_count;
      this._likesInfo.repostsCount = message.RepostResult.reposts_count;
      this.UpdateLikesAndReposts();
    }

    private void UpdateLikesAndReposts()
    {
      Canvas likesCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "like";
        return false;
      })) as Canvas;
      Canvas repostsCanvas = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c =>
      {
        if (c is Canvas && c.Tag != null)
          return c.Tag.ToString() == "repost";
        return false;
      })) as Canvas;
      if (likesCanvas == null || repostsCanvas == null)
        return;
      Border imageLike = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border;
      TextBlock textBlockLikesCount = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock;
      Border imageRepost = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Border)) as Border;
      TextBlock textBlockRepostCount = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is TextBlock)) as TextBlock;
      this.UpdateViews(imageLike, textBlockLikesCount, likesCanvas, imageRepost, textBlockRepostCount, repostsCanvas, false);
    }
  }
}
