using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
  public class CategoryMoreFooterUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal TextBlock textBlockFooter;
    private bool _contentLoaded;

    private CategoryMoreFooter VM
    {
      get
      {
        return base.DataContext as CategoryMoreFooter;
      }
    }

    public CategoryMoreFooterUC()
    {
      this.InitializeComponent();
    }

    private void OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.HandleTap.Invoke();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CategoryMoreFooterUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlockFooter = (TextBlock) base.FindName("textBlockFooter");
    }
  }
}
