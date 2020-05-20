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
      //base.\u002Ector();
      this.InitializeComponent();
      if (new ThemeHelper().PhoneLightThemeVisibility == Visibility.Visible)
      {
        Grid gridContainer = this.gridContainer;
        SolidColorBrush solidColorBrush = new SolidColorBrush();
        Color white = Colors.White;
        solidColorBrush.Color = white;
        double num = 0.75;
        ((Brush) solidColorBrush).Opacity = num;
        ((Panel) gridContainer).Background = ((Brush) solidColorBrush);
      }
      else
        ((UIElement) this.rectSeparator).Visibility = Visibility.Visible;
    }

    private void ButtonPurchase_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      StockItemHeader dataContext = base.DataContext as StockItemHeader;
      if (dataContext == null)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
      StorePurchaseManager.BuyStickersPack(dataContext, "keyboard",  null,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPurchaseUC.xaml", UriKind.Relative));
      this.gridContainer = (Grid) base.FindName("gridContainer");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockCaption = (TextBlock) base.FindName("textBlockCaption");
      this.buttonPurchase = (Button) base.FindName("buttonPurchase");
      this.rectSeparator = (Rectangle) base.FindName("rectSeparator");
    }
  }
}
