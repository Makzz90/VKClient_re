using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class ShowMoreCommentsUC : UserControlVirtualizable
  {
    internal TextBlock textBlockText;
    private bool _contentLoaded;

    public Action OnClickAction { get; set; }

    public string Text
    {
      get
      {
        return this.textBlockText.Text;
      }
      set
      {
        this.textBlockText.Text = value;
      }
    }

    public ShowMoreCommentsUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnClickAction == null)
        return;
      this.OnClickAction();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ShowMoreCommentsUC.xaml", UriKind.Relative));
      this.textBlockText = (TextBlock) base.FindName("textBlockText");
    }
  }
}
