using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.UC
{
  public class AttachmentPickerItem : ViewModelBase
  {
    private string _text;
    //private SolidColorBrush _foreground;
    private bool _isHighlighted;

    public string Text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Text));
      }
    }

    public bool IsHighlighted
    {
      get
      {
        return this._isHighlighted;
      }
      set
      {
        this._isHighlighted = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsHighlighted));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.Foreground));
      }
    }

    public SolidColorBrush Foreground
    {
      get
      {
        return Application.Current.Resources[this.IsHighlighted ? (object) "PhoneButtonTextForegroundBrush" : (object) "PhoneMenuForegroundBrush"] as SolidColorBrush;
      }
    }

    public NamedAttachmentType AttachmentType { get; set; }
  }
}
