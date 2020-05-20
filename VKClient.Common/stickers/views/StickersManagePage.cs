using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Stickers.Views
{
  public class StickersManagePage : PageBase
  {
    private bool _isInitialized;
    private StickersManageViewModel _viewModel;
    internal ProgressIndicator progressIndicator;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public StickersManagePage()
    {
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.textBlockTitle.Text = (CommonResources.MyStickers.ToUpperInvariant());
      this.ucHeader.OnHeaderTap = (Action) (() => {});
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._viewModel = new StickersManageViewModel()
      {
        StickersPackActivationHandler = new Action<bool>(this.StickersPackActivationHandler)
      };
      base.DataContext = this._viewModel;
      this._viewModel.Reload(true);
      this._isInitialized = true;
    }

    private void StickersPackActivationHandler(bool isActivating)
    {
      if (isActivating)
      {
        this.progressIndicator.IsVisible = true;
        this.progressIndicator.Text = CommonResources.Loading;
      }
      else
      {
        this.progressIndicator.IsVisible = false;
        this.progressIndicator.Text = ("");
      }
    }

    private void Deactivate_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      StockItemHeader stockItemHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as StockItemHeader;
      if (stockItemHeader == null)
        return;
      this._viewModel.Deactivate(stockItemHeader);
    }

    private void Activate_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      StockItemHeader stockItemHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as StockItemHeader;
      if (stockItemHeader == null)
        return;
      this._viewModel.Activate(stockItemHeader);
    }

    private void RectOverlay_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void RectOverlay_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void RectOverlay_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersManagePage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) base.FindName("progressIndicator");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
