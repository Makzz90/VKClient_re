using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPackListItemUC : UserControlVirtualizable
  {
    public const int FIXED_HEIGHT = 100;
    private bool _contentLoaded;

    public StickersPackListItemUC()
    {
      this.InitializeComponent();
    }

    private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      StockItemHeader dataContext = base.DataContext as StockItemHeader;
      if (dataContext == null)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
      StickersPackView.Show(dataContext, "store");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackListItemUC.xaml", UriKind.Relative));
    }
  }
}
