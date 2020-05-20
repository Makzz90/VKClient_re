using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPlaceSmallAttachmentUC : MapAttachmentUCBase
  {
    private const int TEXTBLOCK_MARGIN_LEFT = 84;
    private const int MARGIN_LEFT_RIGHT = 16;
    public const int FIXED_HEIGHT = 72;
    private Uri _groupPhotoUri;
    internal Canvas canvas;
    internal Image imageGroupPhoto;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    private bool _contentLoaded;

    public MapPlaceSmallAttachmentUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      this.textBlockSubtitle.Text = ("");
    }

    public override void OnReady()
    {
      ((FrameworkElement) this.canvas).Width=(base.Width);
      this.UpdateTitleSubtitle();
      double maxWidth = base.Width - 100.0;
      this.textBlockTitle.CorrectText(maxWidth);
      this.textBlockSubtitle.CorrectText(maxWidth);
    }

    private void UpdateTitleSubtitle()
    {
      Place place = this.Geo.place;
      if (!string.IsNullOrEmpty(place != null ? place.group_photo :  null))
        this._groupPhotoUri = new Uri(place.group_photo);
      if (!string.IsNullOrEmpty(this.Geo.AttachmentTitle) && !string.IsNullOrEmpty(this.Geo.AttachmentSubtitle))
      {
        this.textBlockTitle.Text = this.Geo.AttachmentTitle;
        this.textBlockSubtitle.Text = this.Geo.AttachmentSubtitle;
      }
      else
      {
        if (place != null)
        {
          string str1 = "";
          string str2 = "";
          if (!string.IsNullOrEmpty(place.title))
            str1 = place.title;
          if (!string.IsNullOrEmpty(place.address))
            str2 = place.address;
          if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
          {
            this.Geo.AttachmentTitle = str1;
            this.Geo.AttachmentSubtitle = str2;
            this.textBlockTitle.Text = (str1.Replace("\n", " ").Replace("\r", " ").Replace("  ", " "));
            this.textBlockSubtitle.Text = (str2.Replace("\n", " ").Replace("\r", " ").Replace("  ", " "));
            return;
          }
        }
        this.textBlockTitle.Text = ("...");
        this.textBlockSubtitle.Text = ("...");
        this.ReverseGeocode((Action<string, string>) ((title, subtitle) => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (string.IsNullOrEmpty(title))
            title = CommonResources.MapAttachment_Place;
          if (string.IsNullOrEmpty(subtitle))
            subtitle = CommonResources.MapAttachment_CountryNotIdentified;
          this.Geo.AttachmentTitle = title;
          this.Geo.AttachmentSubtitle = subtitle;
          this.textBlockTitle.Text = (title.Replace("\n", " ").Replace("\r", " ").Replace("  ", " "));
          this.textBlockSubtitle.Text = (subtitle.Replace("\n", " ").Replace("\r", " ").Replace("  ", " "));
        }))));
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto, this._groupPhotoUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto,  null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._groupPhotoUri !=  null) || !this._groupPhotoUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._groupPhotoUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPlaceSmallAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) base.FindName("canvas");
      this.imageGroupPhoto = (Image) base.FindName("imageGroupPhoto");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) base.FindName("textBlockSubtitle");
    }
  }
}
