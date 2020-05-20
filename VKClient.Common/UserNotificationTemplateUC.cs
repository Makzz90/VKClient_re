using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class UserNotificationTemplateUC : UserControl
  {
    private List<User> _users;
    private string _thumb;
    internal Grid LayoutRoot;
    internal Grid gridInner;
    internal Ellipse mainUserImagePlaceholder;
    internal Image mainUserImage;
    internal Image imageIcon;
    internal TextBlock textBlockHeader;
    internal Canvas canvasAdditionalUserImages;
    internal Ellipse rect1;
    internal Image image1;
    internal Ellipse rect2;
    internal Image image2;
    internal Ellipse rect3;
    internal Image image3;
    internal Ellipse rect4;
    internal Image image4;
    internal Ellipse rect5;
    internal Image image5;
    internal TextBlock textBlockDate;
    internal Image imageThumb;
    internal TextSeparatorUC ucEarlierReplies;
    private bool _contentLoaded;

    public UserNotificationTemplateUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucEarlierReplies.Text = CommonResources.ViewedFeedback;
    }

    public void LoadImages()
    {
      if (this._users == null)
        return;
      List<Image> imageList1 = new List<Image>();
      Image image1 = this.image1;
      imageList1.Add(image1);
      Image image2 = this.image2;
      imageList1.Add(image2);
      Image image3 = this.image3;
      imageList1.Add(image3);
      Image image4 = this.image4;
      imageList1.Add(image4);
      Image image5 = this.image5;
      imageList1.Add(image5);
      List<Image> imageList2 = imageList1;
      List<FrameworkElement> frameworkElementList1 = new List<FrameworkElement>();
      Ellipse rect1 = this.rect1;
      frameworkElementList1.Add((FrameworkElement) rect1);
      Ellipse rect2 = this.rect2;
      frameworkElementList1.Add((FrameworkElement) rect2);
      Ellipse rect3 = this.rect3;
      frameworkElementList1.Add((FrameworkElement) rect3);
      Ellipse rect4 = this.rect4;
      frameworkElementList1.Add((FrameworkElement) rect4);
      Ellipse rect5 = this.rect5;
      frameworkElementList1.Add((FrameworkElement) rect5);
      List<FrameworkElement> frameworkElementList2 = frameworkElementList1;
      frameworkElementList2.ForEach((Action<FrameworkElement>) (r => ((UIElement) r).Visibility = Visibility.Collapsed));
      int val2 = Math.Min(this._users.Count - 1, imageList2.Count);
      if (!string.IsNullOrEmpty(this._thumb))
        val2 = Math.Min(3, val2);
      for (int index = 0; index < val2; ++index)
      {
        ImageLoader.SetSourceForImage(imageList2[index], this._users[index + 1].photo_max, false);
        ((FrameworkElement) imageList2[index]).Tag=(this._users[index + 1]);
        ((UIElement) imageList2[index]).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.UserNotificationTemplateUC_Tap));
      }
      for (int count = this._users.Count; count < frameworkElementList2.Count; ++count)
      {
        ((UIElement) imageList2[count]).Visibility = Visibility.Collapsed;
        ((UIElement) frameworkElementList2[count]).Visibility = Visibility.Collapsed;
      }
      if (this._thumb == null)
        return;
      ImageLoader.SetSourceForImage(this.imageThumb, this._thumb, false);
    }

    public double Configure(List<User> users, string actionText, string dateText, string hightlightedText, string thumb, NotificationType type, int totalUsersCount, bool showEarlierReplies, string forcedTypeIcon = null)
    {
      this._users = users;
      this._thumb = thumb;
      ((UIElement) this.imageThumb).Visibility = (string.IsNullOrWhiteSpace(this._thumb) ? Visibility.Collapsed : Visibility.Visible);
      ((UIElement) this.canvasAdditionalUserImages).Visibility = (totalUsersCount > 1 ? Visibility.Visible : Visibility.Collapsed);
      this.ConfigureLeftSideImage(type, forcedTypeIcon);
      ((FrameworkElement) this.textBlockHeader).Width=(this.CalculateTextBlockHeaderWidth());
      this.ConfigureHeaderText(users, actionText, hightlightedText, totalUsersCount);
      this.textBlockDate.Text = dateText;
      ((UIElement) this.ucEarlierReplies).Visibility = (showEarlierReplies ? Visibility.Visible : Visibility.Collapsed);
      Thickness margin = ((FrameworkElement) this.textBlockHeader).Margin;
      // ISSUE: explicit reference operation
      double num1 = ((Thickness) @margin).Top + ((FrameworkElement) this.textBlockHeader).ActualHeight;
      margin = ((FrameworkElement) this.textBlockHeader).Margin;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @margin).Bottom;
      double num2 = num1 + bottom + (((UIElement)this.canvasAdditionalUserImages).Visibility == Visibility.Visible ? ((FrameworkElement)this.canvasAdditionalUserImages).Height : 0.0) + ((FrameworkElement)this.textBlockDate).ActualHeight + 20.0;
      if (showEarlierReplies)
        num2 += 46.0;
      ((FrameworkElement) this.LayoutRoot).Height = num2;
      return num2;
    }

    private void ConfigureHeaderText(List<User> users, string actionText, string hightlightedText, int totalUsersCount)
    {
      if (users.Count <= 0)
        return;
      Run run1 = new Run();
      run1.Text = (users[0].Name);
      ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run1);
      if (users.Count >= 2 && totalUsersCount == 2)
      {
        Run run2 = new Run();
        FontFamily fontFamily = new FontFamily("Segoe WP SemiLight");
        ((TextElement) run2).FontFamily = fontFamily;
        Brush brush = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        ((TextElement) run2).Foreground = brush;
        Run run3 = run2;
        run3.Text = (" " + CommonResources.And + " ");
        ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run3);
        Run run4 = new Run();
        string name = users[1].Name;
        run4.Text = name;
        ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run4);
      }
      else if (totalUsersCount > 2)
      {
        int number = totalUsersCount - 1;
        string[] strArray = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.AndOneOtherFrm, CommonResources.AndTwoFourOthersFrm, CommonResources.AndFiveOthersFrm, true,  null, false).Split(new string[1]
        {
          number.ToString()
        }, StringSplitOptions.None);
        Run run2 = new Run();
        FontFamily fontFamily1 = new FontFamily("Segoe WP SemiLight");
        ((TextElement) run2).FontFamily = fontFamily1;
        Brush brush1 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        ((TextElement) run2).Foreground = brush1;
        Run run3 = run2;
        run3.Text = (" " + strArray[0].Trim() + " ");
        Run run4 = new Run();
        FontFamily fontFamily2 = new FontFamily("Segoe WP Semibold");
        ((TextElement) run4).FontFamily = fontFamily2;
        Brush brush2 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        ((TextElement) run4).Foreground = brush2;
        Run run5 = run4;
        run5.Text = (number.ToString());
        Run run6 = new Run();
        FontFamily fontFamily3 = new FontFamily("Segoe WP SemiLight");
        ((TextElement) run6).FontFamily = fontFamily3;
        Brush brush3 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        ((TextElement) run6).Foreground = brush3;
        Run run7 = run6;
        run7.Text = (" " + strArray[1].Trim());
        ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run3);
        ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run5);
        ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run7);
      }
      Run run8 = new Run();
      FontFamily fontFamily4 = new FontFamily("Segoe WP SemiLight");
      ((TextElement) run8).FontFamily = fontFamily4;
      Brush brush4 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
      ((TextElement) run8).Foreground = brush4;
      Run run9 = run8;
      run9.Text = (" " + actionText);
      ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run9);
      if (string.IsNullOrEmpty(hightlightedText))
        return;
      Run run10 = new Run();
      string str = " " + hightlightedText;
      run10.Text = str;
      ((PresentationFrameworkCollection<Inline>) this.textBlockHeader.Inlines).Add((Inline) run10);
    }

    private double CalculateTextBlockHeaderWidth()
    {
      double width1 = ((FrameworkElement) this.gridInner).Width;
      Thickness margin1 = ((FrameworkElement) this.textBlockHeader).Margin;
      // ISSUE: explicit reference operation
      double left1 = margin1.Left;
      margin1 = ((FrameworkElement) this.textBlockHeader).Margin;
      // ISSUE: explicit reference operation
      double right1 = margin1.Right;
      double num1 = left1 - right1;
      double num2 = width1 - num1;
      if (((UIElement)this.imageThumb).Visibility == Visibility.Visible)
      {
        double num3 = num2;
        double width2 = ((FrameworkElement) this.imageThumb).Width;
        Thickness margin2 = ((FrameworkElement) this.imageThumb).Margin;
        // ISSUE: explicit reference operation
        double left2 = ((Thickness) @margin2).Left;
        double num4 = width2 + left2;
        margin2 = ((FrameworkElement) this.imageThumb).Margin;
        // ISSUE: explicit reference operation
        double right2 = ((Thickness) @margin2).Right;
        double num5 = num4 + right2;
        num2 = num3 - num5;
      }
      return num2;
    }

    private void ConfigureLeftSideImage(NotificationType type, string forcedTypeIcon)
    {
      if (this._users.Count > 0)
      {
        Image mainUserImage = this.mainUserImage;
        object user;
        ((FrameworkElement) this.mainUserImagePlaceholder).Tag=((User) (user = this._users[0]));
        object obj = user;
        ((FrameworkElement) mainUserImage).Tag = obj;
        ImageLoader.SetSourceForImage(this.mainUserImage, this._users[0].photo_max, false);
      }
      string str = "";
      switch (type)
      {
        case NotificationType.follow:
          str = "/Resources/FeedbackIconsFollower.png";
          break;
        case NotificationType.friend_accepted:
          str = "/Resources/FeedbackIconsRequest.png";
          break;
        case NotificationType.like_post:
        case NotificationType.like_comment:
        case NotificationType.like_photo:
        case NotificationType.like_video:
        case NotificationType.like_comment_photo:
        case NotificationType.like_comment_video:
        case NotificationType.like_comment_topic:
          str = "/Resources/FeedbackIconsLike.png";
          break;
        case NotificationType.copy_post:
        case NotificationType.copy_photo:
        case NotificationType.copy_video:
          str = "/Resources/FeedbackIconsRepost.png";
          break;
      }
      if (forcedTypeIcon != null)
        str = forcedTypeIcon;
      MultiResImageLoader.SetUriSource(this.imageIcon, str);
    }

    private string PrepareText(string text, bool haveThumb)
    {
      int num = haveThumb ? 22 : 30;
      string[] strArray = text.Split(' ');
      string str = "";
      bool flag = false;
      for (int index = 0; index < strArray.Length; ++index)
      {
        if (!flag && str.Length + strArray[index].Length > num)
        {
          str += Environment.NewLine;
          flag = true;
        }
        str = str + strArray[index] + " ";
      }
      if (!flag)
        str += Environment.NewLine;
      return str;
    }

    private void UserNotificationTemplateUC_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      User tag = frameworkElement.Tag as User;
      if (tag == null)
        return;
      e.Handled = true;
      Navigator.Current.NavigateToUserProfile(tag.uid, tag.Name, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UserNotificationTemplateUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.gridInner = (Grid) base.FindName("gridInner");
      this.mainUserImagePlaceholder = (Ellipse) base.FindName("mainUserImagePlaceholder");
      this.mainUserImage = (Image) base.FindName("mainUserImage");
      this.imageIcon = (Image) base.FindName("imageIcon");
      this.textBlockHeader = (TextBlock) base.FindName("textBlockHeader");
      this.canvasAdditionalUserImages = (Canvas) base.FindName("canvasAdditionalUserImages");
      this.rect1 = (Ellipse) base.FindName("rect1");
      this.image1 = (Image) base.FindName("image1");
      this.rect2 = (Ellipse) base.FindName("rect2");
      this.image2 = (Image) base.FindName("image2");
      this.rect3 = (Ellipse) base.FindName("rect3");
      this.image3 = (Image) base.FindName("image3");
      this.rect4 = (Ellipse) base.FindName("rect4");
      this.image4 = (Image) base.FindName("image4");
      this.rect5 = (Ellipse) base.FindName("rect5");
      this.image5 = (Image) base.FindName("image5");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
      this.imageThumb = (Image) base.FindName("imageThumb");
      this.ucEarlierReplies = (TextSeparatorUC) base.FindName("ucEarlierReplies");
    }
  }
}
