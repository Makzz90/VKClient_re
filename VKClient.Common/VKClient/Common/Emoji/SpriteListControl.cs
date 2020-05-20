using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        switch (orientation)
        {
          case PageOrientation.Landscape:
          case PageOrientation.LandscapeLeft:
            return true;
          default:
            return orientation == PageOrientation.LandscapeRight;
        }
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
      this.InitializeComponent();
      this.myPanel.InitializeWithScrollViewer(this.scrollViewer, false);
      EventAggregator.Current.Subscribe((object) this);
      this.DataContext = (object) this.CurrentDataContext;
    }

    public void SetDataContext(object obj)
    {
      this.myPanel.ClearItems();
      this.ProgressBar.Visibility = Visibility.Collapsed;
      ImageLoader.SetUriSource(this.ImageBackground, null);
      if (obj == null)
      {
        this.CurrentDataContext = (SpriteListItemData) null;
      }
      else
      {
        SpriteListItemData spriteListItemData = (SpriteListItemData) obj;
        this.CurrentDataContext = spriteListItemData;
        this.borderStickersPurchase.DataContext = (object) spriteListItemData;
        this.borderExtraTop.DataContext = (object) spriteListItemData;
        this.textBlockNoStickers.DataContext = (object) spriteListItemData;
        if (spriteListItemData.IsEmoji)
          this.CreateAndAddSprites();
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

    private void CreateAndAddSprites()
    {
      EmojiSpritesCreator eSpC = new EmojiSpritesCreator(this.CurrentDataContext.StickerProduct.id);
      if (!eSpC.TryDeserializeSpritePack())
      {
        this.ProgressBar.Visibility = Visibility.Visible;
        this.ProgressBar.IsIndeterminate = true;
        ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
        {
          Thread.Sleep(800);
          this.Dispatcher.BeginInvoke((Action) (() =>
          {
            eSpC.CreateSprites();
            this.AddSprites(eSpC);
            this.ProgressBar.Visibility = Visibility.Collapsed;
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
        if ((Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible && !this.CurrentDataContext.IsRecentStickers)
          ImageLoader.SetUriSource(this.ImageBackground, this.CurrentDataContext.StickerProduct.base_url + "/background.png?" + VeryLowProfileImageLoader.REQUIRE_CACHING_KEY + "=True");
        List<IEnumerable<int>> list1 = this.CurrentDataContext.StickerProduct.stickers.sticker_ids.Partition<int>(StickersConstants.ColumnsCountVerticalOrientation).ToList<IEnumerable<int>>();
        List<IEnumerable<int>> list2 = this.CurrentDataContext.StickerProduct.stickers.sticker_ids.Partition<int>(StickersConstants.ColumnsCountHorizontalOrientation).ToList<IEnumerable<int>>();
        List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
        for (int index = 0; index < list1.Count; ++index)
        {
          List<StickerItemData> verticalStickers = new List<StickerItemData>();
          List<StickerItemData> horizontalStickers = new List<StickerItemData>();
          foreach (int stickerId in list1[index])
          {
            List<StickerItemData> stickerItemDataList = verticalStickers;
            StickerItemData stickerItemData = new StickerItemData();
            stickerItemData.LocalPath = StickersDownloader.Instance.GetLocalPathForStickerId128(this.CurrentDataContext.StickerProduct, stickerId);
            stickerItemData.LocalPathBig = StickersDownloader.Instance.GetLocalPathForStickerId256(this.CurrentDataContext.StickerProduct, stickerId);
            stickerItemData.LocalPathExtraBig = StickersDownloader.Instance.GetLocalPathForStickerId512(this.CurrentDataContext.StickerProduct, stickerId);
            stickerItemData.StickerSetId = this.CurrentDataContext.StickerProduct.id;
            stickerItemData.StickerId = stickerId;
            int num = this.CurrentDataContext.StickerStockItemHeader.IsPurchased ? 1 : (this.CurrentDataContext.IsRecentStickers ? 1 : 0);
            stickerItemData.CanSendSticker = num != 0;
            stickerItemDataList.Add(stickerItemData);
          }
          if (index < list2.Count)
          {
            foreach (int stickerId in list2[index])
            {
              List<StickerItemData> stickerItemDataList = horizontalStickers;
              StickerItemData stickerItemData = new StickerItemData();
              stickerItemData.LocalPath = StickersDownloader.Instance.GetLocalPathForStickerId128(this.CurrentDataContext.StickerProduct, stickerId);
              stickerItemData.LocalPathBig = StickersDownloader.Instance.GetLocalPathForStickerId256(this.CurrentDataContext.StickerProduct, stickerId);
              stickerItemData.LocalPathExtraBig = StickersDownloader.Instance.GetLocalPathForStickerId512(this.CurrentDataContext.StickerProduct, stickerId);
              stickerItemData.StickerSetId = this.CurrentDataContext.StickerProduct.id;
              stickerItemData.StickerId = stickerId;
              int num = this.CurrentDataContext.StickerStockItemHeader.IsPurchased ? 1 : (this.CurrentDataContext.IsRecentStickers ? 1 : 0);
              stickerItemData.CanSendSticker = num != 0;
              stickerItemDataList.Add(stickerItemData);
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
        for (int index = 0; index < spritesHorizontal.Count; ++index)
        {
          SpriteDescription vertSpriteDesc;
          if (index >= spritesVertical.Count)
            vertSpriteDesc = new SpriteDescription()
            {
              SpritePath = "",
              Elements = new List<SpriteElementData>()
            };
          else
            vertSpriteDesc = spritesVertical[index];
          SpriteDescription horSpriteDesc = spritesHorizontal[index];
          int num = this.IsHorizontal ? 1 : 0;
          SpriteItem spriteItem = new SpriteItem(vertSpriteDesc, horSpriteDesc, num != 0);
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
        this.ProgressBar.Visibility = Visibility.Collapsed;
        this.CreateAndAddStickersItems();
      }));
    }

    public void Handle(DownloadFailedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        this.ProgressBar.Visibility = Visibility.Collapsed;
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Emoji/SpriteListControl.xaml", UriKind.Relative));
      this.borderExtraTop = (Border) this.FindName("borderExtraTop");
      this.ImageBackground = (Image) this.FindName("ImageBackground");
      this.ProgressBar = (ProgressBar) this.FindName("ProgressBar");
      this.scrollViewer = (ScrollViewer) this.FindName("scrollViewer");
      this.myPanel = (MyVirtualizingPanel) this.FindName("myPanel");
      this.borderStickersPurchase = (Border) this.FindName("borderStickersPurchase");
      this.textBlockNoStickers = (TextBlock) this.FindName("textBlockNoStickers");
    }
  }
}
