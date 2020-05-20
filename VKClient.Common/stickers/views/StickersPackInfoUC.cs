using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPackInfoUC : UserControl
  {
      public static readonly DependencyProperty NewIndicatorEnabledProperty = DependencyProperty.Register("NewIndicatorEnabled", typeof(bool), typeof(StickersPackInfoUC), new PropertyMetadata(true, new PropertyChangedCallback(StickersPackInfoUC.NewIndicatorEnabled_OnChanged)));
    private bool _isActivating;
    internal Border borderNewIndicator;
    private bool _contentLoaded;

    public bool NewIndicatorEnabled
    {
      get
      {
        return (bool) base.GetValue(StickersPackInfoUC.NewIndicatorEnabledProperty);
      }
      set
      {
        base.SetValue(StickersPackInfoUC.NewIndicatorEnabledProperty, value);
      }
    }

    public string Referrer { get; set; }

    private StockItemHeader ViewModel
    {
      get
      {
        return base.DataContext as StockItemHeader;
      }
    }

    public StickersPackInfoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void NewIndicatorEnabled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((UIElement) ((StickersPackInfoUC) d).borderNewIndicator).Visibility = (((bool) e.NewValue).ToVisiblity());
    }

    private void ButtonBuy_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      StockItemHeader viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      StorePurchaseManager.BuyStickersPack(viewModel, this.Referrer,  null,  null);
    }

    private void Add_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      StockItemHeader viewModel = this.ViewModel;
      if (viewModel == null || this._isActivating)
        return;
      this._isActivating = true;
      StorePurchaseManager.ActivateStickersPack(viewModel, (Action<bool>) (activated => Execute.ExecuteOnUIThread((Action) (() => this._isActivating = false))));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackInfoUC.xaml", UriKind.Relative));
      this.borderNewIndicator = (Border) base.FindName("borderNewIndicator");
    }
  }
}
