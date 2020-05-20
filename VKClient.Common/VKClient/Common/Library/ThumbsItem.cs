using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;

namespace VKClient.Common.Library
{
  public class ThumbsItem : VirtualizableItemBase, IHandle<VideoUploaded>, IHandle
  {
    private readonly List<Attachment> _attachments;
    private double _height;
    private readonly NewsPhotosInfo _newsPhotosInfo;
    private List<ThumbsItem.Thumb> _thumbs;
    private const int MARGIN_BETWEEN_IMAGES = 4;
    public const double DESIRED_HEIGHT_DEFAULT = 320.0;
    private readonly double DesiredHeight;
    private readonly bool _friendsOnly;
    private readonly bool _isCommentThumbItem;
    private readonly bool _isMessage;
    private readonly string _itemId;
    private readonly double _verticalWidth;
    private readonly double _horizontalWidth;
    private bool _isHorizontal;

    private ThumbsItem.ItemDataType ItemType
    {
      get
      {
        return this._attachments == null ? ThumbsItem.ItemDataType.NewsPhotosInfo : ThumbsItem.ItemDataType.Attachment;
      }
    }

    private int PhotosCount
    {
      get
      {
        switch (this.ItemType)
        {
          case ThumbsItem.ItemDataType.Attachment:
            return this._attachments.Count<Attachment>((Func<Attachment, bool>) (a => a.type == "photo"));
          case ThumbsItem.ItemDataType.NewsPhotosInfo:
            return this._newsPhotosInfo.Photos.Count;
          default:
            return 0;
        }
      }
    }

    private List<Photo> PhotosListFromAttachments
    {
      get
      {
        return this._attachments.Where<Attachment>((Func<Attachment, bool>) (a => a.type == "photo")).Select<Attachment, Photo>((Func<Attachment, Photo>) (a => a.photo)).ToList<Photo>();
      }
    }

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        this.CreateOrUpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public ThumbsItem(double width, Thickness margin, List<Attachment> attachments, bool friendsOnly, string itemId, bool isCommentThumbItem = false, bool isMessage = false, bool isHorizontal = false, double horizontalWidth = 0.0, double desiredHeight = 0.0)
      : base(width, margin, new Thickness())
    {
      EventAggregator.Current.Subscribe((object) this);
      this._attachments = (attachments != null ? attachments.Where<Attachment>((Func<Attachment, bool>) (a =>
      {
        if (a.type == "photo" || a.type == "video" || (a.type == "album" || a.type == "market_album"))
          return true;
        if (a.type == "doc" && a.doc != null)
          return a.doc.IsGif;
        return false;
      })).ToList<Attachment>() : (List<Attachment>) null) ?? new List<Attachment>();
      this._friendsOnly = friendsOnly;
      this._itemId = itemId;
      this._isCommentThumbItem = isCommentThumbItem;
      this._isMessage = isMessage;
      this._verticalWidth = width;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontal = isHorizontal;
      if (this._isHorizontal)
        this.Width = horizontalWidth;
      this.DesiredHeight = desiredHeight > 0.0 ? desiredHeight : 320.0;
      this.PrepareThumbsList();
    }

    public ThumbsItem(double width, Thickness margin, NewsPhotosInfo newsPhotosInfo)
      : base(width, margin, new Thickness())
    {
      this._newsPhotosInfo = newsPhotosInfo;
      this.DesiredHeight = 320.0;
      this.PrepareThumbsList();
    }

    private void CreateOrUpdateLayout()
    {
      foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
        virtualizableChild.ChangeState(VirtualizableState.Unloaded);
      this.VirtualizableChildren.Clear();
      if (this._thumbs.Count == 0)
        return;
      List<Rect> layout = RectangleLayoutHelper.CreateLayout(new Size(this.Width, this.DesiredHeight), this._thumbs.Select<ThumbsItem.Thumb, Size>((Func<ThumbsItem.Thumb, Size>) (t => new Size(t.Width, t.Height))).ToList<Size>(), 4.0);
      layout.Max<Rect>((Func<Rect, double>) (r => r.Left + r.Width));
      int index = 0;
      foreach (Rect rect1 in layout)
      {
        Rect rect = rect1;
        Thickness margin = new Thickness(rect.Left, rect.Top, 0.0, 0.0);
        ThumbsItem.Thumb currentThumb = this._thumbs[index];
        bool blackRectOnTop = !string.IsNullOrEmpty(currentThumb.Label);
        bool flag = !string.IsNullOrEmpty(currentThumb.LabelLarge);
        VirtualizableImage virtImage = new VirtualizableImage(rect.Width, rect.Height, margin, currentThumb.GetProperSrc(rect.Width, rect.Height), new Action<VirtualizableImage>(this.Image_Tap), index.ToString(), false, !this._isMessage, Stretch.UniformToFill, (Brush) null, -1.0, blackRectOnTop, false);
        this.VirtualizableChildren.Add((IVirtualizable) virtImage);
        if (currentThumb.Video != null)
        {
          this.VirtualizableChildren.Add((IVirtualizable) new UCItem(rect.Width, margin, (Func<UserControlVirtualizable>) (() =>
          {
            return (UserControlVirtualizable) new VideoNewsItemDescUC()
            {
              Width = rect.Width,
              Height = rect.Height,
              DataContext = (object) new VideoNewsItemDescViewModel(currentThumb.Video, rect.Width > 460.0)
            };
          }), (Func<double>) (() => rect.Height), (Action<UserControlVirtualizable>) null, 0.0, false));
          virtImage.Tag2 = "NotAPreview";
        }
        else if (currentThumb.GifDoc != null)
        {
          UCItem ucItem = new UCItem(rect.Width, margin, (Func<UserControlVirtualizable>) (() =>
          {
            bool isOneItem = layout.Count == 1;
            InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(currentThumb.GifDoc, false, this._isCommentThumbItem || this._isMessage, isOneItem);
            if (isOneItem)
              return (UserControlVirtualizable) new InplaceGifViewerUC()
              {
                Width = rect.Width,
                Height = rect.Height,
                VM = gifViewerViewModel,
                gifOverlayUC = {
                  OnTap = (Action) (() => this.Image_Tap(virtImage))
                }
              };
            return (UserControlVirtualizable) new GifOverlayUC()
            {
              Width = rect.Width,
              Height = rect.Height,
              DataContext = (object) gifViewerViewModel,
              OnTap = (Action) (() => this.Image_Tap(virtImage))
            };
          }), (Func<double>) (() => rect.Height), (Action<UserControlVirtualizable>) null, 0.0, false);
          virtImage.OverlayControl = (object) ucItem;
          this.VirtualizableChildren.Add((IVirtualizable) ucItem);
        }
        else if (currentThumb.Album != null || currentThumb.MarketAlbum != null)
        {
          this.VirtualizableChildren.Add((IVirtualizable) new UCItem(rect.Width, margin, (Func<UserControlVirtualizable>) (() =>
          {
            return (UserControlVirtualizable) new AlbumNewsItemDescUC()
            {
              Width = rect.Width,
              Height = rect.Height,
              DataContext = (object) (currentThumb.Album != null ? new AlbumNewsItemDescViewModel(currentThumb.Album, rect.Width > 460.0) : new AlbumNewsItemDescViewModel(currentThumb.MarketAlbum, rect.Width > 460.0))
            };
          }), (Func<double>) (() => rect.Height), (Action<UserControlVirtualizable>) null, 0.0, false));
          virtImage.Tag2 = "NotAPreview";
        }
        else if (blackRectOnTop)
        {
          if (flag)
          {
            this.VirtualizableChildren.Add((IVirtualizable) new TextItem(rect.Width - 12.0, new Thickness(rect.Left + 12.0, rect.Top + rect.Height - 69.0, 0.0, 0.0), this._thumbs[index].LabelLarge, false, 25.33, "Segoe WP", 27.0, new SolidColorBrush(Colors.White), false, null));
            double width = rect.Width - 12.0;
            Thickness textMargin = new Thickness(rect.Left + 12.0, rect.Top + rect.Height - 37.0, 0.0, 0.0);
            string label = this._thumbs[index].Label;
            int num1 = 0;
            double fontSize = 20.0;
            string fontFamily = "Segoe WP";
            double lineHeight = 22.0;
            SolidColorBrush foreground = new SolidColorBrush(Colors.White);
            double num2 = 0.6;
            foreground.Opacity = num2;
            int num3 = 0;
            object local = null;
            this.VirtualizableChildren.Add((IVirtualizable) new TextItem(width, textMargin, label, num1 != 0, fontSize, fontFamily, lineHeight, foreground, num3 != 0, (Action) local));
          }
          else
            this.VirtualizableChildren.Add((IVirtualizable) new TextItem(rect.Width - 12.0, new Thickness(rect.Left + 12.0, rect.Top + rect.Height - 37.0, 0.0, 0.0), this._thumbs[index].Label, false, 20.0, "Segoe WP", 22.0, new SolidColorBrush(Colors.White), false, null));
          virtImage.Tag2 = "NotAPreview";
        }
        ++index;
      }
      if (this.CurrentState != VirtualizableState.Unloaded)
      {
        foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
          virtualizableChild.ChangeState(this.CurrentState);
      }
      double num;
      if (layout.Count <= 0)
      {
        num = 0.0;
      }
      else
      {
        Rect rect = layout.Last<Rect>();
        double top = rect.Top;
        rect = layout.Last<Rect>();
        double height = rect.Height;
        num = top + height;
      }
      this._height = num;
    }

    public void Image_Tap(VirtualizableImage source = null)
    {
      if (source == null)
      {
        source = this.VirtualizableChildren.FirstOrDefault<IVirtualizable>() as VirtualizableImage;
        if (source == null || source.Tag == null)
          return;
      }
      int selectedPhotoIndex = int.Parse(source.Tag);
      if (this.ItemType == ThumbsItem.ItemDataType.Attachment)
      {
        List<Attachment> list1 = this._attachments.OrderBy<Attachment, bool>((Func<Attachment, bool>) (at =>
        {
          if (at.doc == null)
            return false;
          DocPreview preview = at.doc.preview;
          return (preview != null ? preview.video : (DocPreviewVideo) null) == null;
        })).ToList<Attachment>();
        Attachment attachment1 = list1[selectedPhotoIndex];
        if (attachment1.type == "photo" || attachment1.type == "doc")
        {
          List<PhotoOrDocument> list = new List<PhotoOrDocument>();
          int selectedIndex = 0;
          bool flag = false;
          foreach (Attachment attachment2 in list1.Where<Attachment>((Func<Attachment, bool>) (at =>
          {
            if (at.photo == null)
              return at.doc != null;
            return true;
          })))
          {
            if (attachment2 == attachment1)
              selectedIndex = list.Count;
            if (attachment2.doc != null)
              flag = true;
            list.Add(new PhotoOrDocument()
            {
              document = attachment2.doc,
              photo = attachment2.photo
            });
          }
          if (flag)
          {
            InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
            if (this._isCommentThumbItem)
              CurrentMediaSource.GifPlaySource = StatisticsActionSource.comments;
            else if (this._isMessage)
              CurrentMediaSource.GifPlaySource = StatisticsActionSource.messages;
            Navigator.Current.NavigateToImageViewerPhotosOrGifs(selectedIndex, list, false, this._friendsOnly, this.GetImageFunc(), (PageBase) null, false, (FrameworkElement) gifViewer, (Action<int>) (ind =>
            {
              Doc document = list[ind].document;
              if (document != null)
              {
                InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(document, true, true, false);
                gifViewerViewModel.Play(GifPlayStartType.manual);
                gifViewer.VM = gifViewerViewModel;
                gifViewer.Visibility = Visibility.Visible;
              }
              else
              {
                InplaceGifViewerViewModel vm = gifViewer.VM;
                if (vm != null)
                  vm.Stop();
                gifViewer.Visibility = Visibility.Collapsed;
              }
            }), new Action<int, bool>(this.ShowHideOverlay), false);
          }
          else
            Navigator.Current.NavigateToImageViewer(this.PhotosCount, 0, this.PhotosListFromAttachments.IndexOf(attachment1.photo), this.PhotosListFromAttachments.Select<Photo, long>((Func<Photo, long>) (p => p.pid)).ToList<long>(), this.PhotosListFromAttachments.Select<Photo, long>((Func<Photo, long>) (p => p.owner_id)).ToList<long>(), this.PhotosListFromAttachments.Select<Photo, string>((Func<Photo, string>) (p => p.access_key ?? "")).ToList<string>(), this.PhotosListFromAttachments, "PhotosByIds", false, this._friendsOnly, this.GetImageFunc(), (PageBase) null, false);
        }
        else if (attachment1.type == "video")
        {
          if (attachment1.video.owner_id == 0L || attachment1.video.vid == 0L)
            return;
          if (this._isCommentThumbItem)
          {
            CurrentMediaSource.VideoSource = StatisticsActionSource.comments;
            CurrentMediaSource.VideoContext = this._itemId;
          }
          else if (this._isMessage)
          {
            CurrentMediaSource.VideoSource = StatisticsActionSource.messages;
            CurrentMediaSource.VideoContext = this._itemId;
          }
          else if (CurrentMediaSource.VideoSource == StatisticsActionSource.news)
            CurrentMediaSource.VideoContext = this._itemId;
          else if (CurrentMediaSource.VideoSource == StatisticsActionSource.wall_user || CurrentMediaSource.VideoSource == StatisticsActionSource.wall_group)
            CurrentMediaSource.VideoContext = CurrentMediaSource.VideoContext + "|" + this._itemId;
          Navigator.Current.NavigateToVideoWithComments(attachment1.video.guid == Guid.Empty ? attachment1.video : null, attachment1.video.owner_id, attachment1.video.vid, attachment1.video.access_key);
        }
        else if (attachment1.type == "album")
        {
          Album album = attachment1.album;
          if (album == null)
            return;
          AlbumType albumType = AlbumTypeHelper.GetAlbumType(album.aid);
          Navigator.Current.NavigateToPhotoAlbum(Math.Abs(long.Parse(album.owner_id)), long.Parse(album.owner_id) < 0L, albumType.ToString(), album.aid, "", 0, "", "", false, 0);
        }
        else
        {
          if (!(attachment1.type == "market_album"))
            return;
          MarketAlbum marketAlbum = attachment1.market_album;
          if (marketAlbum == null)
            return;
          Navigator.Current.NavigateToMarketAlbumProducts(marketAlbum.owner_id, marketAlbum.id, marketAlbum.title);
        }
      }
      else
      {
        if (this.ItemType != ThumbsItem.ItemDataType.NewsPhotosInfo || this._newsPhotosInfo.Photos.Count <= 0)
          return;
        Navigator.Current.NaviateToImageViewerPhotoFeed(Math.Abs(this._newsPhotosInfo.SourceId), this._newsPhotosInfo.SourceId < 0L, this._newsPhotosInfo.Photos.First<Photo>().aid.ToString(), this._newsPhotosInfo.Count, selectedPhotoIndex, this._newsPhotosInfo.Date, this._newsPhotosInfo.Photos, this._newsPhotosInfo.NewsType == NewsPhotosInfo.NewsPhotoType.Photo ? "Photos" : "PhotoTags", this.GetImageFunc());
      }
    }

    private void ShowHideOverlay(int ind, bool show)
    {
      if (this._isMessage)
        return;
      VirtualizableImage virtImageByInd = this.FindVirtImageByInd(ind);
      UCItem ucItem = (virtImageByInd != null ? virtImageByInd.OverlayControl : null) as UCItem;
      UserControlVirtualizable controlVirtualizable = ucItem != null ? ucItem.UC : (UserControlVirtualizable) null;
      if (controlVirtualizable == null)
        return;
      controlVirtualizable.Opacity = show ? 1.0 : 0.0;
    }

    private Func<int, Image> GetImageFunc()
    {
      return (Func<int, Image>) (index =>
      {
        if (!this._isMessage)
        {
          VirtualizableImage virtImageByInd = this.FindVirtImageByInd(index);
          if (virtImageByInd != null)
            return virtImageByInd.ImageControl;
        }
        return (Image) null;
      });
    }

    private VirtualizableImage FindVirtImageByInd(int i)
    {
      List<IVirtualizable> list = this.VirtualizableChildren.Where<IVirtualizable>((Func<IVirtualizable, bool>) (c =>
      {
        if (c is VirtualizableImage)
          return (c as VirtualizableImage).Tag2 != "NotAPreview";
        return false;
      })).ToList<IVirtualizable>();
      if (i >= 0 && i < list.Count)
        return list[i] as VirtualizableImage;
      return (VirtualizableImage) null;
    }

    public void PrepareThumbsList()
    {
      this._thumbs = new List<ThumbsItem.Thumb>();
      if (this.ItemType == ThumbsItem.ItemDataType.Attachment)
        this.ReadThumbsListFromAttachments();
      else
        this.ReadThumbsListFromNewsPhotosInfo();
      this.CreateOrUpdateLayout();
    }

    private void ReadThumbsListFromNewsPhotosInfo()
    {
      NewsPhotosInfo newsPhotosInfo = this._newsPhotosInfo;
      if ((newsPhotosInfo != null ? newsPhotosInfo.Photos : (List<Photo>) null) == null)
        return;
      this._thumbs.Clear();
      foreach (Photo photo in this._newsPhotosInfo.Photos)
        this._thumbs.Add(ThumbsItem.ConvertPhotoToThumb(photo, (Attachment) null));
    }

    private void ReadThumbsListFromAttachments()
    {
      if (this._attachments == null)
        return;
      this._thumbs.Clear();
      foreach (Attachment attachment in this._attachments)
      {
        ThumbsItem.Thumb thumb1 = (ThumbsItem.Thumb) null;
        if (attachment.type == "photo")
        {
          Photo photo = attachment.photo;
          if (photo != null)
            thumb1 = ThumbsItem.ConvertPhotoToThumb(photo, attachment);
        }
        else if (attachment.type == "album")
        {
          Photo thumb2 = attachment.album.thumb;
          if (thumb2 != null)
            thumb1 = ThumbsItem.ConvertPhotoToAlbumThumb(thumb2, attachment);
          else
            thumb1 = new ThumbsItem.Thumb(attachment)
            {
              Width = this.Width,
              Height = (double) (int) (this.Width * 2.0 / 3.0)
            };
        }
        else if (attachment.type == "market_album")
        {
          Photo photo = attachment.market_album.photo;
          if (photo != null)
            thumb1 = ThumbsItem.ConvertPhotoToAlbumThumb(photo, attachment);
          else
            thumb1 = new ThumbsItem.Thumb(attachment)
            {
              Width = this.Width,
              Height = (double) (int) (this.Width * 2.0 / 3.0)
            };
        }
        else if (attachment.type == "video")
        {
            VKClient.Common.Backend.DataObjects.Video video = attachment.video;
          if (video != null)
            thumb1 = new ThumbsItem.Thumb(attachment)
            {
              Width = this.Width,
              Height = (double) (int) (this.Width * 9.0 / 16.0),
              BigSrc = string.IsNullOrWhiteSpace(video.image_big) ? video.image_medium : video.image_big
            };
        }
        else if (attachment.type == "doc")
        {
          Doc doc = attachment.doc;
          if (doc != null && doc.IsVideoGif)
          {
            Photo photoPreview = doc.ConvertToPhotoPreview();
            thumb1 = new ThumbsItem.Thumb(attachment)
            {
              Width = (double) photoPreview.width,
              Height = (double) photoPreview.height,
              BigSrc = photoPreview.src_xbig
            };
          }
        }
        if (thumb1 != null)
          this._thumbs.Add(thumb1);
      }
    }

    protected override void ReleaseResourcesOnUnload()
    {
      foreach (FrameworkElement child in this.Children)
      {
        if (child is Image)
        {
          Image image = child as Image;
          BitmapImage bitmapImage = image.Source as BitmapImage;
          if (bitmapImage != null)
            bitmapImage.UriSource = null;
          object local = null;
          image.Source = (ImageSource) local;
        }
      }
    }

    private static ThumbsItem.Thumb ConvertPhotoToThumb(Photo photo, Attachment att)
    {
      return new ThumbsItem.Thumb(att)
      {
        Width = (double) photo.width,
        Height = (double) photo.height,
        BigSrc = photo.src_big,
        MediumSrc = photo.src,
        SmallSrc = photo.src_small
      };
    }

    private static ThumbsItem.Thumb ConvertPhotoToAlbumThumb(Photo photo, Attachment att)
    {
      return new ThumbsItem.Thumb(att)
      {
        Width = (double) photo.width,
        Height = (double) (int) ((double) photo.width * 2.0 / 3.0),
        BigSrc = photo.src_big,
        MediumSrc = photo.src,
        SmallSrc = photo.src_small
      };
    }

    public void Handle(VideoUploaded message)
    {
      List<Attachment> source = this._attachments;
      Attachment attachment1;
      if (source == null)
      {
        attachment1 = (Attachment) null;
      }
      else
      {
        Func<Attachment, bool> predicate = (Func<Attachment, bool>) (a =>
        {
          if (a.video != null)
            return a.video.guid == message.guid;
          return false;
        });
        attachment1 = source.FirstOrDefault<Attachment>(predicate);
      }
      Attachment attachment2 = attachment1;
      if (attachment2 == null)
        return;
      VKClient.Common.Backend.DataObjects.Video video = attachment2.video;
      long videoId = message.SaveVidResp.video_id;
      video.id = videoId;
      string title = message.SaveVidResp.title;
      video.title = title;
      string description = message.SaveVidResp.description;
      video.description = description;
      long ownerId = message.SaveVidResp.owner_id;
      video.owner_id = ownerId;
      string accessKey = message.SaveVidResp.access_key;
      video.access_key = accessKey;
    }

    private enum ItemDataType
    {
      Attachment,
      NewsPhotosInfo,
    }

    private class Thumb
    {
      public double Width { get; set; }

      public double Height { get; set; }

      public string Label { get; set; }

      public string LabelLarge { get; set; }

      public string SmallSrc { get; set; }

      public string MediumSrc { get; set; }

      public string BigSrc { get; set; }

      public VKClient.Common.Backend.DataObjects.Video Video
      {
        get
        {
          Attachment att = this.Att;
          if (att == null)
            return null;
          return att.video;
        }
      }

      public Album Album
      {
        get
        {
          Attachment att = this.Att;
          if (att == null)
            return (Album) null;
          return att.album;
        }
      }

      public MarketAlbum MarketAlbum
      {
        get
        {
          Attachment att = this.Att;
          if (att == null)
            return (MarketAlbum) null;
          return att.market_album;
        }
      }

      public Doc GifDoc
      {
        get
        {
          Attachment att = this.Att;
          if (att == null)
            return (Doc) null;
          return att.doc;
        }
      }

      public Attachment Att { get; private set; }

      public Thumb(Attachment att)
      {
        this.Att = att;
      }

      internal string GetProperSrc(double width, double height)
      {
        double num = Math.Max(width, height);
        if (num <= 75.0 && !string.IsNullOrEmpty(this.SmallSrc))
          return this.SmallSrc;
        if (num <= 130.0 && !string.IsNullOrEmpty(this.MediumSrc) || !AppGlobalStateManager.Current.GlobalState.LoadBigPhotosOverMobile && NetworkStatusInfo.Instance.NetworkStatus != NetworkStatus.WiFi)
          return this.MediumSrc;
        return this.BigSrc;
      }
    }
  }
}
