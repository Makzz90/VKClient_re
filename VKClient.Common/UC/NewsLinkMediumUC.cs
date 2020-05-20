using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsLinkMediumUC : NewsLinkUCBase
  {
    private const double SNIPPET_MIN_HEIGHT = 100.0;
    private const double SNIPPET_MAX_HEIGHT = 152.0;
    private Link _link;
    private Uri _imageUri;
    private double _actualHeight;
    private string _parentPostId;
    internal Grid canvasImageContainer;
    internal Image imagePreview;
    internal Grid gridContent;
    internal TextBlock titleBlock;
    internal TextBlock priceBlock;
    internal TextBlock descriptionBlock;
    internal StackPanel panelProductRating;
    internal TextBlock textBlockPrice;
    internal Rating ucRating;
    internal TextBlock textBlockVotesCount;
    internal TextBlock textBlockCaption;
    internal Button buttonAction;
    private bool _contentLoaded;

    public NewsLinkMediumUC()
    {
      this.InitializeComponent();
      ((UIElement) this.panelProductRating).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockPrice).Visibility = Visibility.Collapsed;
      ((UIElement) this.ucRating).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockVotesCount).Visibility = Visibility.Collapsed;
      ((UIElement) this.textBlockCaption).Visibility = Visibility.Collapsed;
      ((UIElement) this.buttonAction).Visibility = Visibility.Collapsed;
      ((UIElement) this.priceBlock).Visibility = Visibility.Collapsed;
      ((UIElement) this.descriptionBlock).Visibility = Visibility.Collapsed;
    }

    public override void Initialize(Link link, double width, string parentPostId = "")
    {
      this._link = link;
      this._parentPostId = parentPostId;
      LinkProduct product = link.product;
      string str1;
      if (product == null)
      {
        str1 =  null;
      }
      else
      {
        Price price = product.price;
        str1 = price != null ? price.text :  null;
      }
      bool hasProduct = !string.IsNullOrEmpty(str1);
      bool hasRating = link.rating != null;
      double val2 = this.titleBlock.LineHeight * 3.0;
      this.ComposeContentTextInlines(link, hasProduct, hasRating);
      TextBlock titleBlock = this.titleBlock;
      TextBlock priceBlock = this.priceBlock;
      TextBlock descriptionBlock = this.descriptionBlock;
      double num1 = width;
      double width1 = ((FrameworkElement) this.canvasImageContainer).Width;
      Thickness margin = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double left = ((Thickness) @margin).Left;
      double num2 = width1 + left;
      margin = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double right = ((Thickness) @margin).Right;
      double num3 = num2 + right;
      double num4;
      double num5 = num4 = num1 - num3;
      ((FrameworkElement) descriptionBlock).Width = num4;
      double num6;
      double num7 = num6 = num5;
      ((FrameworkElement) priceBlock).Width = num6;
      double num8 = num7;
      ((FrameworkElement) titleBlock).Width = num8;
      ((FrameworkElement) this.titleBlock).MaxHeight=(this.titleBlock.LineHeight * (hasProduct || hasRating ? 2.0 : 3.0));
      if (((FrameworkElement) this.titleBlock).ActualHeight < ((FrameworkElement) this.titleBlock).MaxHeight)
        ((FrameworkElement) this.descriptionBlock).MaxHeight=(((FrameworkElement) this.titleBlock).MaxHeight - ((FrameworkElement) this.titleBlock).ActualHeight);
      else
        ((UIElement) this.descriptionBlock).Visibility = Visibility.Collapsed;
      if (hasRating)
      {
        ((UIElement) this.panelProductRating).Visibility = Visibility.Visible;
        if (hasProduct)
        {
          ((UIElement) this.textBlockPrice).Visibility = Visibility.Visible;
          this.textBlockPrice.Text = link.product.price.text;
        }
        ((UIElement) this.ucRating).Visibility = Visibility.Visible;
        this.ucRating.Value = link.rating.stars;
        long reviewsCount = link.rating.reviews_count;
        if (reviewsCount > 0L)
        {
          ((UIElement) this.textBlockVotesCount).Visibility = Visibility.Visible;
          this.textBlockVotesCount.Text = (string.Format("({0})", UIStringFormatterHelper.FormatForUIVeryShort(reviewsCount)));
        }
      }
      if (!string.IsNullOrEmpty(link.caption) && !hasProduct | hasRating)
      {
        ((UIElement) this.textBlockCaption).Visibility = Visibility.Visible;
        this.textBlockCaption.Text = link.caption;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.textBlockCaption);
      }
      LinkButton linkButton1 = link.button;
      if (linkButton1 == null & hasProduct)
      {
        Link link1 = link;
        LinkButton linkButton2 = new LinkButton();
        linkButton2.title = CommonResources.ViewProduct;
        string str2 = string.Format("https://vk.com/product{0}_{1}", link.product.owner_id, link.product.id);
        linkButton2.url = str2;
        LinkButton linkButton3 = linkButton2;
        link1.button = linkButton2;
        linkButton1 = linkButton3;
      }
      if (!string.IsNullOrWhiteSpace(linkButton1 != null ? linkButton1.title :  null))
      {
        ((UIElement) this.buttonAction).Visibility = Visibility.Visible;
        ((ContentControl) this.buttonAction).Content = linkButton1.title;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.buttonAction);
      }
      this._actualHeight = this._actualHeight + Math.Min(((FrameworkElement) this.titleBlock).ActualHeight + ((FrameworkElement) this.priceBlock).ActualHeight + ((FrameworkElement) this.descriptionBlock).ActualHeight + ((FrameworkElement) this.textBlockPrice).ActualHeight, val2);
      this._actualHeight = Math.Min(this._actualHeight, 152.0);
      this._actualHeight = Math.Max(this._actualHeight, 100.0);
      double actualHeight = this._actualHeight;
      margin = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double top = ((Thickness) @margin).Top;
      margin = ((FrameworkElement) this.gridContent).Margin;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @margin).Bottom;
      double num9 = top + bottom;
      this._actualHeight = actualHeight + num9;
      ((FrameworkElement) this.canvasImageContainer).Height = this._actualHeight;
      this._imageUri = link.photo.GetAppropriateForScaleFactor(this._actualHeight, 1).ConvertToUri();
    }

    private void ComposeContentTextInlines(Link link, bool hasProduct, bool hasRating)
    {
      this.titleBlock.Text = (!string.IsNullOrWhiteSpace(link.title) ? link.title : CommonResources.Link);
      if (hasProduct && !hasRating)
      {
        this.priceBlock.Text = link.product.price.text;
        ((UIElement) this.priceBlock).Visibility = Visibility.Visible;
      }
      string str = link.description;
      if (string.IsNullOrWhiteSpace(str))
      {
        LinkProduct product = link.product;
        str = product != null ? product.description :  null;
      }
      if (string.IsNullOrWhiteSpace(str))
        return;
      this.descriptionBlock.Text = str;
      ((UIElement) this.descriptionBlock).Visibility = Visibility.Visible;
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
        LinkButtonAction action2 = this._link.button.action;
        int num1 = action2 != null ? (action2.IsExternal ? 1 : 0) : 0;
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsLinkMediumUC.xaml", UriKind.Relative));
      this.canvasImageContainer = (Grid) base.FindName("canvasImageContainer");
      this.imagePreview = (Image) base.FindName("imagePreview");
      this.gridContent = (Grid) base.FindName("gridContent");
      this.titleBlock = (TextBlock) base.FindName("titleBlock");
      this.priceBlock = (TextBlock) base.FindName("priceBlock");
      this.descriptionBlock = (TextBlock) base.FindName("descriptionBlock");
      this.panelProductRating = (StackPanel) base.FindName("panelProductRating");
      this.textBlockPrice = (TextBlock) base.FindName("textBlockPrice");
      this.ucRating = (Rating) base.FindName("ucRating");
      this.textBlockVotesCount = (TextBlock) base.FindName("textBlockVotesCount");
      this.textBlockCaption = (TextBlock) base.FindName("textBlockCaption");
      this.buttonAction = (Button) base.FindName("buttonAction");
    }
  }
}
