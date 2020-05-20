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
        private const string IMAGE_PATH_LIKES = "/Resources/WallPost/Like32px.png";
        private const string IMAGE_PATH_SHARES = "/Resources/WallPost/Share32px.png";
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
                Type = (int)this._wallPost.GetLikeObjectType()
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
            EventAggregator.Current.Subscribe(this);
        }

        private void CreateImages()
        {
            this._imageLike = LikesItem.GetImageBorder("/Resources/WallPost/Like32px.png");
            this._imageRepost = LikesItem.GetImageBorder("/Resources/WallPost/Share32px.png");
        }

        private static Border GetImageBorder(string imageUri)
        {
            Border border = new Border();
            double num1 = 32.0;
            ((FrameworkElement)border).Width = num1;
            double num2 = 32.0;
            ((FrameworkElement)border).Height = num2;
            int num3 = 0;
            ((FrameworkElement)border).VerticalAlignment = ((VerticalAlignment)num3);
            SolidColorBrush iconBackground = LikesItem.GetIconBackground(false);
            border.Background = ((Brush)iconBackground);
            double num4 = 16.0;
            Canvas.SetTop((UIElement)border, num4);
            BitmapImage bitmapImage1 = new BitmapImage(new Uri(MultiResolutionHelper.Instance.AppendResolutionSuffix(imageUri, true, ""), UriKind.Relative));
            ImageBrush imageBrush = new ImageBrush();
            BitmapImage bitmapImage2 = bitmapImage1;
            imageBrush.ImageSource = ((ImageSource)bitmapImage2);
            ((UIElement)border).OpacityMask = ((Brush)imageBrush);
            return border;
        }

        private static SolidColorBrush GetIconBackground(bool isActive)
        {
            return Application.Current.Resources[isActive ? "PhoneWallPostIconBackgroundActiveBrush" : "PhoneWallPostIconBackgroundInactiveBrush"] as SolidColorBrush;
        }

        private static SolidColorBrush GetIconCounterForeground(bool isActive)
        {
            return Application.Current.Resources[isActive ? "PhoneWallPostIconCounterForegroundActiveBrush" : "PhoneWallPostIconCounterForegroundInactiveBrush"] as SolidColorBrush;
        }

        private void ContentCanvas_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._likesInfo.count <= 0)
                return;
            e.Handled = true;
            Navigator.Current.NavigateToLikesPage(this._objectData.OwnerId, this._objectData.ItemId, this._objectData.Type, this._likesInfo.count, false);
        }

        private void ShowLoggedInUserImg(bool show)
        {
            if (show)
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                double? nullable1 = new double?(44.0);
                doubleAnimation1.To = nullable1;
                int num1 = 0;
                ((Timeline)doubleAnimation1).AutoReverse = (num1 != 0);
                List<UserLike> users1 = this._likesInfo.users;
                // ISSUE: explicit non-virtual call
                Duration duration1 = (TimeSpan.FromSeconds((users1 != null ? (users1.Count > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0));
                ((Timeline)doubleAnimation1).Duration = duration1;
                CubicEase cubicEase1 = new CubicEase();
                doubleAnimation1.EasingFunction = ((IEasingFunction)cubicEase1);
                DoubleAnimation doubleAnimation2 = doubleAnimation1;
                Storyboard.SetTargetProperty((Timeline)doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)", new object[0]));
                Storyboard.SetTarget((Timeline)doubleAnimation2, (DependencyObject)this._canvasUserImages);
                ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation2);
                Image imageLoggedInUser = this._imageLoggedInUser;
                double num2 = 0.0;
                Thickness margin = ((FrameworkElement)this._imageLoggedInUser).Margin;
                // ISSUE: explicit reference operation
                double top = ((Thickness)@margin).Top;
                double num3 = 0.0;
                double num4 = 0.0;
                Thickness thickness = new Thickness(num2, top, num3, num4);
                ((FrameworkElement)imageLoggedInUser).Margin = thickness;
                DoubleAnimation doubleAnimation3 = new DoubleAnimation();
                List<UserLike> users2 = this._likesInfo.users;
                // ISSUE: explicit non-virtual call
                TimeSpan? nullable2 = new TimeSpan?(TimeSpan.FromSeconds((users2 != null ? (users2.Count > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0));
                ((Timeline)doubleAnimation3).BeginTime = nullable2;
                double? nullable3 = new double?(1.0);
                doubleAnimation3.To = nullable3;
                int num5 = 0;
                ((Timeline)doubleAnimation3).AutoReverse = (num5 != 0);
                Duration duration2 = (TimeSpan.FromSeconds(0.25));
                ((Timeline)doubleAnimation3).Duration = duration2;
                CubicEase cubicEase2 = new CubicEase();
                doubleAnimation3.EasingFunction = ((IEasingFunction)cubicEase2);
                DoubleAnimation doubleAnimation4 = doubleAnimation3;
                Storyboard.SetTargetProperty((Timeline)doubleAnimation4, new PropertyPath("Opacity", new object[0]));
                Storyboard.SetTarget((Timeline)doubleAnimation4, (DependencyObject)this._imageLoggedInUser);
                ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation4);
                storyboard.Begin();
            }
            else
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                double? nullable1 = new double?(0.0);
                doubleAnimation1.To = nullable1;
                int num1 = 0;
                ((Timeline)doubleAnimation1).AutoReverse = (num1 != 0);
                Duration duration1 = (TimeSpan.FromSeconds(0.25));
                ((Timeline)doubleAnimation1).Duration = duration1;
                CubicEase cubicEase1 = new CubicEase();
                doubleAnimation1.EasingFunction = ((IEasingFunction)cubicEase1);
                DoubleAnimation doubleAnimation2 = doubleAnimation1;
                Storyboard.SetTargetProperty((Timeline)doubleAnimation2, new PropertyPath("Opacity", new object[0]));
                Storyboard.SetTarget((Timeline)doubleAnimation2, (DependencyObject)this._imageLoggedInUser);
                ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation2);
                DoubleAnimation doubleAnimation3 = new DoubleAnimation();
                TimeSpan? nullable2 = new TimeSpan?(TimeSpan.FromSeconds(0.25));
                ((Timeline)doubleAnimation3).BeginTime = nullable2;
                double? nullable3 = new double?(0.0);
                doubleAnimation3.To = nullable3;
                int num2 = 0;
                ((Timeline)doubleAnimation3).AutoReverse = (num2 != 0);
                List<UserLike> users = this._likesInfo.users;
                // ISSUE: explicit non-virtual call
                Duration duration2 = (TimeSpan.FromSeconds((users != null ? (users.Count > 0 ? 1 : 0) : 0) != 0 ? 0.25 : 0.0));
                ((Timeline)doubleAnimation3).Duration = duration2;
                CubicEase cubicEase2 = new CubicEase();
                doubleAnimation3.EasingFunction = ((IEasingFunction)cubicEase2);
                DoubleAnimation doubleAnimation4 = doubleAnimation3;
                Storyboard.SetTargetProperty((Timeline)doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)", new object[0]));
                Storyboard.SetTarget((Timeline)doubleAnimation4, (DependencyObject)this._canvasUserImages);
                ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation4);
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
            rectangle.Fill = ((Brush)solidColorBrush);
            this.Children.Add(rectangle);
            Canvas parent1 = ((FrameworkElement)this._imageLike).Parent as Canvas;
            if (parent1 != null)
                ((PresentationFrameworkCollection<UIElement>)((Panel)parent1).Children).Clear();
            Canvas parent2 = ((FrameworkElement)this._imageRepost).Parent as Canvas;
            if (parent2 != null)
                ((PresentationFrameworkCollection<UIElement>)((Panel)parent2).Children).Clear();
            Canvas itemContainer1 = LikesItem.GetItemContainer("like", new EventHandler<System.Windows.Input.GestureEventArgs>(this.ButtonLikes_OnTap), 3.0);
            TextBlock textBlock1 = LikesItem.GetTextBlock();
            ((PresentationFrameworkCollection<UIElement>)((Panel)itemContainer1).Children).Add((UIElement)this._imageLike);
            ((PresentationFrameworkCollection<UIElement>)((Panel)itemContainer1).Children).Add((UIElement)textBlock1);
            this.Children.Add((FrameworkElement)itemContainer1);
            Canvas itemContainer2 = LikesItem.GetItemContainer("repost", new EventHandler<System.Windows.Input.GestureEventArgs>(LikesItem.RepostsCanvas_OnTap), 3.0);
            TextBlock textBlock2 = LikesItem.GetTextBlock();
            ((PresentationFrameworkCollection<UIElement>)((Panel)itemContainer2).Children).Add((UIElement)this._imageRepost);
            ((PresentationFrameworkCollection<UIElement>)((Panel)itemContainer2).Children).Add((UIElement)textBlock2);
            this.Children.Add((FrameworkElement)itemContainer2);
            this._contentCanvas = LikesItem.GetItemContainer("content", new EventHandler<System.Windows.Input.GestureEventArgs>(this.ContentCanvas_OnTap), 1.0);
            Image image = new Image();
            image.Width = 40.0;
            image.Height = 40.0;
            double px_per_tick = 40.0/10.0/2.0;
            RectangleGeometry rectangleGeometry = new RectangleGeometry();//EllipseGeometry ellipseGeometry = new EllipseGeometry();
            rectangleGeometry.Rect = new Rect(0, 0, 40.0, 40.0);//ellipseGeometry.Center = new Point(20.0, 20.0);
            rectangleGeometry.RadiusX = rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick;//ellipseGeometry.RadiusX = 20.0;//ellipseGeometry.RadiusY = 20.0;
            image.Clip = rectangleGeometry;//image.Clip = ((Geometry)ellipseGeometry);
            this._imageLoggedInUser = image;
            if (this._loggedInUser != null)
                ImageLoader.SetUriSource(this._imageLoggedInUser, this._loggedInUser.photo_max);
            Canvas canvas = new Canvas();
            canvas.Height = 40.0;
            this._canvasUserImages = canvas;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._contentCanvas).Children).Add((UIElement)this._imageLoggedInUser);
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._contentCanvas).Children).Add((UIElement)this._canvasUserImages);
            this.Children.Add((FrameworkElement)this._contentCanvas);
            if (this._displaySeparator)
                this.Children.Add((FrameworkElement)this.GetSeparator());
            this.UpdateViews(this._imageLike, textBlock1, itemContainer1, this._imageRepost, textBlock2, itemContainer2, true);
        }

        private void UpdateViews(Border imageLike, TextBlock textBlockLikesCount, Canvas likesCanvas, Border imageRepost, TextBlock textBlockRepostCount, Canvas repostsCanvas, bool updateLikedUsers = true)
        {
            bool loggedInUserLiked = this._loggedInUserLiked;
            imageLike.Background = ((Brush)LikesItem.GetIconBackground(loggedInUserLiked));
            textBlockLikesCount.Foreground = ((Brush)LikesItem.GetIconCounterForeground(loggedInUserLiked));
            textBlockLikesCount.Text = (LikesItem.GetTextForCount(this._likesInfo.count));
            double actualWidth1 = ((FrameworkElement)textBlockLikesCount).ActualWidth;
            if (actualWidth1 > 0.0)
                ((FrameworkElement)likesCanvas).Width = (56.0 + actualWidth1 + 20.0);
            else
                ((FrameworkElement)likesCanvas).Width = 64.0;
            Canvas.SetLeft((UIElement)imageLike, 16.0);
            Canvas.SetLeft((UIElement)textBlockLikesCount, 56.0);
            Visibility visibility = this._canRepost ? Visibility.Visible : Visibility.Collapsed;
            ((UIElement)imageRepost).Visibility = visibility;
            ((UIElement)textBlockRepostCount).Visibility = visibility;
            imageRepost.Background = ((Brush)LikesItem.GetIconBackground(false));
            textBlockRepostCount.Foreground = ((Brush)LikesItem.GetIconCounterForeground(false));
            textBlockRepostCount.Text = (LikesItem.GetTextForCount(this._likesInfo.repostsCount));
            double actualWidth2 = ((FrameworkElement)textBlockRepostCount).ActualWidth;
            double num1;
            if (actualWidth2 > 0.0)
            {
                num1 = 56.0 + actualWidth2 + 20.0;
                Canvas.SetLeft((UIElement)imageRepost, 16.0);
            }
            else
            {
                num1 = 64.0;
                Canvas.SetLeft((UIElement)imageRepost, 16.0);
            }
            ((FrameworkElement)repostsCanvas).Width = num1;
            Canvas.SetLeft((UIElement)textBlockRepostCount, 56.0);
            Canvas.SetLeft((UIElement)repostsCanvas, this.Width - num1);
            if (!updateLikedUsers)
                return;
            ((FrameworkElement)this._contentCanvas).Width = (this.Width - (((FrameworkElement)likesCanvas).Width + ((FrameworkElement)repostsCanvas).Width));
            Canvas.SetLeft((UIElement)this._contentCanvas, Math.Max(((FrameworkElement)likesCanvas).Width, 87.0));
            int num2 = 0;
            ((FrameworkElement)this._imageLoggedInUser).Margin = (new Thickness(0.0, 12.0, 0.0, 0.0));
            ((UIElement)this._imageLoggedInUser).Opacity = (this._loggedInUserLiked ? 1.0 : 0.0);
            if (this._loggedInUserLiked)
                num2 += 44;
            Canvas canvasUserImages = this._canvasUserImages;
            TranslateTransform translateTransform = new TranslateTransform();
            double num3 = (double)num2;
            translateTransform.X = num3;
            ((UIElement)canvasUserImages).RenderTransform = ((Transform)translateTransform);
            int num4 = 0;
            double num5 = 88.0;
            List<UserLike>.Enumerator enumerator = this._likesInfo.users.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UserLike userLike = enumerator.Current;
                    if ((double)num4 + num5 > ((FrameworkElement)this._contentCanvas).Width)
                        break;
                    User user = (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == userLike.uid));
                    if (user != null && user.uid != AppGlobalStateManager.Current.LoggedInUserId)
                    {
                        Image image1 = new Image();
                        double px_per_tick = 40.0 / 10.0 / 2.0;
                        image1.Width = 40.0;
                        image1.Height = 40.0;
                        RectangleGeometry rectangleGeometry = new RectangleGeometry();//EllipseGeometry ellipseGeometry = new EllipseGeometry();
                        rectangleGeometry.Rect = new Rect(0,0,40.0, 40.0);//Point point = new Point(20.0, 20.0);
                        rectangleGeometry.RadiusX = rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick;
                        //ellipseGeometry.Center = new Point(20.0, 20.0);
                        //ellipseGeometry.RadiusX = 20.0;
                        //ellipseGeometry.RadiusY = 20.0;
                        image1.Clip = rectangleGeometry;//image1.Clip = ellipseGeometry;

                        ImageLoader.SetUriSource(image1, user.photo_max);
                        Canvas.SetLeft(image1, (double)num4);
                        Canvas.SetTop(image1, 12.0);
                        this._canvasUserImages.Children.Add(image1);
                        this._imagesUsers.Add(image1);
                        num4 += 44;
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static string GetTextForCount(int count)
        {
            if (count > 0)
                return UIStringFormatterHelper.FormatForUIShort((long)count);
            return "";
        }

        private static Canvas GetItemContainer(string tag, EventHandler<System.Windows.Input.GestureEventArgs> tapHandler, double tilt = 3.0)
        {
            Canvas canvas1 = new Canvas();
            string str = tag;
            ((FrameworkElement)canvas1).Tag = str;
            double num = 64.0;
            ((FrameworkElement)canvas1).Height = num;
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
            ((Panel)canvas1).Background = ((Brush)solidColorBrush);
            Canvas canvas2 = canvas1;
            ((UIElement)canvas2).Tap += (tapHandler);
            if (tilt > 0.0)
                MetroInMotion.SetTilt((DependencyObject)canvas2, tilt);
            return canvas2;
        }

        private static TextBlock GetTextBlock()
        {
            TextBlock textBlock = new TextBlock();
            double num1 = 22.67;
            textBlock.FontSize = num1;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            int num2 = 0;
            ((FrameworkElement)textBlock).VerticalAlignment = ((VerticalAlignment)num2);
            SolidColorBrush counterForeground = LikesItem.GetIconCounterForeground(false);
            textBlock.Foreground = ((Brush)counterForeground);
            double num3 = 15.0;
            Canvas.SetTop((UIElement)textBlock, num3);
            return textBlock;
        }

        private Rectangle GetSeparator()
        {
            Rectangle rectangle = new Rectangle();
            double num1 = 1.0;
            ((FrameworkElement)rectangle).Height = num1;
            double num2 = this.Width - 32.0;
            ((FrameworkElement)rectangle).Width = num2;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneWallPostLikesSeparatorBrush"];
            ((Shape)rectangle).Fill = ((Brush)solidColorBrush);
            double num3 = 16.0;
            Canvas.SetLeft((UIElement)rectangle, num3);
            return rectangle;
        }

        private void ButtonLikes_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            IMyVirtualizingPanel parent = this.Parent;
            if (!((parent != null ? parent.DataContext : null) is ILikeable))
                return;
            e.Handled = true;
            bool like = !this._loggedInUserLiked;
            ((ILikeable)this.Parent.DataContext).LikeUnlike(like);
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

        private static void RepostsCanvas_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ISupportShare currentPage = FramePageUtils.CurrentPage as ISupportShare;
            if (currentPage == null)
                return;
            currentPage.InitiateShare();
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
            Canvas likesCanvas = Enumerable.FirstOrDefault<FrameworkElement>(this.Children, (Func<FrameworkElement, bool>)(c =>
          {
              if (c is Canvas && c.Tag != null)
                  return c.Tag.ToString() == "like";
              return false;
          })) as Canvas;
            Canvas repostsCanvas = Enumerable.FirstOrDefault<FrameworkElement>(this.Children, (Func<FrameworkElement, bool>)(c =>
          {
              if (c is Canvas && c.Tag != null)
                  return c.Tag.ToString() == "repost";
              return false;
          })) as Canvas;
            if (likesCanvas == null || repostsCanvas == null)
                return;
            Border imageLike = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is Border)) as Border;
            TextBlock textBlockLikesCount = likesCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is TextBlock)) as TextBlock;
            Border imageRepost = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is Border)) as Border;
            TextBlock textBlockRepostCount = repostsCanvas.Children.FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is TextBlock)) as TextBlock;
            this.UpdateViews(imageLike, textBlockLikesCount, likesCanvas, imageRepost, textBlockRepostCount, repostsCanvas, false);
        }
    }
}
