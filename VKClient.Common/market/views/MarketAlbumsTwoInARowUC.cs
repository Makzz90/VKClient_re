using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Market.Views
{
  public class MarketAlbumsTwoInARowUC : UserControl
  {
    private bool _contentLoaded;

    public MarketAlbumsTwoInARowUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/MarketAlbumsTwoInARowUC.xaml", UriKind.Relative));
    }
  }
}
