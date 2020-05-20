using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class CountryPickerViewModel : ViewModelBase
  {
    private readonly DelayedExecutor _loadExecutor = new DelayedExecutor(200);
    private List<CountryListItem> _allCountries = new List<CountryListItem>();
    private List<CountryListItem> _nearbyCountries = new List<CountryListItem>();
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _isLoaded;
    private string _query;
    private List<CountryListItem> _countries;
    private bool _isLoading;
    private bool _isError;
    private readonly Country _selectedCountry;
    private readonly bool _allowNoneSelection;

    public string Query
    {
      get
      {
        return this._query;
      }
      set
      {
        this._query = value;
        if (this._isLoaded)
        {
          this.UpdateListWithQueryString();
          this.UpdateSelectedItem();
        }
        else
          this.LoadCountries();
      }
    }

    public List<CountryListItem> Countries
    {
      get
      {
        return this._countries;
      }
      set
      {
        this._countries = value;
        this.NotifyPropertyChanged("Countries");
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FooterText));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FooterTextVisibility));
      }
    }

    public bool IsLoading
    {
      get
      {
        return this._isLoading;
      }
      set
      {
        this._isLoading = value;
        this.NotifyPropertyChanged("IsLoading");
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FooterText));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FooterTextVisibility));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.StatusText));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.StatusTextVisibility));
      }
    }

    public string FooterText
    {
      get
      {
        if (!this._isError && !this._isLoading)
          return this.GetFooterTextForCount(this.Countries.Count);
        return "";
      }
    }

    public Visibility FooterTextVisibility
    {
      get
      {
        return !string.IsNullOrEmpty(this.FooterText) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public string StatusText
    {
      get
      {
        if (this._isLoading)
          return CommonResources.Loading;
        if (!this._isError)
          return "";
        return CommonResources.Error_Generic;
      }
    }

    public Visibility StatusTextVisibility
    {
      get
      {
        return !string.IsNullOrEmpty(this.StatusText) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public CountryPickerViewModel(Country selectedCountry, bool allowNoneSelection)
    {
      this._selectedCountry = selectedCountry;
      this._allowNoneSelection = allowNoneSelection;
      this._countries = new List<CountryListItem>();
    }

    private void UpdateListWithQueryString()
    {
      this.Countries = new List<CountryListItem>(!string.IsNullOrWhiteSpace(this._query) ? this._allCountries.Where<CountryListItem>((Func<CountryListItem, bool>) (listItem => listItem.Title.StartsWith(this._query.Trim(), StringComparison.InvariantCultureIgnoreCase))) : (IEnumerable<CountryListItem>) this._nearbyCountries);
    }

    private string GetFooterTextForCount(int count)
    {
      if (count <= 0)
        return CommonResources.NoCountries;
      if (string.IsNullOrWhiteSpace(this._query) && this._allowNoneSelection)
        --count;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCountryFrm, CommonResources.TwoFourCountriesFrm, CommonResources.FiveCountriesFrm, true, null, false);
    }

    private void UpdateSelectedItem()
    {
      if (this._selectedCountry == null)
        return;
      foreach (CountryListItem country in this.Countries)
      {
        int num = country.Id == this._selectedCountry.id ? 1 : 0;
        country.IsSelected = num != 0;
      }
    }

    public void SelectItem(CountryListItem country)
    {
      if (country == null)
        return;
      foreach (CountryListItem country1 in this.Countries)
      {
        int num = country1.Id == country.Id ? 1 : 0;
        country1.IsSelected = num != 0;
      }
    }

    public void LoadCountries()
    {
      this._isError = false;
      if (this._isLoaded)
        return;
      this._cts.Cancel();
      this._cts = new CancellationTokenSource();
      CancellationToken token = this._cts.Token;
      this.IsLoading = true;
      this._loadExecutor.AddToDelayedExecution((Action) (() =>
      {
        if (token.IsCancellationRequested)
          this.IsLoading = false;
        else
          DatabaseService.Instance.GetCountries(!AppGlobalStateManager.Current.IsUserLoginRequired(), (Action<BackendResult<CountriesResponse, ResultCode>>) (res =>
          {
            if (token.IsCancellationRequested)
              this.IsLoading = false;
            else if (res.ResultCode != ResultCode.Succeeded)
            {
              this._isError = true;
              this.IsLoading = false;
            }
            else
              Execute.ExecuteOnUIThread((Action) (() =>
              {
                this._allCountries = new List<CountryListItem>(res.ResultData.countries.Select<Country, CountryListItem>((Func<Country, CountryListItem>) (country => new CountryListItem(country))));
                this._nearbyCountries = new List<CountryListItem>(res.ResultData.countriesNearby.Select<Country, CountryListItem>((Func<Country, CountryListItem>) (country => new CountryListItem(country))));
                if (this._allowNoneSelection)
                  this._nearbyCountries.Insert(0, new CountryListItem(new Country()
                  {
                    id = 0L,
                    title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
                  }));
                this.UpdateListWithQueryString();
                this.UpdateSelectedItem();
                this.IsLoading = false;
                this._isLoaded = true;
              }));
          }));
      }));
    }
  }
}
