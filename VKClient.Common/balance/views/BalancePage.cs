using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Library;
using VKClient.Common.Balance.ViewModels;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Balance.Views
{
  public class BalancePage : PageBase
  {
    private bool _isInitialized;
    private BalanceViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    internal Hyperlink hyperlinkLicenseAgreement;
    private bool _contentLoaded;

    public BalancePage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Balance.ToUpperInvariant());
      this.InitLicenseAgreementHyperlink();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._viewModel = new BalanceViewModel();
      base.DataContext = this._viewModel;
      this._viewModel.Reload(true);
      this._isInitialized = true;
    }

    private void InitLicenseAgreementHyperlink()
    {
      ((Inline) this.hyperlinkLicenseAgreement).TextDecorations=( null);
      this.hyperlinkLicenseAgreement.MouseOverTextDecorations=( null);
      SolidColorBrush solidColorBrush = new SolidColorBrush(((SolidColorBrush) ((TextElement) this.hyperlinkLicenseAgreement).Foreground).Color);
      double num = 0.667;
      ((Brush) solidColorBrush).Opacity = num;
      this.hyperlinkLicenseAgreement.MouseOverForeground=((Brush) solidColorBrush);
    }

    private void HyperlinkLicenseAgreement_OnClick(object sender, RoutedEventArgs e)
    {
      string uri = "https://m.vk.com/licence?api_view=1";
      string lang = LangHelper.GetLang();
      if (!string.IsNullOrEmpty(lang))
        uri += string.Format("&lang={0}", lang);
      Navigator.Current.NavigateToWebUri(uri, true, false);
    }

    private void OnOptionsMenuItemSelected(object sender, OptionsMenuItemType e)
    {
      if (e != OptionsMenuItemType.More)
        return;
      this.ShowMoreOptions();
    }

    private void ShowMoreOptions()
    {
      List<MenuItem> menuItems = new List<MenuItem>();
      MenuItem menuItem1 = new MenuItem();
      string lowerInvariant = CommonResources.RestorePurchases.ToLowerInvariant();
      menuItem1.Header = lowerInvariant;
      MenuItem menuItem2 = menuItem1;
      // ISSUE: method pointer
      menuItem2.Click += new RoutedEventHandler( this.MenuItemRestorePurchases_OnClick);
      menuItems.Add(menuItem2);
      this.ucHeader.SetMenu(menuItems);
      this.ucHeader.ShowMenu();
    }

    private void MenuItemRestorePurchases_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      StorePurchaseManager.RestorePurchases( null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Balance/Views/BalancePage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.hyperlinkLicenseAgreement = (Hyperlink) base.FindName("hyperlinkLicenseAgreement");
    }
  }
}
