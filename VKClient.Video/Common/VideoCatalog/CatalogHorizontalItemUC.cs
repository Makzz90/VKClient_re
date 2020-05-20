using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
    public partial class CatalogHorizontalItemUC : UserControlVirtualizable
  {
    private CatalogItemViewModel VM
    {
      get
      {
        return this.DataContext as CatalogItemViewModel;
      }
    }

    public CatalogHorizontalItemUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      this.VM.HandleTap();
    }

  }
}
