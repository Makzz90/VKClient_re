using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Shared
{
  public class ListHeaderUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal StackPanel MoreContainerPanel;
    private bool _contentLoaded;

    private ListHeaderViewModel VM
    {
      get
      {
        return this.DataContext as ListHeaderViewModel;
      }
    }

    public ListHeaderUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      this.VM.HandleTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Shared/ListHeaderUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.MoreContainerPanel = (StackPanel) this.FindName("MoreContainerPanel");
    }
  }
}
