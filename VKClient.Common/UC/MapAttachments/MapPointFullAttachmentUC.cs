using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPointFullAttachmentUC : MapAttachmentUCBase
  {
    private Uri _mapUri;
    private const int TEXTBLOCK_HEIGHT = 24;
    private const int TITLE_MARGIN_TOP = 9;
    private const int MARGIN_BOTTOM = 15;
    private const int TEXTBLOCK_MARGIN_LEFT = 16;
    internal Canvas canvas;
    internal Rectangle rectanglePlaceholder;
    internal Image imageMap;
    internal Image imageMapIcon;
    internal Rectangle rectMapBorderBottom;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    internal Rectangle rectBorder;
    private bool _contentLoaded;

    public MapPointFullAttachmentUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      this.textBlockSubtitle.Text = ("");
    }

    public static double CalculateTotalHeight(double width)
    {
      return MapAttachmentUCBase.GetMapHeight(width) + 48.0 + 9.0 + 15.0;
    }

    public override void OnReady()
    {
      double mapHeight = MapAttachmentUCBase.GetMapHeight(base.Width);
      double totalHeight = MapPointFullAttachmentUC.CalculateTotalHeight(base.Width);
      this._mapUri = this.GetMapUri();
      ((FrameworkElement) this.canvas).Width=(base.Width);
      ((FrameworkElement) this.canvas).Height = totalHeight;
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
      Canvas.SetTop((UIElement) this.textBlockTitle, mapHeight + 9.0);
      Canvas.SetTop((UIElement) this.textBlockSubtitle, mapHeight + 24.0 + 9.0);
      this.UpdateTitleSubtitle();
      double maxWidth = base.Width - 32.0;
      this.textBlockTitle.CorrectText(maxWidth);
      this.textBlockSubtitle.CorrectText(maxWidth);
    }

    private void UpdateTitleSubtitle()
    {
      if (!string.IsNullOrEmpty(this.Geo.AttachmentTitle) && !string.IsNullOrEmpty(this.Geo.AttachmentSubtitle))
      {
        this.textBlockTitle.Text = this.Geo.AttachmentTitle;
        this.textBlockSubtitle.Text = this.Geo.AttachmentSubtitle;
      }
      else
      {
        Place place = this.Geo.place;
        if (place != null)
        {
          string str1 = "";
          string str2 = "";
          if (!string.IsNullOrEmpty(place.title))
          {
            string[] strArray = place.title.Split(',');
            if (strArray.Length != 0)
              str1 = strArray[0];
            if (!string.IsNullOrEmpty(place.city))
              str2 = place.city;
            else if (!string.IsNullOrEmpty(place.country))
              str2 = place.country;
          }
          if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
          {
            if (str1 == str2)
              str1 = CommonResources.MapAttachment_Point;
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
            title = CommonResources.MapAttachment_Point;
          if (string.IsNullOrEmpty(subtitle))
            subtitle = CommonResources.MapAttachment_CountryNotIdentified;
          this.Geo.AttachmentTitle = title;
          this.Geo.AttachmentSubtitle = subtitle;
          this.textBlockTitle.Text = title;
          this.textBlockSubtitle.Text = subtitle;
        }))));
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, this._mapUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap,  null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._mapUri !=  null) || !this._mapUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._mapUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPointFullAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) base.FindName("canvas");
      this.rectanglePlaceholder = (Rectangle) base.FindName("rectanglePlaceholder");
      this.imageMap = (Image) base.FindName("imageMap");
      this.imageMapIcon = (Image) base.FindName("imageMapIcon");
      this.rectMapBorderBottom = (Rectangle) base.FindName("rectMapBorderBottom");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) base.FindName("textBlockSubtitle");
      this.rectBorder = (Rectangle) base.FindName("rectBorder");
    }
  }
}
