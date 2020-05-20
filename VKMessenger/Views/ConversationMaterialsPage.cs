using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.UC;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Photos.Library;
using VKClient.Video.Library;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationMaterialsPage : PageBase
  {
    private bool _isInitialized;
    private bool _photosLoaded;
    private bool _videosLoaded;
    private bool _audiosLoaded;
    private bool _documentsLoaded;
    private bool _linksLoaded;
    internal GenericHeaderUC header;
    internal Pivot pivot;
    internal PivotItem pivotItemPhotos;
    internal ExtendedLongListSelector photosList;
    internal PivotItem pivotItemVideos;
    internal ExtendedLongListSelector videosList;
    internal PivotItem pivotItemAudios;
    internal ExtendedLongListSelector audiosList;
    internal PivotItem pivotItemDocuments;
    internal ExtendedLongListSelector documentsList;
    internal PivotItem pivotItemLinks;
    internal ExtendedLongListSelector linksList;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    private ConversationMaterialsViewModel ViewModel
    {
      get
      {
        return base.DataContext as ConversationMaterialsViewModel;
      }
    }

    public ConversationMaterialsPage()
    {
      this.InitializeComponent();
      this.header.OnHeaderTap = new Action(this.HandleHeaderTap);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.photosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.videosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.audiosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.documentsList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.linksList);
      this.photosList.OnRefresh = (Action) (() => this.ViewModel.PhotosVM.LoadData(true, false,  null, false));
      this.videosList.OnRefresh = (Action) (() => this.ViewModel.VideosVM.LoadData(true, false,  null, false));
      this.audiosList.OnRefresh = (Action) (() => this.ViewModel.AudiosVM.LoadData(true, false,  null, false));
      this.documentsList.OnRefresh = (Action) (() => this.ViewModel.DocumentsVM.LoadData(true, false,  null, false));
      this.linksList.OnRefresh = (Action) (() => this.ViewModel.LinksVM.LoadData(true, false,  null, false));
    }

    private void HandleHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemPhotos)
        this.photosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemVideos)
        this.videosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemAudios)
        this.audiosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemDocuments)
        this.documentsList.ScrollToTop();
      if (this.pivot.SelectedItem != this.pivotItemLinks)
        return;
      this.linksList.ScrollToTop();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      base.DataContext = (new ConversationMaterialsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["PeerId"])));
      this._isInitialized = true;
    }

    private void pivot_OnItemLoaded(object sender, PivotItemEventArgs e)
    {
      if (e.Item == this.pivotItemPhotos && !this._photosLoaded)
      {
        this.ViewModel.PhotosVM.LoadData(false, false,  null, false);
        this._photosLoaded = true;
      }
      if (e.Item == this.pivotItemVideos && !this._videosLoaded)
      {
        this.ViewModel.VideosVM.LoadData(false, false,  null, false);
        this._videosLoaded = true;
      }
      if (e.Item == this.pivotItemAudios && !this._audiosLoaded)
      {
        this.ViewModel.AudiosVM.LoadData(false, false,  null, false);
        this._audiosLoaded = true;
      }
      if (e.Item == this.pivotItemDocuments && !this._documentsLoaded)
      {
        this.ViewModel.DocumentsVM.LoadData(false, false,  null, false);
        this._documentsLoaded = true;
      }
      if (e.Item != this.pivotItemLinks || this._linksLoaded)
        return;
      this.ViewModel.LinksVM.LoadData(false, false,  null, false);
      this._linksLoaded = true;
    }

    private void photosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.PhotosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void videosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.VideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void audiosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.AudiosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void documentsList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.DocumentsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void linksList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.LinksVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void photo_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      Photo photo =  null;
      AlbumPhotoHeaderFourInARow dataContext = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
      if (dataContext != null)
      {
        string str = frameworkElement.Tag.ToString();
        if (!(str == "1"))
        {
          if (!(str == "2"))
          {
            if (!(str == "3"))
            {
              if (str == "4")
                photo = dataContext.Photo4.Photo;
            }
            else
              photo = dataContext.Photo3.Photo;
          }
          else
            photo = dataContext.Photo2.Photo;
        }
        else
          photo = dataContext.Photo1.Photo;
      }
      if (photo == null)
        return;
      List<Photo> photoList1 = new List<Photo>();
      foreach (AlbumPhotoHeaderFourInARow headerFourInArow in (Collection<AlbumPhotoHeaderFourInARow>) this.ViewModel.PhotosVM.Collection)
        photoList1.AddRange(headerFourInArow.GetAsPhotos());
      int num1 = photoList1.IndexOf(photo);
      int num2 = Math.Max(0, num1 - 20);
      List<Photo> photoList2 = new List<Photo>();
      for (int index = num2; index < Math.Min(photoList1.Count, num1 + 30); ++index)
        photoList2.Add(photoList1[index]);
      Navigator.Current.NavigateToImageViewer(this.ViewModel.PhotosVM.TotalCount, num2, photoList2.IndexOf(photo), (List<long>)Enumerable.ToList<long>(Enumerable.Select<Photo, long>(photoList2, (Func<Photo, long>)(p => p.pid))), (List<long>)Enumerable.ToList<long>(Enumerable.Select<Photo, long>(photoList2, (Func<Photo, long>)(p => p.owner_id))), (List<string>)Enumerable.ToList<string>(Enumerable.Select<Photo, string>(photoList2, (Func<Photo, string>)(p => p.access_key))), photoList2, "PhotosByIds", true, false, (Func<int, Image>)(i => null), null, false);
    }

    private void videosList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      VideoHeader selectedItem = this.videosList.SelectedItem as VideoHeader;
      this.videosList.SelectedItem = null;
      if (selectedItem == null)
        return;
      Navigator.Current.NavigateToVideoWithComments(selectedItem.VKVideo, selectedItem.VKVideo.owner_id, selectedItem.VKVideo.vid, selectedItem.VKVideo.access_key ?? "");
    }

    private void audiosList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      AudioHeader selectedItem = this.audiosList.SelectedItem as AudioHeader;
      this.audiosList.SelectedItem = null;
      if (selectedItem == null)
        return;
      if (selectedItem.IsContentRestricted)
      {
        selectedItem.ShowContentRestrictedMessage();
      }
      else
      {
          PlaylistManager.SetAudioAgentPlaylist((List<AudioObj>)Enumerable.ToList<AudioObj>(Enumerable.Select<AudioHeader, AudioObj>(Enumerable.OfType<AudioHeader>((IEnumerable)this.audiosList.ItemsSource), (Func<AudioHeader, AudioObj>)(item => item.Track))), CurrentMediaSource.AudioSource);
        if (!selectedItem.TryAssignTrack())
          return;
        Navigator.Current.NavigateToAudioPlayer(false);
      }
    }

    private void documentsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      DocumentHeader selectedItem = this.documentsList.SelectedItem as DocumentHeader;
      this.documentsList.SelectedItem = null;
      if (selectedItem == null)
        return;
      if (!selectedItem.IsGif)
      {
        Navigator.Current.NavigateToWebUri(selectedItem.Document.url, true, false);
      }
      else
      {
          List<DocumentHeader> list1 = (List<DocumentHeader>)Enumerable.ToList<DocumentHeader>(Enumerable.Where<DocumentHeader>(this.ViewModel.DocumentsVM.Collection, (Func<DocumentHeader, bool>)(doc => doc.IsGif)));
        int num = -1;
        List<PhotoOrDocument> list = new List<PhotoOrDocument>();
        for (int index = 0; index < list1.Count; ++index)
        {
          DocumentHeader documentHeader = list1[index];
          if (documentHeader == selectedItem)
            num = index;
          list.Add(new PhotoOrDocument()
          {
            document = documentHeader.Document
          });
        }
        if (num < 0)
          return;
        InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
        Navigator.Current.NavigateToImageViewerPhotosOrGifs(num, list, false, false,  null,  null, false, (FrameworkElement) gifViewer, (Action<int>) (ind =>
        {
          Doc document = list[ind].document;
          if (document != null)
          {
            InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(document, true, false, false);
            gifViewerViewModel.Play(GifPlayStartType.manual);
            gifViewer.VM = gifViewerViewModel;
            ((UIElement) gifViewer).Visibility = Visibility.Visible;
          }
          else
          {
            InplaceGifViewerViewModel vm = gifViewer.VM;
            if (vm != null)
              vm.Stop();
            ((UIElement) gifViewer).Visibility = Visibility.Collapsed;
          }
        }), (Action<int, bool>) ((i, b) => {}), false);
      }
    }

    private void linksList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      LinkHeader selectedItem = this.linksList.SelectedItem as LinkHeader;
      this.linksList.SelectedItem = null;
      if (selectedItem == null)
        return;
      Navigator.Current.NavigateToWebUri(selectedItem.Url, false, false);
    }

    private void GoToMessage_OnClicked(object sender, RoutedEventArgs e)
    {
      long message_id = 0;
      object dataContext = ((FrameworkElement) sender).DataContext;
      switch (this.pivot.SelectedIndex)
      {
        case 0:
          AlbumPhoto albumPhoto = dataContext as AlbumPhoto;
          message_id = albumPhoto != null ? albumPhoto.MessageId : 0L;
          break;
        case 1:
          VideoHeader videoHeader = dataContext as VideoHeader;
          message_id = videoHeader != null ? videoHeader.MessageId : 0L;
          break;
        case 2:
          AudioHeader audioHeader = dataContext as AudioHeader;
          message_id = audioHeader != null ? audioHeader.MessageId : 0L;
          break;
        case 3:
          DocumentHeader documentHeader = dataContext as DocumentHeader;
          message_id = documentHeader != null ? documentHeader.MessageId : 0L;
          break;
        case 4:
          LinkHeader linkHeader = dataContext as LinkHeader;
          message_id = linkHeader != null ? linkHeader.MessageId : 0L;
          break;
      }
      if (message_id == 0L)
        return;
      long peerId = this.ViewModel.PeerId;
      if (this.ViewModel.IsChat)
        peerId -= 2000000000L;
      Navigator.Current.NavigateToConversation(peerId, this.ViewModel.IsChat, false, "", message_id, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationMaterialsPage.xaml", UriKind.Relative));
      this.header = (GenericHeaderUC) base.FindName("header");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemPhotos = (PivotItem) base.FindName("pivotItemPhotos");
      this.photosList = (ExtendedLongListSelector) base.FindName("photosList");
      this.pivotItemVideos = (PivotItem) base.FindName("pivotItemVideos");
      this.videosList = (ExtendedLongListSelector) base.FindName("videosList");
      this.pivotItemAudios = (PivotItem) base.FindName("pivotItemAudios");
      this.audiosList = (ExtendedLongListSelector) base.FindName("audiosList");
      this.pivotItemDocuments = (PivotItem) base.FindName("pivotItemDocuments");
      this.documentsList = (ExtendedLongListSelector) base.FindName("documentsList");
      this.pivotItemLinks = (PivotItem) base.FindName("pivotItemLinks");
      this.linksList = (ExtendedLongListSelector) base.FindName("linksList");
      this.pullToRefresh = (PullToRefreshUC) base.FindName("pullToRefresh");
    }
  }
}
