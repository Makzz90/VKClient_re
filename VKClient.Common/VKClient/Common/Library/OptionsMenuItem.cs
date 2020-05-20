using System;
using System.Linq.Expressions;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class OptionsMenuItem : ViewModelBase
  {
    private OptionsMenuItemType _type;

    public OptionsMenuItemType Type
    {
      get
      {
        return this._type;
      }
      set
      {
        this._type = value;
        string str = "";
        switch (this._type)
        {
          case OptionsMenuItemType.Search:
            str = "/Resources/Search32px.png";
            break;
          case OptionsMenuItemType.More:
            str = "/Resources/MoreHorizontal32px.png";
            break;
        }
        this.Icon = str;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Icon));
      }
    }

    public string Icon { get; private set; }
  }
}
