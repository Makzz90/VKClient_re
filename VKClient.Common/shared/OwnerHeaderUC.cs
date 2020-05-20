using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Shared
{
  public class OwnerHeaderUC : UserControl
  {
    internal Grid LayoutRoot;
    internal Image imageUserOrGroup;
    internal TextBlock textBlockName;
    internal TextBlock textBlockDate;
    private bool _contentLoaded;

    private OwnerHeaderViewModel VM
    {
      get
      {
        return base.DataContext as OwnerHeaderViewModel;
      }
    }

    public OwnerHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.HandleTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/OwnerHeaderUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.imageUserOrGroup = (Image) base.FindName("imageUserOrGroup");
      this.textBlockName = (TextBlock) base.FindName("textBlockName");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
    }
  }
}
