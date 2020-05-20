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
        public static readonly DependencyProperty NewIndicatorEnabledProperty = DependencyProperty.Register("NewIndicatorEnabled", typeof(bool), typeof(StickersPackInfoUC), new PropertyMetadata((object)true, new PropertyChangedCallback(StickersPackInfoUC.NewIndicatorEnabled_OnChanged)));
        private bool _isActivating;
        internal Border borderNewIndicator;
        private bool _contentLoaded;

        public string Referrer { get; set; }

        public bool NewIndicatorEnabled
        {
            get
            {
                return (bool)this.GetValue(StickersPackInfoUC.NewIndicatorEnabledProperty);
            }
            set
            {
                this.SetValue(StickersPackInfoUC.NewIndicatorEnabledProperty, (object)value);
            }
        }

        private StockItemHeader ViewModel
        {
            get
            {
                return this.DataContext as StockItemHeader;
            }
        }

        public StickersPackInfoUC()
        {
            this.Referrer = "store";

            this.InitializeComponent();
        }

        private static void NewIndicatorEnabled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StickersPackInfoUC)d).borderNewIndicator.Visibility = ((bool)e.NewValue).ToVisiblity();
        }

        private void ButtonBuy_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            StockItemHeader viewModel = this.ViewModel;
            if (viewModel == null)
                return;
            StorePurchaseManager.BuyStickersPack(viewModel, this.Referrer, null, null);
        }

        private void Add_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            StockItemHeader viewModel = this.ViewModel;
            if (viewModel == null || this._isActivating)
                return;
            this._isActivating = true;
            StorePurchaseManager.ActivateStickersPack(viewModel, (Action<bool>)(activated => Execute.ExecuteOnUIThread((Action)(() => this._isActivating = false))));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackInfoUC.xaml", UriKind.Relative));
            this.borderNewIndicator = (Border)this.FindName("borderNewIndicator");
        }
    }
}
