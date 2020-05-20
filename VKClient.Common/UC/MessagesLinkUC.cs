using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class MessagesLinkUC : NewsLinkUCBase
  {
    private Uri _imageUri;
    private Link _link;
    private double _actualHeight;
    internal StackPanel LayoutRoot;
    internal Grid imageContainer;
    internal Image image;
    internal Grid textContainer;
    internal TextBlock textBlockContent;
    internal StackPanel ratingContainer;
    internal TextBlock priceBlock;
    internal Rating ratingUC;
    internal TextBlock votesCountBlock;
    internal TextBlock captionBlock;
    internal Button actionButton;
    private bool _contentLoaded;

    public MessagesLinkUC()
    {
      this.InitializeComponent();
      StackPanel ratingContainer = this.ratingContainer;
      TextBlock priceBlock = this.priceBlock;
      Rating ratingUc = this.ratingUC;
      TextBlock votesCountBlock = this.votesCountBlock;
      TextBlock captionBlock = this.captionBlock;
      Visibility visibility1;
      ((UIElement) this.actionButton).Visibility = ((Visibility) (int) (visibility1 = Visibility.Collapsed));
      Visibility visibility2;
      Visibility visibility3 = visibility2 = visibility1;
      ((UIElement) captionBlock).Visibility = visibility2;
      Visibility visibility4;
      Visibility visibility5 = visibility4 = visibility3;
      ((UIElement) votesCountBlock).Visibility = visibility4;
      Visibility visibility6;
      Visibility visibility7 = visibility6 = visibility5;
      ((UIElement) ratingUc).Visibility = visibility6;
      Visibility visibility8;
      Visibility visibility9 = visibility8 = visibility7;
      ((UIElement) priceBlock).Visibility = visibility8;
      Visibility visibility10 = visibility9;
      ((UIElement) ratingContainer).Visibility = visibility10;
    }

    public override void Initialize(Link link, double width, string parentPostId = "")
    {
      Photo photo = link.photo;
      if (photo != null)
      {
        double width1 = (double) photo.width;
        double height = (double) photo.height;
        if (height > 0.0 && width1 > 0.0)
        {
          double requiredHeight = ((FrameworkElement) this.imageContainer).Width / (width1 / height);
          this._imageUri = ExtensionsBase.ConvertToUri(photo.GetAppropriateForScaleFactor(requiredHeight, 1));
        }
      }
      double actualHeight1 = this._actualHeight;
      double height1 = ((FrameworkElement) this.imageContainer).Height;
      Thickness margin1 = ((FrameworkElement) this.imageContainer).Margin;
      // ISSUE: explicit reference operation
      double top1 = margin1.Top;
      double num1 = height1 + top1;
      Thickness margin2 = ((FrameworkElement) this.imageContainer).Margin;
      // ISSUE: explicit reference operation
      double bottom1 = ((Thickness) @margin2).Bottom;
      double num2 = num1 + bottom1;
      this._actualHeight = actualHeight1 + num2;
      this._link = link;
      if (!string.IsNullOrEmpty(link.url) && MetroInMotion.GetTilt((DependencyObject) this.LayoutRoot) != 1.2)
        MetroInMotion.SetTilt((DependencyObject) this.LayoutRoot, 1.2);
      this.ComposeContentTextInlines(link);
      double num3 = width;
      LinkButton button = link.button;
      string str1 = button != null ? button.title :  null;
      Thickness margin3;
      if (!string.IsNullOrEmpty(str1))
      {
        ((UIElement) this.actionButton).Visibility = Visibility.Visible;
        ((ContentControl) this.actionButton).Content = str1;
        ((UIElement) this.actionButton).Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        TextBlock textBlock1 = new TextBlock();
        FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
        textBlock1.FontFamily = fontFamily;
        double num4 = 20.0;
        textBlock1.FontSize = num4;
        string str2 = str1;
        textBlock1.Text = str2;
        TextBlock textBlock2 = textBlock1;
        double num5 = num3;
        double actualWidth = ((FrameworkElement) textBlock2).ActualWidth;
        margin3 = ((FrameworkElement) this.actionButton).Margin;
        // ISSUE: explicit reference operation
        double left = ((Thickness) @margin3).Left;
        double num6 = actualWidth + left;
        margin3 = ((FrameworkElement) this.actionButton).Margin;
        // ISSUE: explicit reference operation
        double right = ((Thickness) @margin3).Right;
        double num7 = num6 + right + 53.0;
        num3 = num5 - num7;
      }
      ((FrameworkElement) this.textBlockContent).Width = num3;
      LinkProduct product = link.product;
      string str3;
      if (product == null)
      {
        str3 =  null;
      }
      else
      {
        Price price = product.price;
        str3 = price != null ? price.text :  null;
      }
      string str4 = str3;
      LinkRating rating = link.rating;
      bool flag1 = !string.IsNullOrEmpty(str4);
      bool flag2 = rating != null;
      if (flag1 | flag2)
      {
        ((UIElement) this.ratingContainer).Visibility = Visibility.Visible;
        if (flag1)
        {
          ((UIElement) this.priceBlock).Visibility = Visibility.Visible;
          this.priceBlock.Text = str4;
        }
        if (flag2)
        {
          ((UIElement) this.ratingUC).Visibility = Visibility.Visible;
          this.ratingUC.Value = rating.stars;
          long reviewsCount = rating.reviews_count;
          if (reviewsCount > 0L)
          {
            ((UIElement) this.votesCountBlock).Visibility = Visibility.Visible;
            this.votesCountBlock.Text = (string.Format("({0})", UIStringFormatterHelper.FormatForUIVeryShort(reviewsCount)));
          }
        }
        ((FrameworkElement) this.actionButton).VerticalAlignment = ((VerticalAlignment) 2);
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.ratingContainer);
      }
      string caption = link.caption;
      if (!string.IsNullOrEmpty(caption))
      {
        ((UIElement) this.captionBlock).Visibility = Visibility.Visible;
        this.captionBlock.Text = caption;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.captionBlock);
      }
      this._actualHeight = this._actualHeight + Math.Min(((FrameworkElement) this.textBlockContent).ActualHeight, ((FrameworkElement) this.textBlockContent).MaxHeight);
      double actualHeight2 = this._actualHeight;
      margin3 = ((FrameworkElement) this.textContainer).Margin;
      // ISSUE: explicit reference operation
      double top2 = ((Thickness) @margin3).Top;
      margin3 = ((FrameworkElement) this.textContainer).Margin;
      // ISSUE: explicit reference operation
      double bottom2 = ((Thickness) @margin3).Bottom;
      double num8 = top2 + bottom2;
      this._actualHeight = actualHeight2 + num8;
    }

    private void ComposeContentTextInlines(Link link)
    {
      ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Clear();
      Run run1 = new Run();
      string str = !string.IsNullOrWhiteSpace(link.title) ? link.title : CommonResources.Link;
      run1.Text = str;
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      ((TextElement) run1).FontFamily = fontFamily;
      ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Add((Inline) run1);
      if (string.IsNullOrWhiteSpace(link.description))
        return;
      Run run2 = new Run();
      string description = link.description;
      run2.Text = description;
      Run run3 = run2;
      if (((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Count > 0)
        ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Add((Inline) new LineBreak());
      ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Add((Inline) run3);
    }

    public override double CalculateTotalHeight()
    {
      return this._actualHeight;
    }

    private void LayoutRoot_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (string.IsNullOrEmpty(this._link.url))
        return;
      INavigator current = Navigator.Current;
      string url = this._link.url;
      LinkButton button = this._link.button;
      int num1;
      if (button == null)
      {
        num1 = 0;
      }
      else
      {
        LinkButtonAction action = button.action;
        bool? nullable = action != null ? new bool?(action.IsExternal) : new bool?();
        bool flag = true;
        num1 = nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0;
      }
      int num2 = 0;
      current.NavigateToWebUri(url, num1 != 0, num2 != 0);
    }

    private void ActionButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      LinkButton button = this._link.button;
      string url = (button != null ? button.url :  null) ?? this._link.url;
      if (string.IsNullOrEmpty(url))
        return;
      e.Handled = true;
      LinkButtonAction action = button != null ? button.action :  null;
      LinkButtonAction linkButtonAction1 = action;
      if ((linkButtonAction1 != null ? (linkButtonAction1.Type == LinkButtonActionType.JoinGroupAndOpenUrl ? 1 : 0) : 0) != 0)
      {
        GroupsService.Current.Join(action.group_id, false, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
        {
          if (result.ResultCode != ResultCode.Succeeded)
            return;
          Execute.ExecuteOnUIThread((Action) (() => Navigator.Current.NavigateToWebUri(url, action.IsExternal, false)));
        }),  null);
      }
      else
      {
        INavigator current = Navigator.Current;
        string uri = url;
        LinkButtonAction linkButtonAction2 = action;
        int num1 = linkButtonAction2 != null ? (linkButtonAction2.IsExternal ? 1 : 0) : 0;
        int num2 = 0;
        current.NavigateToWebUri(uri, num1 != 0, num2 != 0);
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image, this._imageUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image,  null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._imageUri !=  null) || !this._imageUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._imageUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MessagesLinkUC.xaml", UriKind.Relative));
      this.LayoutRoot = (StackPanel) base.FindName("LayoutRoot");
      this.imageContainer = (Grid) base.FindName("imageContainer");
      this.image = (Image) base.FindName("image");
      this.textContainer = (Grid) base.FindName("textContainer");
      this.textBlockContent = (TextBlock) base.FindName("textBlockContent");
      this.ratingContainer = (StackPanel) base.FindName("ratingContainer");
      this.priceBlock = (TextBlock) base.FindName("priceBlock");
      this.ratingUC = (Rating) base.FindName("ratingUC");
      this.votesCountBlock = (TextBlock) base.FindName("votesCountBlock");
      this.captionBlock = (TextBlock) base.FindName("captionBlock");
      this.actionButton = (Button) base.FindName("actionButton");
    }
  }
}
