using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class ProfileInfoItem : ViewModelBase
  {
    private object _data;

    public string Title { get; set; }

    public object Data
    {
      get
      {
        return this._data;
      }
      set
      {
        this._data = value;
        this.NotifyPropertyChanged("Data");
      }
    }

    public ProfileInfoItemType Type { get; private set; }

    public Action NavigationAction { get; protected set; }

    public double Tilt
    {
      get
      {
        if (this.NavigationAction != null)
          return VKConstants.DefaultTilt;
        return 0.0;
      }
    }

    protected ProfileInfoItem(ProfileInfoItemType type = ProfileInfoItemType.RichText)
    {
      this.Type = type;
    }
  }
}
