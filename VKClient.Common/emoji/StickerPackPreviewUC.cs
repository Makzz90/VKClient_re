using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Localization;

namespace VKClient.Common.Emoji
{
  public class StickerPackPreviewUC : UserControl, IHandle<DownloadSucceededEvent>, IHandle, IHandle<DownloadFailedEvent>, IHandle<DownloadProgressedEvent>
  {
    internal Image previewImage;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockAuthor;
    internal TextBlock textBlockDesc;
    internal Button ButtonDownload;
    internal ProgressBar progressBar;
    private bool _contentLoaded;

    public SpriteListItemData CurrentDataContext { get; private set; }

    public StickerPackPreviewUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((ContentControl) this.ButtonDownload).Content = CommonResources.download;
      EventAggregator.Current.Subscribe(this);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      SpriteListItemData currentDataContext = this.CurrentDataContext;
      if ((currentDataContext != null ? currentDataContext.StickerProduct :  null) == null)
        return;
      StickersDownloader.Instance.InitiateDownload(this.CurrentDataContext.StickerProduct);
      this.UpdateState(false);
    }

    internal void SetDataContext(object obj)
    {
      this.CurrentDataContext = obj as SpriteListItemData;
      this.UpdateState(false);
    }

    private void UpdateState(bool animate = false)
    {
      if (this.CurrentDataContext == null || this.CurrentDataContext.IsEmoji)
      {
        base.Visibility = Visibility.Collapsed;
      }
      else
      {
        StoreProduct stickerProduct = this.CurrentDataContext.StickerProduct;
        if (stickerProduct == null)
          return;
        if (BatchDownloadManager.IsDownloaded(stickerProduct.id.ToString()))
        {
          base.Visibility = Visibility.Collapsed;
        }
        else
        {
          base.Visibility = Visibility.Visible;
          int id = stickerProduct.id;
          if (BatchDownloadManager.IsDownloading(id.ToString()))
          {
            ((UIElement) this.progressBar).Visibility = Visibility.Visible;
            ((UIElement) this.ButtonDownload).Opacity = 0.0;
            id = stickerProduct.id;
            double to = BatchDownloadManager.DownloadProgress(id.ToString());
            if (!animate)
            {
              ProgressBar progressBar = this.progressBar;
              id = stickerProduct.id;
              double num = BatchDownloadManager.DownloadProgress(id.ToString());
              ((RangeBase) progressBar).Value = num;
            }
            else
              ((DependencyObject) this.progressBar).Animate(((RangeBase) this.progressBar).Value, to, RangeBase.ValueProperty, 100, new int?(),  null,  null);
          }
          else
          {
            ((UIElement) this.progressBar).Visibility = Visibility.Collapsed;
            ((UIElement) this.ButtonDownload).Opacity = 1.0;
            ((RangeBase) this.progressBar).Value = 0.0;
          }
          Uri uriSource = VeryLowProfileImageLoader.GetUriSource(this.previewImage);
          if (uriSource ==  null || uriSource.OriginalString != stickerProduct.photo_140)
            VeryLowProfileImageLoader.SetUriSource(this.previewImage, stickerProduct.photo_140 == null ?  null : new Uri(stickerProduct.photo_140));
          this.textBlockAuthor.Text = (stickerProduct.author ?? "");
          this.textBlockTitle.Text = (stickerProduct.title ?? "");
          this.textBlockDesc.Text = (stickerProduct.description ?? "");
        }
      }
    }

    public void Handle(DownloadSucceededEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        this.UpdateState(false);
      }));
    }

    public void Handle(DownloadFailedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        this.UpdateState(false);
      }));
    }

    public void Handle(DownloadProgressedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.CurrentDataContext == null || !(message.Id == this.CurrentDataContext.StickerProduct.id.ToString()))
          return;
        this.UpdateState(true);
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Emoji/StickerPackPreviewUC.xaml", UriKind.Relative));
      this.previewImage = (Image) base.FindName("previewImage");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockAuthor = (TextBlock) base.FindName("textBlockAuthor");
      this.textBlockDesc = (TextBlock) base.FindName("textBlockDesc");
      this.ButtonDownload = (Button) base.FindName("ButtonDownload");
      this.progressBar = (ProgressBar) base.FindName("progressBar");
    }
  }
}
