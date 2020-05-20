using System;
using System.Linq.Expressions;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PivotHeader : ViewModelBase
  {
    private string _title = "";
    private string _subtitle = "";
    private double _tilt;
    private bool _hideSubtitle;

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
      }
    }

    public string Subtitle
    {
      get
      {
        return this._subtitle;
      }
      set
      {
        this._subtitle = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Subtitle));
      }
    }

    public double Tilt
    {
      get
      {
        return this._tilt;
      }
      set
      {
        this._tilt = value;
        this.NotifyPropertyChanged<double>((Expression<Func<double>>) (() => this.Tilt));
      }
    }

    public bool HideSubtitle
    {
      get
      {
        return this._hideSubtitle;
      }
      set
      {
        this._hideSubtitle = value;
        this.NotifyPropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.SubtitleOpacity));
      }
    }

    public Decimal SubtitleOpacity
    {
      get
      {
        return (Decimal) (this.HideSubtitle ? 0 : 1);
      }
    }
  }
}
