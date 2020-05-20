using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPurchaseUC : UserControl
  {
    public const double HEIGHT = 92.0;
    internal Grid gridContainer;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockCaption;
    internal Button buttonPurchase;
    internal Rectangle rectSeparator;
    private bool _contentLoaded;

    public StickersPurchaseUC()
    {
      this.InitializeComponent();
      if (new ThemeHelper().PhoneLightThemeVisibility == Visibility.Visible)
      {
        Grid grid = this.gridContainer;
        SolidColorBrush solidColorBrush = new SolidColorBrush();
        solidColorBrush.Color = Colors.White;
        double num = 0.75;
        solidColorBrush.Opacity = num;
        grid.Background = (Brush) solidColorBrush;
      }
      else
        this.rectSeparator.Visibility = Visibility.Visible;
    }

    private void ButtonPurchase_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      StockItemHeader stockItemHeader = this.DataContext as StockItemHeader;
      if (stockItemHeader == null)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
      StorePurchaseManager.BuyStickersPack(stockItemHeader, "keyboard", null, null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPurchaseUC.xaml", UriKind.Relative));
      this.gridContainer = (Grid) this.FindName("gridContainer");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockCaption = (TextBlock) this.FindName("textBlockCaption");
      this.buttonPurchase = (Button) this.FindName("buttonPurchase");
      this.rectSeparator = (Rectangle) this.FindName("rectSeparator");
    }
  }
}
