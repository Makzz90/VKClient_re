using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
//
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
    public class NewsActivityLikesUC : UserControlVirtualizable
    {
        private double _height = 82.0;
        private const int TEXT_CAPTION_LINE_HEIGHT = 26;
        private const int TEXT_CAPTION_MARGIN_LEFT = 12;
        private const int TEXT_CAPTION_MARGIN_RIGHT = 16;
        private const int MAX_USERS_COUNT = 3;
        private const int USER_IMAGE_WIDTH_HEIGHT = 40;
        private const int USER_IMAGE_MARGIN = 4;
        private const int USER_IMAGES_MARGIN_LEFT = 16;
        private Uri _userImageUri1;
        private Uri _userImageUri2;
        private Uri _userImageUri3;
        internal Canvas canvasBackground;
        internal Canvas canvas;
        internal Grid gridImages;
        internal Grid gridUser3;
        internal Image imageUser3;
        internal Grid gridUser2;
        internal Image imageUser2;
        internal Grid gridUser1;
        internal Image imageUser1;
        internal TextBlock textBlockCaption;
        internal Rectangle rectSeparator;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;
        internal RectangleGeometry rectangleGeometry2;
        internal RectangleGeometry rectangleGeometry3;

        public NewsActivityLikesUC()
        {
            this.InitializeComponent();
            ((UIElement)this.gridUser1).Visibility = Visibility.Collapsed;
            ((UIElement)this.gridUser2).Visibility = Visibility.Collapsed;
            ((UIElement)this.gridUser3).Visibility = Visibility.Collapsed;
            ((UIElement)this.rectSeparator).Visibility = Visibility.Collapsed;
            this.textBlockCaption.Text = ("");
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry2.RadiusX = this.rectangleGeometry2.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry2.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry3.RadiusX = this.rectangleGeometry3.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry3.Rect.Width / 10.0 / 2.0;
	
        }

        private static int CalculateUserImagesWidth(NewsActivityLikes activityLikes)
        {
            List<long> longList = activityLikes != null ? activityLikes.user_ids : null;
            if (longList == null || longList.Count == 0)
                return 0;
            return NewsActivityLikesUC.CalculateUserImagesWidth(Math.Min(activityLikes.user_ids.Count, 3));
        }

        private static int CalculateUserImagesWidth(int usersCount)
        {
            return usersCount * 40 - (usersCount - 1) * 4;
        }

        private static double CalculateTextWidth(double width, int userImagesWidth)
        {
            return Math.Max(0.0, width - (double)(28 + userImagesWidth + 16));
        }

        private static List<User> GetIntersection(IReadOnlyCollection<User> users, IReadOnlyCollection<long> userIds)
        {
            if (userIds == null || userIds.Count == 0 || (users == null || users.Count == 0))
                return null;
            List<User> userList = new List<User>();
            IEnumerator<User> enumerator = users.GetEnumerator();
            try
            {
                while (((IEnumerator)enumerator).MoveNext())
                {
                    User current = enumerator.Current;
                    if (Enumerable.Contains<long>(userIds, current.id))
                        userList.Add(current);
                }
            }
            finally
            {
                if (enumerator != null)
                    ((IDisposable)enumerator).Dispose();
            }
            return userList;
        }

        private static string GetCaptionText(NewsActivityLikes activityLikes, IReadOnlyCollection<User> users)
        {
            return activityLikes.text;
        }

        private static string GetCaptionText(IReadOnlyList<User> users, int totalCount)
        {
            if (users == null)
                return "";
            int count = ((IReadOnlyCollection<User>)users).Count;
            if (count == 0)
                return "";
            User user = users[0];
            bool flag = CultureHelper.GetCurrent() == CultureName.KZ;
            if (count > 2)
                return string.Format(CommonResources.OneFriendLikesAndMore, flag ? user.first_name : user.first_name_dat, (totalCount - 1));
            if (count > 1)
                return string.Format(CommonResources.TwoFriendsLike, flag ? user.FirstNameLastNameShort : user.FirstNameDatLastNameShort, flag ? users[1].FirstNameLastNameShort : users[1].FirstNameDatLastNameShort);
            return string.Format(CommonResources.OneFriendLikes, flag ? string.Format("{0} {1}", user.first_name, user.last_name) : string.Format("{0} {1}", user.first_name_dat, user.last_name_dat));
        }

        public static double CalculateHeight(double width, NewsActivityLikes activityLikes, List<User> users)
        {
            int userImagesWidth = NewsActivityLikesUC.CalculateUserImagesWidth(activityLikes);
            return TextBlockMeasurementHelper.MeasureHeight(NewsActivityLikesUC.CalculateTextWidth(width, userImagesWidth), NewsActivityLikesUC.GetCaptionText(activityLikes, (IReadOnlyCollection<User>)users), new FontFamily("Segoe WP"), 18.0, 26.0, (LineStackingStrategy)1, (TextWrapping)2, new Thickness()) > 26.0 ? 82.0 : 56.0;
        }

        public void Initialize(double width, NewsActivityLikes activityLikes, List<User> users, bool addSeparator)
        {
            List<User> intersection = NewsActivityLikesUC.GetIntersection((IReadOnlyCollection<User>)users, (IReadOnlyCollection<long>)activityLikes.user_ids);
            if (intersection == null || intersection.Count == 0)
                return;
            int count = intersection.Count;
            string photoMax1 = intersection[0].photo_max;
            if (!string.IsNullOrEmpty(photoMax1))
            {
                ((UIElement)this.gridUser1).Visibility = Visibility.Visible;
                this._userImageUri1 = new Uri(photoMax1);
            }
            int num1 = 1;
            if (count > num1)
            {
                string photoMax2 = intersection[1].photo_max;
                if (!string.IsNullOrEmpty(photoMax2))
                {
                    ((UIElement)this.gridUser2).Visibility = Visibility.Visible;
                    this._userImageUri2 = new Uri(photoMax2);
                }
            }
            int num2 = 2;
            if (count > num2)
            {
                string photoMax2 = intersection[2].photo_max;
                if (!string.IsNullOrEmpty(photoMax2))
                {
                    ((UIElement)this.gridUser3).Visibility = Visibility.Visible;
                    this._userImageUri3 = new Uri(photoMax2);
                }
            }
            int userImagesWidth = NewsActivityLikesUC.CalculateUserImagesWidth(count);
            double textWidth = NewsActivityLikesUC.CalculateTextWidth(width, userImagesWidth);
            Canvas.SetLeft((UIElement)this.textBlockCaption, (double)(16 + userImagesWidth + 12));
            ((FrameworkElement)this.textBlockCaption).Width = textWidth;
            this.textBlockCaption.Text = (NewsActivityLikesUC.GetCaptionText(activityLikes, (IReadOnlyCollection<User>)users));
            if (((FrameworkElement)this.textBlockCaption).ActualHeight <= 26.0)
                this._height = this._height - 26.0;
            ((FrameworkElement)this.canvas).Width = width;
            ((FrameworkElement)this.canvas).Height = this._height;
            ((FrameworkElement)this.canvasBackground).Width = width;
            ((FrameworkElement)this.canvasBackground).Height = this._height;
            ((FrameworkElement)this.gridImages).Height = this._height;
            if (!addSeparator)
                return;
            ((UIElement)this.rectSeparator).Visibility = Visibility.Visible;
            Canvas.SetTop((UIElement)this.rectSeparator, ((FrameworkElement)this.canvas).Height - ((FrameworkElement)this.rectSeparator).Height);
        }

        public override void LoadFullyNonVirtualizableItems()
        {
            VeryLowProfileImageLoader.SetUriSource(this.imageUser1, this._userImageUri1);
            VeryLowProfileImageLoader.SetUriSource(this.imageUser2, this._userImageUri2);
            VeryLowProfileImageLoader.SetUriSource(this.imageUser3, this._userImageUri3);
        }

        public override void ReleaseResources()
        {
            VeryLowProfileImageLoader.SetUriSource(this.imageUser1, null);
            VeryLowProfileImageLoader.SetUriSource(this.imageUser2, null);
            VeryLowProfileImageLoader.SetUriSource(this.imageUser3, null);
        }

        public override void ShownOnScreen()
        {
            DateTime now;
            if (this._userImageUri1 != null && this._userImageUri1.IsAbsoluteUri)
            {
                string originalString = this._userImageUri1.OriginalString;
                now = DateTime.Now;
                long ticks = now.Ticks;
                VeryLowProfileImageLoader.SetPriority(originalString, ticks);
            }
            if (this._userImageUri2 != null && this._userImageUri2.IsAbsoluteUri)
            {
                string originalString = this._userImageUri2.OriginalString;
                now = DateTime.Now;
                long ticks = now.Ticks;
                VeryLowProfileImageLoader.SetPriority(originalString, ticks);
            }
            if (!(this._userImageUri3 != null) || !this._userImageUri3.IsAbsoluteUri)
                return;
            string originalString1 = this._userImageUri3.OriginalString;
            now = DateTime.Now;
            long ticks1 = now.Ticks;
            VeryLowProfileImageLoader.SetPriority(originalString1, ticks1);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsActivityLikesUC.xaml", UriKind.Relative));
            this.canvasBackground = (Canvas)base.FindName("canvasBackground");
            this.canvas = (Canvas)base.FindName("canvas");
            this.gridImages = (Grid)base.FindName("gridImages");
            this.gridUser3 = (Grid)base.FindName("gridUser3");
            this.imageUser3 = (Image)base.FindName("imageUser3");
            this.gridUser2 = (Grid)base.FindName("gridUser2");
            this.imageUser2 = (Image)base.FindName("imageUser2");
            this.gridUser1 = (Grid)base.FindName("gridUser1");
            this.imageUser1 = (Image)base.FindName("imageUser1");
            this.textBlockCaption = (TextBlock)base.FindName("textBlockCaption");
            this.rectSeparator = (Rectangle)base.FindName("rectSeparator");
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
            this.rectangleGeometry2 = (RectangleGeometry)base.FindName("rectangleGeometry2");
            this.rectangleGeometry3 = (RectangleGeometry)base.FindName("rectangleGeometry3");
        }
    }
}
