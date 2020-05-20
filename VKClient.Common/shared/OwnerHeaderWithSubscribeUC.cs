using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Shared
{
  public class OwnerHeaderWithSubscribeUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal Image imageUserOrGroup;
    internal TextBlock textBlockName;
    private bool _contentLoaded;

    private OwnerHeaderWithSubscribeViewModel VM
    {
      get
      {
        return base.DataContext as OwnerHeaderWithSubscribeViewModel;
      }
    }

    public OwnerHeaderWithSubscribeUC()
    {
      this.InitializeComponent();
    }

    private void SubscribeUnsubscribeButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.SubscribeUnsubscribe( null);
    }

    private void gridOwnerTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.NavigateToOwner();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/OwnerHeaderWithSubscribeUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.imageUserOrGroup = (Image) base.FindName("imageUserOrGroup");
      this.textBlockName = (TextBlock) base.FindName("textBlockName");
    }
  }
}
