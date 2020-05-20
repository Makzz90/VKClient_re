using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information.Library
{
  public sealed class CommunityPlacementViewModel : ViewModelBase, IHandle<CommunityPlacementEdited>, IHandle
  {
    private string _descriptionText = "";
    private Place _place;
    private Visibility _visibility;
    private SolidColorBrush _descriptionForeground;
    private Visibility _editButtonVisibility;
    private SolidColorBrush _pinForeground;
    private double _panelTilt;

    public InformationViewModel ParentViewModel { get; private set; }

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

    public string DescriptionText
    {
      get
      {
        return this._descriptionText;
      }
      set
      {
        this._descriptionText = value;
        this.NotifyPropertyChanged<string>((() => this.DescriptionText));
      }
    }

    public SolidColorBrush DescriptionForeground
    {
      get
      {
        return this._descriptionForeground;
      }
      set
      {
        this._descriptionForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.DescriptionForeground));
      }
    }

    public Visibility EditButtonVisibility
    {
      get
      {
        return this._editButtonVisibility;
      }
      set
      {
        this._editButtonVisibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.EditButtonVisibility));
      }
    }

    public SolidColorBrush PinForeground
    {
      get
      {
        return this._pinForeground;
      }
      set
      {
        this._pinForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.PinForeground));
      }
    }

    public double PanelTilt
    {
      get
      {
        return this._panelTilt;
      }
      set
      {
        this._panelTilt = value;
        this.NotifyPropertyChanged<double>((() => this.PanelTilt));
      }
    }

    public CommunityPlacementViewModel(InformationViewModel parentViewModel)
    {
      this.ParentViewModel = parentViewModel;
      EventAggregator.Current.Subscribe(this);
    }

    public void Read(CommunitySettings information)
    {
      if (information.Type == GroupType.PublicPage)
        this.Visibility = Visibility.Collapsed;
      else
        this.SetPlacement(information.place);
    }

    public void SetPlacement(Place place)
    {
      this._place = place;
      this.DescriptionText = "";
      if (place == null || string.IsNullOrWhiteSpace(place.country_name) && string.IsNullOrWhiteSpace(place.city_name) && (string.IsNullOrWhiteSpace(place.address) && string.IsNullOrWhiteSpace(place.title)))
      {
        this.DescriptionText = CommonResources.ChoosePlacement;
        this.DescriptionForeground = this.PinForeground = (SolidColorBrush) Application.Current.Resources["PhoneBlue300Brush"];
        this.EditButtonVisibility = Visibility.Collapsed;
        this.PanelTilt = 2.5;
      }
      else
      {
        if (!string.IsNullOrWhiteSpace(place.title))
          this.DescriptionText = place.title;
        if (!string.IsNullOrWhiteSpace(place.address))
          this.DescriptionText = !(this.DescriptionText == "") ? this.DescriptionText + ", " + place.address : place.address;
        if (this.DescriptionText == "")
        {
          if (!string.IsNullOrWhiteSpace(place.city_name))
            this.DescriptionText = Extensions.ForUI(place.city_name);
          else if (!string.IsNullOrWhiteSpace(place.country_name))
            this.DescriptionText = Extensions.ForUI(place.country_name);
        }
        this.DescriptionForeground = (SolidColorBrush) Application.Current.Resources["PhoneContrastTitleBrush"];
        this.PinForeground = (SolidColorBrush) Application.Current.Resources["PhoneGray300Brush"];
        this.EditButtonVisibility = Visibility.Visible;
        this.PanelTilt = 0.0;
      }
    }

    public void NavigateToPlacementSelection()
    {
      Navigator.Current.NavigateToCommunityManagementPlacementSelection(this.ParentViewModel.CommunityId, this._place);
    }

    public void Handle(CommunityPlacementEdited message)
    {
      this.SetPlacement(message.Place);
    }
  }
}
