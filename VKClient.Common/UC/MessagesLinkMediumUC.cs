using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.MoneyTransfers;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class MessagesLinkMediumUC : NewsLinkUCBase
  {
    private const double SNIPPET_MIN_HEIGHT = 112.0;
    private const double SNIPPET_MAX_HEIGHT = 156.0;
    private readonly bool _isForwardedMessage;
    private string _imageUrl;
    private Link _link;
    private double _actualHeight;
    internal Grid LayoutRoot;
    internal Grid imageContainer;
    internal Image image;
    internal StackPanel textContainer;
    internal TextBlock titleBlock;
    internal TextBlock priceBlock;
    internal TextBlock descriptionBlock;
    internal StackPanel productContainer;
    internal TextBlock ratingPriceBlock;
    internal Rating ratingUC;
    internal TextBlock votesCountBlock;
    internal TextBlock captionBlock;
    internal Button actionButton;
    private bool _contentLoaded;

    public bool CanShowCard
    {
      get
      {
        if (this._isForwardedMessage && !this._link.money_transfer.IsOutbox)
          return this._link.money_transfer.IsInbox;
        return true;
      }
    }

    public MessagesLinkMediumUC(bool isForwardedMessage)
    {
      this.InitializeComponent();
      this._isForwardedMessage = isForwardedMessage;
      StackPanel productContainer = this.productContainer;
      TextBlock ratingPriceBlock = this.ratingPriceBlock;
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
      ((UIElement) ratingPriceBlock).Visibility = visibility8;
      Visibility visibility10 = visibility9;
      ((UIElement) productContainer).Visibility = visibility10;
      ((UIElement) this.priceBlock).Visibility = Visibility.Collapsed;
      ((UIElement) this.descriptionBlock).Visibility = Visibility.Collapsed;
    }

    public override void Initialize(Link link, double width, string parentPostId = "")
    {
      this._link = link;
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
      ((FrameworkElement) this.titleBlock).MaxHeight=(this.titleBlock.LineHeight * (hasProduct || hasRating ? 2.0 : 3.0));
      if (((FrameworkElement) this.titleBlock).ActualHeight < ((FrameworkElement) this.titleBlock).MaxHeight)
        ((FrameworkElement) this.descriptionBlock).MaxHeight=(((FrameworkElement) this.titleBlock).MaxHeight - ((FrameworkElement) this.titleBlock).ActualHeight);
      else
        ((UIElement) this.descriptionBlock).Visibility = Visibility.Collapsed;
      if (hasRating)
      {
        ((UIElement) this.productContainer).Visibility = Visibility.Visible;
        if (hasProduct)
        {
          ((UIElement) this.ratingPriceBlock).Visibility = Visibility.Visible;
          this.ratingPriceBlock.Text = link.product.price.text;
        }
        ((UIElement) this.ratingUC).Visibility = Visibility.Visible;
        this.ratingUC.Value = link.rating.stars;
        long reviewsCount = link.rating.reviews_count;
        if (reviewsCount > 0L)
        {
          ((UIElement) this.votesCountBlock).Visibility = Visibility.Visible;
          this.votesCountBlock.Text = (string.Format("({0})", UIStringFormatterHelper.FormatForUIVeryShort(reviewsCount)));
        }
      }
      if (!string.IsNullOrEmpty(link.caption) && !hasProduct | hasRating)
      {
        ((UIElement) this.captionBlock).Visibility = Visibility.Visible;
        this.captionBlock.Text = link.caption;
        ((FrameworkElement) this.captionBlock).Height=(((FrameworkElement) this.captionBlock).ActualHeight);
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.captionBlock);
      }
      LinkButton linkButton1 = link.button;
      if (linkButton1 == null & hasProduct)
      {
        Link link1 = link;
        LinkButton linkButton2 = new LinkButton();
        linkButton2.title = CommonResources.View;
        string str2 = string.Format("https://vk.com/product{0}_{1}", link.product.owner_id, link.product.id);
        linkButton2.url = str2;
        LinkButton linkButton3 = linkButton2;
        link1.button = linkButton2;
        linkButton1 = linkButton3;
      }
      if (link.money_transfer != null || !string.IsNullOrWhiteSpace(linkButton1 != null ? linkButton1.title :  null))
      {
        ((UIElement) this.actionButton).Visibility = Visibility.Visible;
        if (link.money_transfer == null)
          ((ContentControl) this.actionButton).Content = linkButton1.title;
        else
          ((ContentControl) this.actionButton).Content=(this.CanShowCard ? CommonResources.Open : CommonResources.AboutService);
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.actionButton);
      }
      this._actualHeight = this._actualHeight + Math.Min(((FrameworkElement) this.titleBlock).ActualHeight + ((FrameworkElement) this.priceBlock).ActualHeight + ((FrameworkElement) this.descriptionBlock).ActualHeight + ((FrameworkElement) this.ratingPriceBlock).ActualHeight, val2);
      this._actualHeight = Math.Min(this._actualHeight, 156.0);
      this._actualHeight = Math.Max(this._actualHeight, 112.0);
      double actualHeight = this._actualHeight;
      Thickness margin = ((FrameworkElement) this.textContainer).Margin;
      // ISSUE: explicit reference operation
      double top = ((Thickness) @margin).Top;
      margin = ((FrameworkElement) this.textContainer).Margin;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @margin).Bottom;
      double num = top + bottom;
      this._actualHeight = actualHeight + num;
      ((FrameworkElement) this.imageContainer).Height = this._actualHeight;
      this._imageUrl = this._link.money_transfer != null ? "/Resources/MoneyTransfers/Snippet.png" : link.photo.GetAppropriateForScaleFactor(this._actualHeight, 1);
      if (string.IsNullOrEmpty(this._link.url) || MetroInMotion.GetTilt((DependencyObject) this.LayoutRoot) == 1.2)
        return;
      MetroInMotion.SetTilt((DependencyObject) this.LayoutRoot, 1.2);
    }

    private void ComposeContentTextInlines(Link link, bool hasProduct, bool hasRating)
    {
      this.titleBlock.Text = (!string.IsNullOrWhiteSpace(link.title) ? link.title : CommonResources.Link);
      if (link.money_transfer != null)
        this.titleBlock.Text = link.money_transfer.amount.text;
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
      if (this._link.money_transfer == null)
      {
        LinkButton button = this._link.button;
        string url = (button != null ? button.url :  null) ?? this._link.url;
        if (string.IsNullOrEmpty(url))
          return;
        e.Handled = true;
        LinkButtonAction action = button.action;
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
      else
      {
        e.Handled = true;
        if (this.CanShowCard)
        {
          MoneyTransfer moneyTransfer = this._link.money_transfer;
          TransferCardView.Show(moneyTransfer.id, moneyTransfer.from_id, moneyTransfer.to_id);
        }
        else
        {
          string uri = "https://m.vk.com/landings/moneysend";
          string lang = LangHelper.GetLang();
          if (!string.IsNullOrEmpty(lang))
            uri += string.Format("?lang={0}", lang);
          Navigator.Current.NavigateToWebViewPage(uri, true);
        }
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      if (this._link.money_transfer == null)
        VeryLowProfileImageLoader.SetUriSource(this.image, this._imageUrl.ConvertToUri());
      else
        MultiResImageLoader.SetUriSource(this.image, this._imageUrl);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image,  null);
    }

    public override void ShownOnScreen()
    {
      string imageUrl = this._imageUrl;
      if ((imageUrl != null ? (imageUrl.ConvertToUri().IsAbsoluteUri ? 1 : 0) : 0) == 0)
        return;
      VeryLowProfileImageLoader.SetPriority(this._imageUrl, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MessagesLinkMediumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.imageContainer = (Grid) base.FindName("imageContainer");
      this.image = (Image) base.FindName("image");
      this.textContainer = (StackPanel) base.FindName("textContainer");
      this.titleBlock = (TextBlock) base.FindName("titleBlock");
      this.priceBlock = (TextBlock) base.FindName("priceBlock");
      this.descriptionBlock = (TextBlock) base.FindName("descriptionBlock");
      this.productContainer = (StackPanel) base.FindName("productContainer");
      this.ratingPriceBlock = (TextBlock) base.FindName("ratingPriceBlock");
      this.ratingUC = (Rating) base.FindName("ratingUC");
      this.votesCountBlock = (TextBlock) base.FindName("votesCountBlock");
      this.captionBlock = (TextBlock) base.FindName("captionBlock");
      this.actionButton = (Button) base.FindName("actionButton");
    }
  }
}
