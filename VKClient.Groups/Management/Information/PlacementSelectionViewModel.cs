using System;
using System.Device.Location;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Groups.Management.Information
{
  public sealed class PlacementSelectionViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private string _address = "";
    private string _place = "";
    private readonly long _communityId;
    private long _countryId;
    private long _cityId;
    private string _country;
    private string _city;

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
      }
    }

    public string Country
    {
      get
      {
        return this._country;
      }
      set
      {
        this._country = value;
        this.NotifyPropertyChanged<string>((() => this.Country));
        this.NotifyPropertyChanged<Visibility>((() => this.CityFieldVisibility));
        this._cityId = 0L;
        this.City =  null;
      }
    }

    public string City
    {
      get
      {
        return this._city;
      }
      set
      {
        this._city = value;
        this.NotifyPropertyChanged<string>((() => this.City));
        this.NotifyPropertyChanged<Visibility>((() => this.AddressFieldVisibility));
      }
    }

    public string Address
    {
      get
      {
        return this._address;
      }
      set
      {
        this._address = value;
        this.NotifyPropertyChanged<string>((() => this.Address));
        this.NotifyPropertyChanged<double>((() => this.AddressPlaceholderOpacity));
      }
    }

    public string Place
    {
      get
      {
        return this._place;
      }
      set
      {
        this._place = value;
        this.NotifyPropertyChanged<string>((() => this.Place));
      }
    }

    public GeoCoordinate GeoCoordinate { get; set; }

    public Visibility CityFieldVisibility
    {
      get
      {
        return (this.Country != null).ToVisiblity();
      }
    }

    public Visibility AddressFieldVisibility
    {
      get
      {
        return (this.Country != null && this.City != null).ToVisiblity();
      }
    }

    public double AddressPlaceholderOpacity
    {
      get
      {
        return string.IsNullOrWhiteSpace(this.Address) ? 1.0 : 0.0;
      }
    }

    public PlacementSelectionViewModel(long communityId, VKClient.Common.Backend.DataObjects.Place place)
    {
      this._communityId = communityId;
      if (place == null)
        return;
      if (place.country_name != null)
      {
        this._countryId = place.country_id;
        this.Country = place.country_name;
      }
      if (place.city_name != null)
      {
        this._cityId = place.city_id;
        this.City = place.city_name;
      }
      this.Address = place.address;
      this.Place = place.title;
      this.GeoCoordinate = new GeoCoordinate(place.latitude, place.longitude);
    }

    public void ChooseCountry()
    {
      VKClient.Common.Backend.DataObjects.Country selectedCountry = new VKClient.Common.Backend.DataObjects.Country();
      selectedCountry.id = this._countryId;
      int num = 1;
      Action<VKClient.Common.Backend.DataObjects.Country> countryPickedCallback = (Action<VKClient.Common.Backend.DataObjects.Country>) (c =>
      {
        this._countryId = c.id;
        this.Country = c.id != 0L ? c.name :  null;
      });
      // ISSUE: variable of the null type
      
      CountryPickerUC.Show(selectedCountry, num != 0, countryPickedCallback, null);
    }

    public void ChooseCity()
    {
      long countryId = this._countryId;
      VKClient.Common.Backend.DataObjects.City selectedCity = new VKClient.Common.Backend.DataObjects.City();
      selectedCity.id = this._cityId;
      int num = 1;
      Action<VKClient.Common.Backend.DataObjects.City> cityPickedCallback = (Action<VKClient.Common.Backend.DataObjects.City>) (c =>
      {
        this._cityId = c.id;
        this.City = c.id != 0L ? c.name :  null;
      });
      // ISSUE: variable of the null type
      
      CityPickerUC.Show(countryId, selectedCity, num != 0, cityPickedCallback, null);
    }

    public void SaveChanges()
    {
        this.SetInProgress(true, "");
        this.IsFormEnabled = false;
        GroupsService current = GroupsService.Current;
        long communityId = this._communityId;
        long countryId = this._countryId;
        long cityId = this._cityId;
        string address = this.Address;
        string place1 = this.Place;
        GeoCoordinate geoCoordinate1 = this.GeoCoordinate;
        double latitude = (object)geoCoordinate1 != null ? geoCoordinate1.Latitude : 0.0;
        GeoCoordinate geoCoordinate2 = this.GeoCoordinate;
        double longitude = (object)geoCoordinate2 != null ? geoCoordinate2.Longitude : 0.0;
        Action<BackendResult<PlacementEditingResult, ResultCode>> callback = (Action<BackendResult<PlacementEditingResult, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                VKClient.Common.Backend.DataObjects.Place place2 = new VKClient.Common.Backend.DataObjects.Place()
                {
                    country_id = this._countryId,
                    country_name = this.Country,
                    city_id = this._cityId,
                    city_name = this.City,
                    address = this.Address,
                    title = this.Place,
                    latitude = this.GeoCoordinate.Latitude,
                    longitude = this.GeoCoordinate.Longitude,
                    group_id = this._communityId
                };
                Navigator.Current.GoBack();
                EventAggregator.Current.Publish((object)new CommunityPlacementEdited()
                {
                    Place = place2
                });
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }
        })));
        current.SetCommunityPlacement(communityId, countryId, cityId, address, place1, latitude, longitude, callback);
    }
  }
}
