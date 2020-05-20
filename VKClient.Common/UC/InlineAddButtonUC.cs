using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class InlineAddButtonUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBlock textBlock;
    private bool _contentLoaded;

    public Action OnAdd { get; set; }

    public string Text
    {
      get
      {
        return this.textBlock.Text;
      }
      set
      {
        this.textBlock.Text = value;
      }
    }

    public InlineAddButtonUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void OnAddTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnAdd == null)
        return;
      this.OnAdd();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/InlineAddButtonUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlock = (TextBlock) base.FindName("textBlock");
    }
  }
}
