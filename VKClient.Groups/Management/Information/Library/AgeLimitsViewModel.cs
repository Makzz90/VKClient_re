using System;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;

namespace VKClient.Groups.Management.Information.Library
{
  public sealed class AgeLimitsViewModel : ViewModelBase
  {
    private bool? _isNoLimits;
    private bool? _from16Only;
    private bool? _from18Only;
    private Visibility _fullFormVisibility;

    public InformationViewModel ParentViewModel { get; private set; }

    public bool? IsNoLimits
    {
      get
      {
        return this._isNoLimits;
      }
      set
      {
        this._isNoLimits = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsNoLimits));
      }
    }

    public bool? From16Only
    {
      get
      {
        return this._from16Only;
      }
      set
      {
        this._from16Only = value;
        this.NotifyPropertyChanged<bool?>((() => this.From16Only));
      }
    }

    public bool? From18Only
    {
      get
      {
        return this._from18Only;
      }
      set
      {
        this._from18Only = value;
        this.NotifyPropertyChanged<bool?>((() => this.From18Only));
      }
    }

    public Visibility FullFormVisibility
    {
      get
      {
        return this._fullFormVisibility;
      }
      set
      {
        this._fullFormVisibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.FullFormVisibility));
        this.NotifyPropertyChanged<Visibility>((() => this.SetAgeLimitsButtonVisibility));
      }
    }

    public Visibility SetAgeLimitsButtonVisibility
    {
      get
      {
          return (this.FullFormVisibility == Visibility.Collapsed).ToVisiblity();
      }
    }

    public AgeLimitsViewModel(InformationViewModel parentViewModel)
    {
      this.ParentViewModel = parentViewModel;
    }

    public void Read(CommunitySettings information)
    {
      switch (information.age_limits)
      {
        case 1:
          this.IsNoLimits = new bool?(true);
          break;
        case 2:
          this.From16Only = new bool?(true);
          break;
        case 3:
          this.From18Only = new bool?(true);
          break;
      }
      this.FullFormVisibility = (information.age_limits != 1).ToVisiblity();
    }
  }
}
