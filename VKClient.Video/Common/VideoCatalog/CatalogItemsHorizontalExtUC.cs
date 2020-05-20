using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Video.VideoCatalog;

namespace VKClient.Common.VideoCatalog
{
    public partial class CatalogItemsHorizontalExtUC : UserControlVirtualizable
  {
    private CatalogItemsHorizontalViewModel VM
    {
      get
      {
        return this.DataContext as CatalogItemsHorizontalViewModel;
      }
    }

    private AlbumsListHorizontalViewModel AlbumsVM
    {
      get
      {
        return this.DataContext as AlbumsListHorizontalViewModel;
      }
    }

    public CatalogItemsHorizontalExtUC()
    {
      this.InitializeComponent();
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      if (this.horizontalListBox.ManipulationState == ManipulationState.Idle)
        return;
      if (this.VM != null)
      {
        this.VM.CatalogItemsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
      }
      else
      {
        if (this.AlbumsVM == null)
          return;
        this.AlbumsVM.CatalogItemsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
      }
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

  }
}
