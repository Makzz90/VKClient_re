using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPlaceFullAttachmentUC : MapAttachmentUCBase
  {
    private Uri _mapUri;
    private Uri _groupPhotoUri;
    private const int PLACE_HEIGHT = 80;
    internal Canvas canvas;
    internal Rectangle rectanglePlaceholder;
    internal Image imageMap;
    internal Image imageMapIcon;
    internal Rectangle rectMapBorderBottom;
    internal Grid gridPlace;
    internal Image imageGroupPhoto;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    internal Rectangle rectBorder;
    private bool _contentLoaded;

    public MapPlaceFullAttachmentUC()
    {
      this.InitializeComponent();
    }

    public static double CalculateTotalHeight(double width)
    {
      return MapAttachmentUCBase.GetMapHeight(width) + 80.0;
    }

    public override void OnReady()
    {
      double mapHeight = MapAttachmentUCBase.GetMapHeight(base.Width);
      double totalHeight = MapPlaceFullAttachmentUC.CalculateTotalHeight(base.Width);
      this._mapUri = this.GetMapUri();
      ((FrameworkElement) this.canvas).Width=(base.Width);
      ((FrameworkElement) this.canvas).Height=(mapHeight + 80.0);
      ((FrameworkElement) this.rectBorder).Width=(base.Width);
      ((FrameworkElement) this.rectBorder).Height = totalHeight;
      ((FrameworkElement) this.rectanglePlaceholder).Width=(base.Width);
      ((FrameworkElement) this.rectanglePlaceholder).Height = mapHeight;
      ((FrameworkElement) this.imageMap).Width=(base.Width);
      ((FrameworkElement) this.imageMap).Height = mapHeight;
      Canvas.SetLeft((UIElement) this.imageMapIcon, base.Width / 2.0 - ((FrameworkElement) this.imageMapIcon).Width / 2.0);
      Canvas.SetTop((UIElement) this.imageMapIcon, mapHeight / 2.0 - ((FrameworkElement) this.imageMapIcon).Height);
      ((FrameworkElement) this.rectMapBorderBottom).Width=(base.Width - 2.0);
      Canvas.SetTop((UIElement) this.rectMapBorderBottom, mapHeight - 1.0);
      Canvas.SetTop((UIElement) this.gridPlace, mapHeight);
      this.UpdateTitleSubtitle();
      double width = base.Width;
      Thickness margin = ((FrameworkElement) this.textBlockTitle).Margin;
      // ISSUE: explicit reference operation
      double left = ((Thickness) @margin).Left;
      double val2 = width - left;
      this.textBlockTitle.CorrectText(Math.Max(0.0, val2));
      this.textBlockSubtitle.CorrectText(Math.Max(0.0, val2));
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
            str1 = place.title.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          if (!string.IsNullOrEmpty(place.address))
            str2 = place.address.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
          {
            this.Geo.AttachmentTitle = str1;
            this.Geo.AttachmentSubtitle = str2;
            this.textBlockTitle.Text = str1;
            this.textBlockSubtitle.Text = str2;
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
          this.Geo.AttachmentTitle = title.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          this.Geo.AttachmentSubtitle = subtitle.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
        }))));
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, this._mapUri);
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto, this._groupPhotoUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap,  null);
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto,  null);
    }

    public override void ShownOnScreen()
    {
      DateTime now;
      if (this._groupPhotoUri !=  null && this._groupPhotoUri.IsAbsoluteUri)
      {
        string originalString = this._groupPhotoUri.OriginalString;
        now = DateTime.Now;
        long ticks = now.Ticks;
        VeryLowProfileImageLoader.SetPriority(originalString, ticks);
      }
      if (!(this._mapUri !=  null) || !this._mapUri.IsAbsoluteUri)
        return;
      string originalString1 = this._mapUri.OriginalString;
      now = DateTime.Now;
      long ticks1 = now.Ticks;
      VeryLowProfileImageLoader.SetPriority(originalString1, ticks1);
    }

    private void GridPlace_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      Place place = this.Geo.place;
      long groupId = place != null ? place.group_id : 0L;
      if (groupId <= 0L)
        return;
      Navigator.Current.NavigateToGroup(groupId, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPlaceFullAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) base.FindName("canvas");
      this.rectanglePlaceholder = (Rectangle) base.FindName("rectanglePlaceholder");
      this.imageMap = (Image) base.FindName("imageMap");
      this.imageMapIcon = (Image) base.FindName("imageMapIcon");
      this.rectMapBorderBottom = (Rectangle) base.FindName("rectMapBorderBottom");
      this.gridPlace = (Grid) base.FindName("gridPlace");
      this.imageGroupPhoto = (Image) base.FindName("imageGroupPhoto");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) base.FindName("textBlockSubtitle");
      this.rectBorder = (Rectangle) base.FindName("rectBorder");
    }
  }
}
