using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.VideoCatalog;

namespace VKClient.Video.VideoCatalog
{
    public partial class CatalogItemCompactUC : UserControlVirtualizable
  {

    public CatalogItemCompactUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      (this.DataContext as CatalogItemViewModel).HandleTap();
    }

  }
}
