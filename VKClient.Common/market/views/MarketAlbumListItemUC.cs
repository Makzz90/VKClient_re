using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.Views
{
  public class MarketAlbumListItemUC : UserControl
  {
      public static readonly DependencyProperty AlbumProperty = DependencyProperty.Register("Album", typeof(MarketAlbum), typeof(MarketAlbumListItemUC), new PropertyMetadata(new PropertyChangedCallback(MarketAlbumListItemUC.Album_OnChanged)));
    private long _ownerId;
    private long _albumId;
    private string _albumName;
    internal Border borderPlaceholder;
    internal Image image;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    private bool _contentLoaded;

    public MarketAlbum Album
    {
      get
      {
        return (MarketAlbum) base.GetValue(MarketAlbumListItemUC.AlbumProperty);
      }
      set
      {
        base.SetValue(MarketAlbumListItemUC.AlbumProperty, value);
      }
    }

    public MarketAlbumListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      this.textBlockSubtitle.Text = ("");
      ((UIElement) this.borderPlaceholder).Visibility = Visibility.Collapsed;
    }

    private static void Album_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((MarketAlbumListItemUC) d).UpdateData(e.NewValue as MarketAlbum);
    }

    private void UpdateData(MarketAlbum album)
    {
      if (album == null)
        return;
      this._ownerId = album.owner_id;
      this._albumId = album.id;
      this._albumName = album.title;
      this.UpdateTitle(album.title);
      this.UpdateSubtitle(album.count);
      this.UpdateImage(album.photo);
    }

    private void UpdateTitle(string newTitle)
    {
      this.textBlockTitle.Text = newTitle;
      if (((FrameworkElement) this.textBlockTitle).ActualWidth <= base.Width)
        return;
      this.textBlockTitle.CorrectText(base.Width);
    }

    private void UpdateSubtitle(int count)
    {
      this.textBlockSubtitle.Text = (count != 0 ? UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneProductItemFrm, CommonResources.TwoFourProductItemsFrm, CommonResources.FiveProductItemsFrm, true,  null, false) : CommonResources.NoProducts);
    }

    private void UpdateImage(Photo photo)
    {
      if (photo != null)
      {
        ((UIElement) this.borderPlaceholder).Visibility = Visibility.Collapsed;
        ImageLoader.SetUriSource(this.image, photo.GetAppropriateForScaleFactor(((FrameworkElement) this.image).Height, 1));
      }
      else
      {
        ((UIElement) this.borderPlaceholder).Visibility = Visibility.Visible;
        ImageLoader.SetUriSource(this.image, "");
      }
    }

    private void Album_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._ownerId == 0L || this._albumId == 0L)
        return;
      Navigator.Current.NavigateToMarketAlbumProducts(this._ownerId, this._albumId, this._albumName);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/MarketAlbumListItemUC.xaml", UriKind.Relative));
      this.borderPlaceholder = (Border) base.FindName("borderPlaceholder");
      this.image = (Image) base.FindName("image");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) base.FindName("textBlockSubtitle");
    }
  }
}
