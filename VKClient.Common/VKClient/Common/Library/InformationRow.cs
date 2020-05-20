using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Library
{
  public class InformationRow
  {
    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string ImageUri { get; set; }

    public bool IsHighlighted { get; set; }

    public Brush TitleForegroundBrush
    {
      get
      {
        return Application.Current.Resources[this.IsHighlighted ? (object) "PhoneForegroundBrush" : (object) "PhoneVKSubtleBrush"] as Brush;
      }
    }

    public Brush SubtitleForegroundBrush
    {
      get
      {
        return Application.Current.Resources[this.IsHighlighted ? (object) "PhoneProfileRowHightlightedForegroundBrush" : (object) "PhoneVKSubtleBrush"] as Brush;
      }
    }

    public double Tilt
    {
      get
      {
        if (!this.CanNavigate)
          return 0.0;
        return VKConstants.DefaultTilt;
      }
    }

    public bool CanNavigate { get; set; }

    public bool NoNavigation
    {
      get
      {
        return !this.CanNavigate;
      }
    }

    public Visibility HaveImage
    {
      get
      {
        return !string.IsNullOrEmpty(this.ImageUri) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public virtual void Navigate()
    {
    }
  }
}
