using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class InfoListItem : ViewModelBase
  {
    private string _text;

    public string IconUrl { get; set; }

    public InlinesCollection Inlines { get; set; }

    public string Text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = value;
        this.NotifyPropertyChanged("Text");
      }
    }

    public string Preview1 { get; set; }

    public string Preview2 { get; set; }

    public string Preview3 { get; set; }

    public bool IsTiltEnabled { get; set; }

    public Action TapAction { get; set; }
  }
}
