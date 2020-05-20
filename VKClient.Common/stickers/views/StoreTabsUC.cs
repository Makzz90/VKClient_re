using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.Views
{
  public class StoreTabsUC : UserControl
  {
    private bool _contentLoaded;

    private StickersStoreViewModel ViewModel
    {
      get
      {
        return base.DataContext as StickersStoreViewModel;
      }
    }

    public StoreTabsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void TabPopular_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.StickersListSource = StickersStoreViewModel.CurrentSource.Popular;
    }

    private void TabNew_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.StickersListSource = StickersStoreViewModel.CurrentSource.New;
    }

    private void TabFree_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.StickersListSource = StickersStoreViewModel.CurrentSource.Free;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StoreTabsUC.xaml", UriKind.Relative));
    }
  }
}
