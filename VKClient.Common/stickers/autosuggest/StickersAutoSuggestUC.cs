using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Shared.ImagePreview;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Stickers.Views;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.AutoSuggest
{
  public class StickersAutoSuggestUC : UserControl
  {
    private int currentStickerId;
    private DateTime _lastHiddenTime;
    internal Grid LayoutRoot;
    internal ScrollViewer scrollViewer;
    private bool _contentLoaded;

    private StickersAutoSuggestViewModel VM
    {
      get
      {
        return base.DataContext as StickersAutoSuggestViewModel;
      }
    }

    public Action AutoSuggestStickerSentCallback { get; set; }

    public Action AutoSuggestStickerSendingCallback { get; set; }

    public bool HasItemsToShow
    {
      get
      {
        return Enumerable.Any<StickersAutoSuggestItem>(this.VM.AutoSuggestCollection);
      }
    }

    public StickersAutoSuggestUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.DataContext = (new StickersAutoSuggestViewModel());
    }

    public void ShowHide(bool show)
    {
        if (show && this.Visibility == Visibility.Collapsed)
        {
            this.Visibility = Visibility.Visible;
            if ((DateTime.Now - this._lastHiddenTime).TotalMilliseconds < 700.0)
            {
                this.Opacity = 1.0;
            }
            else
            {
                this.Opacity = 0.0;
                this.Animate(0.0, 1.0, (object)UIElement.OpacityProperty, 250, new int?(0), null, null, false);
            }
        }
        else
        {
            if (show || this.Visibility != Visibility.Visible)
                return;
            this.Visibility = Visibility.Collapsed;
            this.Opacity = 0.0;
            this._lastHiddenTime = DateTime.Now;
        }
    }

    public void SetData(IEnumerable<StickersAutoSuggestItem> items, string keyword)
    {
      this.scrollViewer.ScrollToHorizontalOffset(0.0);
      ((FrameworkElement)this.scrollViewer).Margin=(Enumerable.Count<StickersAutoSuggestItem>(items) == 1 ? new Thickness(17.0, 0.0, 0.0, 0.0) : new Thickness());
      this.VM.SetItems(items);
      this.VM.Keyword = keyword;
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (PreviewBehavior.JustShown)
        return;
      StickersAutoSuggestItem dataContext = (sender as FrameworkElement).DataContext as StickersAutoSuggestItem;
      if (!dataContext.IsPromoted)
      {
        Action stickerSendingCallback = this.AutoSuggestStickerSendingCallback;
        if (stickerSendingCallback != null)
          stickerSendingCallback();
        string str = !string.IsNullOrEmpty(this.VM.Keyword) ? StickerReferrer.FromKeyword(this.VM.Keyword) : "keyboard";
        EventAggregator.Current.Publish(new StickerItemTapEvent()
        {
          StickerItem = dataContext.StickerData,
          Referrer = str
        });
        Action stickerSentCallback = this.AutoSuggestStickerSentCallback;
        if (stickerSentCallback != null)
          stickerSentCallback();
      }
      else
      {
        string referrer = !string.IsNullOrEmpty(this.VM.Keyword) ? StickerReferrer.FromKeyword(this.VM.Keyword) : "store";
        CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
        this.currentStickerId = dataContext.StickerData.StickerId;
        StickersPackView.ShowAndReloadStickers((long) this.currentStickerId, referrer);
      }
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/AutoSuggest/StickersAutoSuggestUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
    }
  }
}
