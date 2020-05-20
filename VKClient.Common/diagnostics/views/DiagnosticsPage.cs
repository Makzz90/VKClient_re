using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Diagnostics.ViewModels;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Diagnostics.Views
{
  public class DiagnosticsPage : PageBase
  {
    private bool _isInitialized;
    private DiagnosticsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public DiagnosticsPage()
    {
      this.InitializeComponent();
      this.ucHeader.Title = CommonResources.Diagnostics.ToUpperInvariant();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._viewModel = new DiagnosticsViewModel();
      base.DataContext = this._viewModel;
      this._isInitialized = true;
    }

    private void SendData_OnClicked(object sender, RoutedEventArgs e)
    {
      this._viewModel.SendData();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Diagnostics/Views/DiagnosticsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
