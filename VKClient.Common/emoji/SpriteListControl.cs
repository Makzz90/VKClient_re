using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Stickers.Views;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public class SpriteListControl : UserControl, ISupportDataContext, ISupportState, IHandle<StickersSettings.StickerRecentItemInsertedEvent>, IHandle, IHandle<PanelControlFocusedChangedEvent>, IHandle<PanelControlOpenedChangedEvent>, IHandle<DownloadSucceededEvent>, IHandle<DownloadFailedEvent>
  {
    private bool _isActive;
    private bool _isDataContextUpdateScheduled;
    internal Border borderExtraTop;
    internal Image ImageBackground;
    internal ProgressBar ProgressBar;
    internal ScrollViewer scrollViewer;
    internal MyVirtualizingPanel myPanel;
    internal Border borderStickersPurchase;
    internal TextBlock textBlockNoStickers;
    private bool _contentLoaded;

    public bool IsHorizontal
    {
      get
      {
        PageOrientation orientation = ((PhoneApplicationFrame) Application.Current.RootVisual).Orientation;
        if (orientation != PageOrientation.Landscape && orientation != PageOrientation.LandscapeLeft)
          return orientation == PageOrientation.LandscapeRight;
        return true;
      }
    }

    private SpriteListItemData CurrentDataContext { get; set; }

    public MyVirtualizingPanel MyPanel
    {
      get
      {
        return this.myPanel;
      }
    }

    public SpriteListControl()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.myPanel.InitializeWithScrollViewer(this.scrollViewer, false);
      EventAggregator.Current.Subscribe(this);
      base.DataContext = this.CurrentDataContext;
    }

    public void SetDataContext(object obj)
    {
      this.myPanel.ClearItems();
      ((UIElement) this.ProgressBar).Visibility = Visibility.Collapsed;
      ImageLoader.SetUriSource(this.ImageBackground,  null);
      if (obj == null)
      {
        this.CurrentDataContext =  null;
      }
      else
      {
        SpriteListItemData spriteListItemData = (SpriteListItemData) obj;
        this.CurrentDataContext = spriteListItemData;
        ((FrameworkElement) this.borderStickersPurchase).DataContext = spriteListItemData;
        ((FrameworkElement) this.borderExtraTop).DataContext = spriteListItemData;
        ((FrameworkElement) this.textBlockNoStickers).DataContext = spriteListItemData;
        if (spriteListItemData.IsEmoji)
          this.CreateAndAddSprites(spriteListItemData);
        else
          this.CreateAndAddStickersItems();
      }
    }

    public void SetState(bool isActive)
    {
      this._isActive = isActive;
      if (this._isActive || !this._isDataContextUpdateScheduled || (this.CurrentDataContext == null || !this.CurrentDataContext.IsRecentStickers))
        return;
      this._isDataContextUpdateScheduled = false;
      this.CreateAndAddStickersItems();
    }

    private void CreateAndAddSprites(SpriteListItemData item)
    {
      EmojiSpritesCreator eSpC = new EmojiSpritesCreator(this.CurrentDataContext.StickerProduct.id);
      if (!eSpC.TryDeserializeSpritePack())
      {
        ((UIElement) this.ProgressBar).Visibility = Visibility.Visible;
        this.ProgressBar.IsIndeterminate = true;
        ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
        {
          Thread.Sleep(800);
          if (!item.IsSelected)
            return;
          base.Dispatcher.BeginInvoke((Action) (() =>
          {
            eSpC.CreateSprites();
            this.AddSprites(eSpC);
            ((UIElement) this.ProgressBar).Visibility = Visibility.Collapsed;
            this.ProgressBar.IsIndeterminate = false;
          }));
        }));
      }
      else
        this.AddSprites(eSpC);
    }

    private void CreateAndAddStickersItems()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
          if ((Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible && !this.CurrentDataContext.IsRecentStickers)
          ImageLoader.SetUriSource(this.ImageBackground, this.CurrentDataContext.StickerProduct.base_url + "/background.png?" + VeryLowProfileImageLoader.REQUIRE_CACHING_KEY + "=True");
        List<IEnumerable<int>> list1 = (List<IEnumerable<int>>) Enumerable.ToList<IEnumerable<int>>(this.CurrentDataContext.StickerProduct.stickers.sticker_ids.Partition<int>(StickersConstants.ColumnsCountVerticalOrientation));
        List<IEnumerable<int>> list2 = (List<IEnumerable<int>>) Enumerable.ToList<IEnumerable<int>>(this.CurrentDataContext.StickerProduct.stickers.sticker_ids.Partition<int>(StickersConstants.ColumnsCountHorizontalOrientation));
        List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
        for (int index = 0; index < list1.Count; ++index)
        {
          List<StickerItemData> verticalStickers = new List<StickerItemData>();
          List<StickerItemData> horizontalStickers = new List<StickerItemData>();
          IEnumerator<int> enumerator1 = list1[index].GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator1).MoveNext())
            {
              int current = enumerator1.Current;
              List<StickerItemData> stickerItemDataList = verticalStickers;
              StickerItemData stickerItemData = new StickerItemData();
              stickerItemData.LocalPath = StickersDownloader.Instance.GetLocalPathForStickerId128(this.CurrentDataContext.StickerProduct, current);
              stickerItemData.LocalPathBig = StickersDownloader.Instance.GetLocalPathForStickerId256(this.CurrentDataContext.StickerProduct, current);
              stickerItemData.LocalPathExtraBig = StickersDownloader.Instance.GetLocalPathForStickerId512(this.CurrentDataContext.StickerProduct, current);
              stickerItemData.StickerSetId = this.CurrentDataContext.StickerProduct.id;
              stickerItemData.StickerId = current;
              int num = this.CurrentDataContext.StickerStockItemHeader.IsPurchased ? 1 : (this.CurrentDataContext.IsRecentStickers ? 1 : 0);
              stickerItemData.CanSendSticker = num != 0;
              stickerItemDataList.Add(stickerItemData);
            }
          }
          finally
          {
            if (enumerator1 != null)
              ((IDisposable) enumerator1).Dispose();
          }
          if (index < list2.Count)
          {
            IEnumerator<int> enumerator2 = list2[index].GetEnumerator();
            try
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                int current = enumerator2.Current;
                List<StickerItemData> stickerItemDataList = horizontalStickers;
                StickerItemData stickerItemData = new StickerItemData();
                stickerItemData.LocalPath = StickersDownloader.Instance.GetLocalPathForStickerId128(this.CurrentDataContext.StickerProduct, current);
                stickerItemData.LocalPathBig = StickersDownloader.Instance.GetLocalPathForStickerId256(this.CurrentDataContext.StickerProduct, current);
                stickerItemData.LocalPathExtraBig = StickersDownloader.Instance.GetLocalPathForStickerId512(this.CurrentDataContext.StickerProduct, current);
                stickerItemData.StickerSetId = this.CurrentDataContext.StickerProduct.id;
                stickerItemData.StickerId = current;
                int num = this.CurrentDataContext.StickerStockItemHeader.IsPurchased ? 1 : (this.CurrentDataContext.IsRecentStickers ? 1 : 0);
                stickerItemData.CanSendSticker = num != 0;
                stickerItemDataList.Add(stickerItemData);
              }
            }
            finally
            {
              if (enumerator2 != null)
                ((IDisposable) enumerator2).Dispose();
            }
          }
          StickersItem stickersItem = new StickersItem(StickersConstants.VerticalSpriteWidth, new Thickness(), StickersConstants.HorizontalSpriteWidth, this.IsHorizontal, StickersConstants.StickerHeight, StickersConstants.ColumnsCountVerticalOrientation, StickersConstants.ColumnsCountHorizontalOrientation, verticalStickers, horizontalStickers);
          virtualizableList.Add((IVirtualizable) stickersItem);
        }
        this.myPanel.ClearItems();
        this.myPanel.AddItems((IEnumerable<IVirtualizable>) virtualizableList);
      }));
    }

    private void AddSprites(EmojiSpritesCreator eSpC)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        SpritesPack spritesPack = eSpC.SpritesPack;
        List<SpriteDescription> spritesVertical = spritesPack.SpritesVertical;
        List<SpriteDescription> spritesHorizontal = spritesPack.SpritesHorizontal;
        this.myPanel.ClearItems();
        List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
        for (int index = 0; index < spritesVertical.Count || index < spritesHorizontal.Count; ++index)
        {
          SpriteItem spriteItem = new SpriteItem(index < spritesVertical.Count ? spritesVertical[index] : new SpriteDescription(), index < spritesHorizontal.Count ? spritesHorizontal[index] : new SpriteDescription(), this.IsHorizontal);
          if (index == 0)
            spriteItem.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
          virtualizableList.Add((IVirtualizable) spriteItem);
        }
        this.myPanel.AddItems((IEnumerable<IVirtualizable>) virtualizableList);
      }));
    }

    public void Handle(StickersSettings.StickerRecentItemInsertedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !this.CurrentDataContext.IsRecentStickers)
          return;
        if (!this._isActive)
          this.CreateAndAddStickersItems();
        else
          this._isDataContextUpdateScheduled = true;
      }));
    }

    public void Handle(PanelControlFocusedChangedEvent message)
    {
      if (message.IsFocused)
        return;
      this.UpdateRecentStickers();
    }

    public void Handle(PanelControlOpenedChangedEvent message)
    {
      if (message.IsOpened)
        return;
      this.UpdateRecentStickers();
    }

    private void UpdateRecentStickers()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !this.CurrentDataContext.IsRecentStickers)
          return;
        this.CreateAndAddStickersItems();
      }));
    }

    public void Handle(DownloadSucceededEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        ((UIElement) this.ProgressBar).Visibility = Visibility.Collapsed;
        this.CreateAndAddStickersItems();
      }));
    }

    public void Handle(DownloadFailedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        ((UIElement) this.ProgressBar).Visibility = Visibility.Collapsed;
      }));
    }

    private void GridRoot_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.CurrentDataContext.IsStickersPack)
        return;
      StockItemHeader stickerStockItemHeader = this.CurrentDataContext.StickerStockItemHeader;
      if (stickerStockItemHeader == null || stickerStockItemHeader.IsPurchased)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
      StickersPackView.Show(stickerStockItemHeader, "keyboard");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Emoji/SpriteListControl.xaml", UriKind.Relative));
      this.borderExtraTop = (Border) base.FindName("borderExtraTop");
      this.ImageBackground = (Image) base.FindName("ImageBackground");
      this.ProgressBar = (ProgressBar) base.FindName("ProgressBar");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.myPanel = (MyVirtualizingPanel) base.FindName("myPanel");
      this.borderStickersPurchase = (Border) base.FindName("borderStickersPurchase");
      this.textBlockNoStickers = (TextBlock) base.FindName("textBlockNoStickers");
    }
  }
}
