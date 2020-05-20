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
        return this.DataContext as OwnerHeaderViewModel;
      }
    }

    public OwnerHeaderUC()
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Shared/OwnerHeaderUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.imageUserOrGroup = (Image) this.FindName("imageUserOrGroup");
      this.textBlockName = (TextBlock) this.FindName("textBlockName");
      this.textBlockDate = (TextBlock) this.FindName("textBlockDate");
    }
  }
}
