using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Gifts.Views
{
  public class GiftsThreeInARowUC : UserControl
  {
    private bool _contentLoaded;

    public event EventHandler ItemTap;

    public GiftsThreeInARowUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Item_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler itemTap = this.ItemTap;
      if (itemTap == null)
        return;
      object sender1 = sender;
      EventArgs empty = EventArgs.Empty;
      itemTap(sender1, empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsThreeInARowUC.xaml", UriKind.Relative));
    }
  }
}
