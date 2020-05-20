using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.Views
{
  public class GiftsPage : PageBase
  {
    private bool _isInitialized;
    private GiftsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    public GiftsPage()
    {
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() => this._viewModel.GiftsVM.LoadData(true, false,  null, false));
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long userId = this.CommonParameters.UserId;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      string firstName = "";
      string firstNameGen = "";
      if (queryString.ContainsKey("FirstName"))
        firstName = queryString["FirstName"];
      if (queryString.ContainsKey("FirstNameGen"))
        firstNameGen = queryString["FirstNameGen"];
      if (userId != AppGlobalStateManager.Current.LoggedInUserId)
        this.BuildAppBar();
      this._viewModel = new GiftsViewModel(userId, firstName, firstNameGen);
      base.DataContext = this._viewModel;
      this._viewModel.GiftsVM.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton(new Uri("/Resources/AppBarGift.png", UriKind.Relative));
      string lowerInvariant = CommonResources.SendGift.ToLowerInvariant();
      applicationBarIconButton1.Text = lowerInvariant;
      ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
      applicationBarIconButton2.Click+=((EventHandler) ((sender, args) => this._viewModel.NavigateToGiftsCatalog()));
      applicationBar.Buttons.Add(applicationBarIconButton2);
      this.ApplicationBar = ((IApplicationBar) applicationBar);
    }

    private void ListBox_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.GiftsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void GiftListItemUC_OnDeleteClicked(object sender, EventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftHeader giftHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftHeader;
      if (giftHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gifts_page, GiftPurchaseStepsAction.delete));
      this._viewModel.Delete(giftHeader);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
