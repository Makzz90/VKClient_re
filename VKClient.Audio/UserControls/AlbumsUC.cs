using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Localization;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Localization;

namespace VKClient.Audio
{
  public class AlbumsUC : UserControl
  {
    internal ExtendedLongListSelector AllAlbums;
    private bool _contentLoaded;

    public AllAudioViewModel VM
    {
      get
      {
        return base.DataContext as AllAudioViewModel;
      }
    }

    public ExtendedLongListSelector ListAllAlbums
    {
      get
      {
        return this.AllAlbums;
      }
    }

    public AlbumsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void EditAlbumItem_Tap(object sender, RoutedEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null || !(frameworkElement.DataContext is AudioAlbumHeader))
        return;
      this.ShowEditAlbum((frameworkElement.DataContext as AudioAlbumHeader).Album);
    }

    private void DeleteAlbumItem_Tap(object sender, RoutedEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null || !(frameworkElement.DataContext is AudioAlbumHeader))
        return;
      AudioAlbumHeader dataContext = frameworkElement.DataContext as AudioAlbumHeader;
      if (MessageBox.Show(CommonResources.GenericConfirmation, AudioResources.DeleteAlbum, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.VM.AllAlbumsVM.DeleteAlbum(dataContext);
    }

    private void AllAlbums_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      (base.DataContext as AllAudioViewModel).AllAlbumsVM.LoadMore(e.ContentPresenter.Content);
    }

    public void ShowEditAlbum(AudioAlbum album)
    {
      DialogService dc = new DialogService();
      dc.SetStatusBarBackground = true;
      dc.HideOnNavigation = false;
      EditAlbumUC editAlbum = new EditAlbumUC();
      editAlbum.textBlockCaption.Text = (album.album_id == 0L ? AudioResources.CreateAlbum : AudioResources.EditAlbum);
      editAlbum.textBoxText.Text = (album.title ?? "");
      dc.Child = (FrameworkElement) editAlbum;
      ((UIElement) editAlbum.buttonSave).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>) ((s, e) =>
      {
        album.title = editAlbum.textBoxText.Text;
        this.VM.AllAlbumsVM.CreateEditAlbum(album);
        dc.Hide();
      }));
      dc.Show((UIElement) this);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/AlbumsUC.xaml", UriKind.Relative));
      this.AllAlbums = (ExtendedLongListSelector) base.FindName("AllAlbums");
    }
  }
}
