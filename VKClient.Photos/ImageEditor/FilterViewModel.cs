using System;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Photos.ImageEditor
{
  public class FilterViewModel : ViewModelBase
  {
    private Visibility _isSelectedVisibilty;

    public string FilterImage { get; set; }

    public string FilterName { get; set; }

    public Visibility IsSelectedVisibility
    {
      get
      {
        return this._isSelectedVisibilty;
      }
      set
      {
        this._isSelectedVisibilty = value;
        this.NotifyPropertyChanged<Visibility>((() => this.IsSelectedVisibility));
      }
    }
  }
}
