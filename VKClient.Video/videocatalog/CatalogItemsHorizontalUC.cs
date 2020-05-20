using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogItemsHorizontalUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector horizontalListBox;
    private bool _contentLoaded;

    private CatalogItemsHorizontalViewModel VM
    {
      get
      {
        return base.DataContext as CatalogItemsHorizontalViewModel;
      }
    }

    public CatalogItemsHorizontalUC()
    {
      this.InitializeComponent();
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
        if (this.horizontalListBox.ManipulationState == System.Windows.Controls.Primitives.ManipulationState.Idle)
        return;
      this.VM.CatalogItemsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void horizontalListBox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void horizontalListBox_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void horizontalListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CatalogItemsHorizontalUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.horizontalListBox = (ExtendedLongListSelector) base.FindName("horizontalListBox");
    }
  }
}
