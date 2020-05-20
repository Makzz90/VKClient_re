using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class UserOrGroupHeaderItem : VirtualizableItemBase
    {
        private const int MARGIN_LEFT = 16;
        private const int MARGIN_RIGHT_TOP = 4;
        private const int IMAGE_WIDTH_HEIGHT = 64;
        private const int OPTIONS_WIDTH_HEIGHT = 64;
        private const int MARGIN_TOP_INFO = 16;
        private const int MARGIN_LEFT_TEXT = 92;
        private readonly int _date;
        private readonly User _user;
        private readonly Group _group;
        private readonly bool _isGroupPost;
        private readonly string _extraText;
        private readonly PostIconType _postIconType;
        private readonly PostSourcePlatform _postSourcePlatform;
        private readonly Action _moreOptionsTapCallback;
        private readonly Action _navigatedToUserOrGroupCallback;
        private readonly string _extraTextEnd;
        private Canvas _optionsIcon;

        public override double FixedHeight
        {
            get
            {
                return 80.0;
            }
        }

        public UserOrGroupHeaderItem(double width, Thickness margin, bool isGroupPost, int date, User profile, Group group, string extraText = "", PostIconType postIconType = PostIconType.None, PostSourcePlatform postSourcePlatform = PostSourcePlatform.None, Action moreOptionsTapCallback = null, Action navigatedToUserOrGroupCallback = null, string extraTextEnd = "")
            : base(width, margin, new Thickness())
        {
            this._date = date;
            this._user = profile;
            this._group = group;
            this._isGroupPost = isGroupPost;
            this._extraText = extraText;
            this._postIconType = postIconType;
            this._postSourcePlatform = postSourcePlatform;
            this._moreOptionsTapCallback = moreOptionsTapCallback;
            this._navigatedToUserOrGroupCallback = navigatedToUserOrGroupCallback;
            this._extraTextEnd = extraTextEnd;
            this.GenerateVirtChildren();
        }

        private void GenerateVirtChildren()
        {
            VirtualizableImage virtualizableImage = new VirtualizableImage(64.0, 64.0, new Thickness(0.0), this._isGroupPost ? this._group.photo_200 : this._user.photo_max, null, "", false, true, Stretch.UniformToFill, null, -1.0, false, true);
            MetroInMotion.SetTilt((DependencyObject)virtualizableImage.View, 1.5);
            Canvas.SetLeft((UIElement)virtualizableImage.View, 16.0);
            Canvas.SetTop((UIElement)virtualizableImage.View, 16.0);
            this.VirtualizableChildren.Add((IVirtualizable)virtualizableImage);
        }

        private void View_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            if (this._isGroupPost)
                Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false);
            else
                Navigator.Current.NavigateToUserProfile(this._user.uid, this._user.Name, "", false);
            if (this._navigatedToUserOrGroupCallback == null)
                return;
            this._navigatedToUserOrGroupCallback();
        }

        protected override void GenerateChildren()
        {
            this.CreateFirstLinePanel();
            this.CreateSecondLinePanel();
            if (this._moreOptionsTapCallback != null)
                this.CreateOptionsIcon();
            this._view.Background = (Brush)new SolidColorBrush(Colors.Transparent);
            this._view.Tap += new EventHandler<GestureEventArgs>(this.View_OnTap);
        }

        private void CreateFirstLinePanel()
        {
            double num = 0.0;
            Border postTypeIcon = this.GetPostTypeIcon();
            if (postTypeIcon != null)
                num += UserOrGroupHeaderItem.GetElementTotalWidth((FrameworkElement)postTypeIcon);
            TextBlock textName = new TextBlock()
            {
                Text = this._isGroupPost ? this._group.name : this._user.Name,
                FontSize = 25.3,
                Foreground = (Brush)(Application.Current.Resources["PhoneDarkBlueBrush"] as SolidColorBrush)
            };
            double maxWidth = this.Width - (92.0 + (this._moreOptionsTapCallback != null ? 68.0 : 16.0) + num);
            textName.CorrectText(maxWidth);
            Canvas.SetLeft((UIElement)textName, 92.0);
            Canvas.SetTop((UIElement)textName, 17.0);
            this.Children.Add((FrameworkElement)textName);
            if (postTypeIcon == null)
                return;
            Canvas.SetLeft((UIElement)postTypeIcon, 92.0 + textName.ActualWidth);
            Canvas.SetTop((UIElement)postTypeIcon, 17.0);
            this.Children.Add((FrameworkElement)postTypeIcon);
        }

        private Border GetOnlineIcon()
        {
            if (this._isGroupPost || this._user.online == 0 && this._user.online_mobile == 0)
                return (Border)null;
            bool flag = this._user.online_mobile == 1;
            string str = "/VKClient.Common;component/Resources/" + (flag ? "OnlineMobileMark" : "OnlineMark") + ".png";
            ImageBrush imageBrush1 = new ImageBrush();
            ImageLoader.SetImageBrushMultiResSource(imageBrush1, str);
            Border border = new Border();
            double num1 = flag ? 12.0 : 9.0;
            border.Width = num1;
            double num2 = flag ? 18.0 : 9.0;
            border.Height = num2;
            int num3 = 0;
            border.VerticalAlignment = (VerticalAlignment)num3;
            Thickness thickness = flag ? new Thickness(8.0, 9.0, 0.0, 0.0) : new Thickness(8.0, 15.0, 0.0, 0.0);
            border.Margin = thickness;
            Style style = (Style)Application.Current.Resources["IconBorderWallPost"];
            border.Style = style;
            ImageBrush imageBrush2 = imageBrush1;
            border.OpacityMask = (Brush)imageBrush2;
            return border;
        }

        private Border GetPostTypeIcon()
        {
            string str1 = "";
            switch (this._postIconType)
            {
                case PostIconType.Private:
                    str1 = "CardFriendsOnly";
                    break;
                case PostIconType.Pinned:
                    str1 = "CardPinned";
                    break;
            }
            if (string.IsNullOrEmpty(str1))
                return (Border)null;
            string str2 = "/VKClient.Common;component/Resources/PostAppIcons/" + str1 + ".png";
            ImageBrush imageBrush1 = new ImageBrush();
            ImageLoader.SetImageBrushMultiResSource(imageBrush1, str2);
            Border border = new Border();
            double num1 = 16.0;
            border.Height = num1;
            double num2 = 16.0;
            border.Width = num2;
            int num3 = 0;
            border.VerticalAlignment = (VerticalAlignment)num3;
            int num4 = 0;
            border.HorizontalAlignment = (HorizontalAlignment)num4;
            Thickness thickness = new Thickness(8.0, 11.0, 0.0, 0.0);
            border.Margin = thickness;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneGreyIconBrush"];
            border.Background = (Brush)solidColorBrush;
            ImageBrush imageBrush2 = imageBrush1;
            border.OpacityMask = (Brush)imageBrush2;
            return border;
        }

        private void CreateSecondLinePanel()
        {
            double num = 0.0;
            Border postSourceTypeIcon = this.GetPostSourceTypeIcon();
            if (postSourceTypeIcon != null)
                num += UserOrGroupHeaderItem.GetElementTotalWidth((FrameworkElement)postSourceTypeIcon);
            string subtitleText = this.GetSubtitleText();
            TextBlock textBlock = new TextBlock();
            textBlock.Text = subtitleText;
            textBlock.FontSize = 20.0;
            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            textBlock.LineHeight = 26.0;
            SolidColorBrush solidColorBrush = Application.Current.Resources["PhoneCaptionGrayBrush"] as SolidColorBrush;
            textBlock.Foreground = (Brush)solidColorBrush;
            TextBlock textName = textBlock;
            double maxWidth = this.Width - (92.0 + (this._moreOptionsTapCallback != null ? 68.0 : 16.0) + num);
            textName.CorrectText(maxWidth);
            Canvas.SetLeft((UIElement)textName, 92.0);
            Canvas.SetTop((UIElement)textName, 48.0);
            this.Children.Add((FrameworkElement)textName);
            if (postSourceTypeIcon == null)
                return;
            Canvas.SetLeft((UIElement)postSourceTypeIcon, 92.0 + textName.ActualWidth);
            Canvas.SetTop((UIElement)postSourceTypeIcon, 48.0);
            this.Children.Add((FrameworkElement)postSourceTypeIcon);
        }

        private string GetSubtitleText()
        {
            StringBuilder stringBuilder = new StringBuilder(this._extraText);
            if (this._date != 0)
            {
                string str = UIStringFormatterHelper.FormatDateTimeForUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double)this._date, true));
                if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "kk")
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(" ");
                    stringBuilder.Append(str);
                }
                else
                    stringBuilder.Insert(0, str + "  ");
            }
            string str1 = stringBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(this._extraTextEnd))
                return string.Format("{0} {1}", (object)str1, (object)this._extraTextEnd);
            return str1;
        }

        private Border GetPostSourceTypeIcon()
        {
            string iconUri = this._postSourcePlatform.GetIconUri();
            if (string.IsNullOrEmpty(iconUri))
                return (Border)null;
            ImageBrush imageBrush1 = new ImageBrush();
            ImageLoader.SetImageBrushMultiResSource(imageBrush1, iconUri);
            Border border = new Border();
            double num1 = 16.0;
            border.Height = num1;
            double num2 = 16.0;
            border.Width = num2;
            int num3 = 0;
            border.VerticalAlignment = (VerticalAlignment)num3;
            Thickness thickness = new Thickness(8.0, 6.0, 0.0, 0.0);
            border.Margin = thickness;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneGreyIconBrush"];
            border.Background = (Brush)solidColorBrush;
            ImageBrush imageBrush2 = imageBrush1;
            border.OpacityMask = (Brush)imageBrush2;
            return border;
        }

        private void CreateOptionsIcon()
        {
            this._optionsIcon = UserOrGroupHeaderItem.GetOptionsIcon();
            this._optionsIcon.Tap += new EventHandler<GestureEventArgs>(this.OptionsIcon_OnTap);
            Canvas.SetLeft((UIElement)this._optionsIcon, this.Width - this._optionsIcon.Width - 4.0);
            Canvas.SetTop((UIElement)this._optionsIcon, 4.0);
            this.Children.Add((FrameworkElement)this._optionsIcon);
        }

        private void OptionsIcon_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            Action action = this._moreOptionsTapCallback;
            if (action == null)
                return;
            action();
        }

        private static double GetElementTotalWidth(FrameworkElement element)
        {
            Thickness margin = element.Margin;
            double num = margin.Left + element.Width;
            margin = element.Margin;
            double right = margin.Right;
            return num + right;
        }

        private static Canvas GetOptionsIcon()
        {
            Canvas canvas = new Canvas();
            canvas.Width = 64.0;
            canvas.Height = 64.0;
            canvas.Background = new SolidColorBrush(Colors.Transparent);
            Border border1 = new Border();
            border1.Width = 56.0;
            border1.Height = 56.0;
            Image image2 = new Image();
            image2.VerticalAlignment = VerticalAlignment.Center;
            image2.HorizontalAlignment = HorizontalAlignment.Center;
            MultiResImageLoader.SetUriSource(image2, "/Resources/WallPost/CardActions.png");
            border1.Child = image2;
            Canvas.SetLeft(border1, (64.0 - border1.Width) / 2.0);
            Canvas.SetTop(border1, (64.0 - border1.Height) / 2.0);
            canvas.Children.Add(border1);
            MetroInMotion.SetTilt(canvas, 1.5);
            return canvas;
        }

        protected override void ReleaseResourcesOnUnload()
        {
            base.ReleaseResourcesOnUnload();
            this._view.Tap -= new EventHandler<GestureEventArgs>(this.View_OnTap);
            if (this._optionsIcon == null)
                return;
            this._optionsIcon.Tap -= new EventHandler<GestureEventArgs>(this.OptionsIcon_OnTap);
        }
    }
}
