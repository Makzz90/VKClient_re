using System;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Groups.Management.Information.Library
{
  public sealed class CommunityTypeSelectionViewModel : ViewModelBase
  {
    private string _title;
    private Visibility _visibility;
    private bool? _isOpenedSelected;
    private bool? _isClosedSelected;
    private bool? _isPrivateSelected;
    private Visibility _privateVisibility;
    private string _openedTitle;
    private string _closedTitle;
    private string _openedDescription;
    private string _closedDescription;

    public InformationViewModel ParentViewModel { get; private set; }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.NotifyPropertyChanged<string>((() => this.Title));
      }
    }

    public Visibility Visibility
    {
      get
      {
        return this._visibility;
      }
      set
      {
        this._visibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.Visibility));
      }
    }

    public bool? IsOpenedSelected
    {
      get
      {
        return this._isOpenedSelected;
      }
      set
      {
        this._isOpenedSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsOpenedSelected));
      }
    }

    public bool? IsClosedSelected
    {
      get
      {
        return this._isClosedSelected;
      }
      set
      {
        this._isClosedSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsClosedSelected));
      }
    }

    public bool? IsPrivateSelected
    {
      get
      {
        return this._isPrivateSelected;
      }
      set
      {
        this._isPrivateSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsPrivateSelected));
      }
    }

    public Visibility PrivateVisibility
    {
      get
      {
        return this._privateVisibility;
      }
      set
      {
        this._privateVisibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.PrivateVisibility));
      }
    }

    public string OpenedTitle
    {
      get
      {
        return this._openedTitle;
      }
      set
      {
        this._openedTitle = value;
        this.NotifyPropertyChanged<string>((() => this.OpenedTitle));
      }
    }

    public string ClosedTitle
    {
      get
      {
        return this._closedTitle;
      }
      set
      {
        this._closedTitle = value;
        this.NotifyPropertyChanged<string>((() => this.ClosedTitle));
      }
    }

    public string OpenedDescription
    {
      get
      {
        return this._openedDescription;
      }
      set
      {
        this._openedDescription = value;
        this.NotifyPropertyChanged<string>((() => this.OpenedDescription));
      }
    }

    public string ClosedDescription
    {
      get
      {
        return this._closedDescription;
      }
      set
      {
        this._closedDescription = value;
        this.NotifyPropertyChanged<string>((() => this.ClosedDescription));
      }
    }

    public CommunityTypeSelectionViewModel(InformationViewModel parentViewModel)
    {
      this.ParentViewModel = parentViewModel;
    }

    public void Read(CommunitySettings information)
    {
      if (information.Type == GroupType.PublicPage)
        this.Visibility = Visibility.Collapsed;
      else if (information.Type == GroupType.Group)
      {
        this.Title = CommonResources.GroupType.ToUpper();
        this.PrivateVisibility = Visibility.Visible;
        this.OpenedTitle = CommonResources.GroupType_Opened;
        this.ClosedTitle = CommonResources.GroupType_Closed;
        this.OpenedDescription = CommonResources.GroupType_Opened_Description;
        this.ClosedDescription = CommonResources.GroupType_Closed_Description;
        switch (information.access)
        {
          case 0:
            this.IsOpenedSelected = new bool?(true);
            break;
          case 1:
            this.IsClosedSelected = new bool?(true);
            break;
          case 2:
            this.IsPrivateSelected = new bool?(true);
            break;
        }
      }
      else
      {
        this.Title = CommonResources.EventType.ToUpper();
        this.PrivateVisibility = Visibility.Collapsed;
        this.OpenedTitle = CommonResources.EventType_Opened;
        this.ClosedTitle = CommonResources.EventType_Closed;
        this.OpenedDescription = CommonResources.EventType_Opened_Description;
        this.ClosedDescription = CommonResources.EventType_Closed_Description;
        if (information.access == 0)
          this.IsOpenedSelected = new bool?(true);
        else
          this.IsClosedSelected = new bool?(true);
      }
    }
  }
}
