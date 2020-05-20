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
  public class MapPointSmallAttachmentUC : MapAttachmentUCBase
  {
    public const int FIXED_HEIGHT = 40;
    private const int TEXTBLOCK_MARGIN_LEFT = 40;
    private const int TEXTBLOCK_MARGIN_RIGHT = 8;
    internal Canvas canvas;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public MapPointSmallAttachmentUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
    }

    public override void OnReady()
    {
      ((FrameworkElement) this.canvas).Width=(base.Width);
      this.UpdateTitle();
      this.textBlockTitle.CorrectText(base.Width - 48.0);
    }

    private void UpdateTitle()
    {
      if (!string.IsNullOrEmpty(this.Geo.AttachmentTitle))
      {
        this.textBlockTitle.Text = this.Geo.AttachmentTitle;
      }
      else
      {
        string str = "";
        Place place = this.Geo.place;
        if (place != null)
        {
          if (!string.IsNullOrEmpty(place.title))
          {
            string[] strArray = place.title.Split(',');
            if (strArray.Length != 0)
              str = strArray[0];
          }
          if (!string.IsNullOrEmpty(place.city) && place.city != str)
          {
            if (!string.IsNullOrEmpty(str))
              str += ", ";
            str += place.city;
          }
          else if (!string.IsNullOrEmpty(place.country) && place.country != str)
          {
            if (!string.IsNullOrEmpty(str))
              str += ", ";
            str += place.country;
          }
          if (!string.IsNullOrEmpty(str))
          {
            this.Geo.AttachmentTitle = str;
            this.textBlockTitle.Text = str;
            return;
          }
        }
        this.textBlockTitle.Text = ("...");
        this.ReverseGeocode((Action<string, string>) ((title, subtitle) => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (string.IsNullOrEmpty(title))
            title = CommonResources.MapAttachment_Point;
          else if (!string.IsNullOrEmpty(subtitle))
            this.textBlockTitle.Text = (string.Format("{0}, {1}", title, subtitle));
          this.Geo.AttachmentTitle = title;
          this.textBlockTitle.Text = title;
        }))));
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPointSmallAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) base.FindName("canvas");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
