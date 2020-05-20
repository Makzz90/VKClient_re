using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsLinkUC : NewsLinkUCBase
  {
    private const double MIN_IMAGE_RATIO = 1.5;
    private Link _link;
    private Uri _imageUri;
    private double _actualHeight;
    private string _parentPostId;
    internal Canvas canvasImageContainer;
    internal Image imagePreview;
    internal Grid gridContent;
    internal TextBlock textBlockContent;
    internal StackPanel panelProductRating;
    internal TextBlock textBlockPrice;
    internal Rating ucRating;
    internal TextBlock textBlockVotesCount;
    internal TextBlock textBlockCaption;
    internal Button buttonAction;
    private bool _contentLoaded;

    public NewsLinkUC()
    {
      this.InitializeComponent();
      ((UIElement) this.panelProductRating).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockPrice).Visibility = Visibility.Collapsed;
      ((UIElement) this.ucRating).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockVotesCount).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockCaption).Visibility = Visibility.Collapsed;
      Grid.SetColumnSpan((FrameworkElement) this.textBlockContent, 2);
      ((UIElement) this.buttonAction).Visibility = Visibility.Collapsed;
      ((FrameworkElement) this.buttonAction).VerticalAlignment = ((VerticalAlignment) 1);
    }

    public override void Initialize(Link link, double width, string parentPostId = "")
    {
      this._link = link;
      this._parentPostId = parentPostId;
      double val1 = (double) link.photo.width / (double) link.photo.height;
      double num1 = Math.Max(val1, 1.5);
      double num2 = width;
      double num3 = Math.Round(num2 / num1);
      this._imageUri = ExtensionsBase.ConvertToUri(link.photo.GetAppropriateForScaleFactor(num2 / val1, 1));
      ((FrameworkElement) this.canvasImageContainer).Height = num3;
      ((FrameworkElement) this.canvasImageContainer).Width = num2;
      ((FrameworkElement) this.imagePreview).Height = num3;
      ((FrameworkElement) this.imagePreview).Width = num2;
      this._actualHeight = this._actualHeight + num3;
      this.ComposeContentTextInlines(link);
      double num4 = width;
      Thickness margin1 = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double left1 = margin1.Left;
      double num5 = num4 - left1;
      Thickness margin2 = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double right = ((Thickness) @margin2).Right;
      double num6 = num5 - right;
      LinkButton button = link.button;
      if (button != null)
      {
        ((UIElement) this.buttonAction).Visibility = Visibility.Visible;
        ((ContentControl) this.buttonAction).Content = button.title;
        ((UIElement) this.buttonAction).Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        TextBlock textBlock = new TextBlock();
        FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
        textBlock.FontFamily = fontFamily;
        double num7 = 20.0;
        textBlock.FontSize = num7;
        string title = button.title;
        textBlock.Text = title;
        double actualWidth = (double) (int) ((FrameworkElement) textBlock).ActualWidth;
        Thickness margin3 = ((FrameworkElement) this.buttonAction).Margin;
        // ISSUE: explicit reference operation
        double left2 = ((Thickness) @margin3).Left;
        double num8 = actualWidth + left2;
        Thickness margin4 = ((FrameworkElement) this.buttonAction).Margin;
        // ISSUE: explicit reference operation
        double num9 = -((Thickness) @margin4).Right;
        double num10 = num8 + num9 + 32.0;
        num6 -= num10;
      }
      ((FrameworkElement) this.textBlockContent).Width = num6;
      LinkProduct product = link.product;
      string str;
      if (product == null)
      {
        str =  null;
      }
      else
      {
        Price price = product.price;
        str = price != null ? price.text :  null;
      }
      bool flag1 = !string.IsNullOrEmpty(str);
      bool flag2 = link.rating != null;
      if (flag1 | flag2)
      {
        ((UIElement) this.panelProductRating).Visibility = Visibility.Visible;
        if (flag1)
        {
          ((UIElement) this.textBlockPrice).Visibility = Visibility.Visible;
          this.textBlockPrice.Text = link.product.price.text;
        }
        if (flag2)
        {
          ((UIElement) this.ucRating).Visibility = Visibility.Visible;
          this.ucRating.Value = link.rating.stars;
          long reviewsCount = link.rating.reviews_count;
          if (reviewsCount > 0L)
          {
            ((UIElement) this.textBlockVotesCount).Visibility = Visibility.Visible;
            this.textBlockVotesCount.Text = (string.Format("({0})", UIStringFormatterHelper.FormatForUIVeryShort(reviewsCount)));
          }
        }
        ((FrameworkElement) this.buttonAction).VerticalAlignment = ((VerticalAlignment) 2);
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.panelProductRating);
      }
      if (!string.IsNullOrEmpty(link.caption))
      {
        ((UIElement) this.textBlockCaption).Visibility = Visibility.Visible;
        this.textBlockCaption.Text = link.caption;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.textBlockCaption);
      }
      this._actualHeight = this._actualHeight + Math.Min(((FrameworkElement) this.textBlockContent).ActualHeight, ((FrameworkElement) this.textBlockContent).MaxHeight);
      double actualHeight = this._actualHeight;
      Thickness margin5 = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double top = ((Thickness) @margin5).Top;
      Thickness margin6 = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @margin6).Bottom;
      double num11 = top + bottom;
      this._actualHeight = actualHeight + num11;
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

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (string.IsNullOrEmpty(this._link.url))
        return;
      LinkButton button = this._link.button;
      LinkButtonAction linkButtonAction = button != null ? button.action :  null;
      Navigator.Current.NavigateToWebUri(this._link.url, linkButtonAction != null && linkButtonAction.IsExternal, false);
      if (string.IsNullOrEmpty(this._parentPostId) || linkButtonAction == null || linkButtonAction.Type != LinkButtonActionType.JoinGroupAndOpenUrl)
        return;
      EventAggregator.Current.Publish(new PostInteractionEvent()
      {
        PostId = this._parentPostId,
        Action = PostInteractionAction.snippet_action,
        Link = this._link.url
      });
    }

    private void ActionButton_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      string url = this._link.button.url ?? this._link.url;
      LinkButtonAction action1 = this._link.button.action;
      if ((action1 != null ? (action1.Type == LinkButtonActionType.JoinGroupAndOpenUrl ? 1 : 0) : 0) != 0)
      {
        GroupsService.Current.Join(this._link.button.action.group_id, false, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
        {
          if (result.ResultCode != ResultCode.Succeeded)
            return;
          Execute.ExecuteOnUIThread((Action) (() => Navigator.Current.NavigateToWebUri(url, this._link.button.action.IsExternal, false)));
        }), "wall" + this._parentPostId);
        if (string.IsNullOrEmpty(this._parentPostId))
          return;
        EventAggregator.Current.Publish(new PostInteractionEvent()
        {
          PostId = this._parentPostId,
          Action = PostInteractionAction.snippet_button_action,
          Link = url
        });
      }
      else
      {
        INavigator current = Navigator.Current;
        string uri = url;
        LinkButton button = this._link.button;
        int num1;
        if (button == null)
        {
          num1 = 0;
        }
        else
        {
          LinkButtonAction action2 = button.action;
          bool? nullable = action2 != null ? new bool?(action2.IsExternal) : new bool?();
          bool flag = true;
          num1 = nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0;
        }
        int num2 = 0;
        current.NavigateToWebUri(uri, num1 != 0, num2 != 0);
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imagePreview, this._imageUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imagePreview,  null);
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsLinkUC.xaml", UriKind.Relative));
      this.canvasImageContainer = (Canvas) base.FindName("canvasImageContainer");
      this.imagePreview = (Image) base.FindName("imagePreview");
      this.gridContent = (Grid) base.FindName("gridContent");
      this.textBlockContent = (TextBlock) base.FindName("textBlockContent");
      this.panelProductRating = (StackPanel) base.FindName("panelProductRating");
      this.textBlockPrice = (TextBlock) base.FindName("textBlockPrice");
      this.ucRating = (Rating) base.FindName("ucRating");
      this.textBlockVotesCount = (TextBlock) base.FindName("textBlockVotesCount");
      this.textBlockCaption = (TextBlock) base.FindName("textBlockCaption");
      this.buttonAction = (Button) base.FindName("buttonAction");
    }
  }
}
