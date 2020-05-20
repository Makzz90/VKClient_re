using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.UC
{
  public class AttachmentPickerItemViewModel : ViewModelBase
  {
    private string _icon;
    private string _title;
    private bool _isHighlighted;

    public string Icon
    {
      get
      {
        if (string.IsNullOrEmpty(this.HighlightedIcon) || !this._isHighlighted)
          return this._icon;
        return this.HighlightedIcon;
      }
      set
      {
        this._icon = value;
        base.NotifyPropertyChanged<string>(() => this.Icon);
      }
    }

    public string HighlightedIcon { get; set; }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        base.NotifyPropertyChanged<string>(() => this.Title);
      }
    }

    public SolidColorBrush IconBackground
    {
      get
      {
        return (SolidColorBrush) Application.Current.Resources[this.IsHighlighted ? "PhoneListItemAccentForegroundBrush" : "PhoneCommunityManagementSectionIconBrush"];
      }
    }

    public SolidColorBrush TitleForeground
    {
      get
      {
        return (SolidColorBrush) Application.Current.Resources[this.IsHighlighted ? "PhoneListItemAccentForegroundBrush" : "PhoneCommunityManagementSectionTitleBrush"];
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
        base.NotifyPropertyChanged<string>(() => this.Icon);
        base.NotifyPropertyChanged<bool>(() => this.IsHighlighted);
        base.NotifyPropertyChanged<SolidColorBrush>(() => this.IconBackground);
        base.NotifyPropertyChanged<SolidColorBrush>(() => this.TitleForeground);
      }
    }

    public NamedAttachmentType AttachmentType { get; set; }
  }
}
