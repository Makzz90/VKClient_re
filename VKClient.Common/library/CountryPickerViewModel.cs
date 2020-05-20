using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
                base.NotifyPropertyChanged<string>(() => this.FooterText);
                base.NotifyPropertyChanged<Visibility>(() => this.FooterTextVisibility);
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
                base.NotifyPropertyChanged<string>(() => this.FooterText);
                base.NotifyPropertyChanged<Visibility>(() => this.FooterTextVisibility);
                base.NotifyPropertyChanged<string>(() => this.StatusText);
                base.NotifyPropertyChanged<Visibility>(() => this.StatusTextVisibility);
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
                if (!string.IsNullOrEmpty(this.FooterText))
                    return Visibility.Visible;
                return Visibility.Collapsed;
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
                if (!string.IsNullOrEmpty(this.StatusText))
                    return Visibility.Visible;
                return Visibility.Collapsed;
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
            IEnumerable<CountryListItem> arg_2F_0;
            if (string.IsNullOrWhiteSpace(this._query))
            {
                IEnumerable<CountryListItem> nearbyCountries = this._nearbyCountries;
                arg_2F_0 = nearbyCountries;
            }
            else
            {
                arg_2F_0 = Enumerable.Where<CountryListItem>(this._allCountries, (CountryListItem listItem) => listItem.Title.StartsWith(this._query.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }
            this.Countries = new List<CountryListItem>(arg_2F_0);
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
            List<CountryListItem>.Enumerator enumerator = this.Countries.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    CountryListItem current = enumerator.Current;
                    int num = current.Id == this._selectedCountry.id ? 1 : 0;
                    current.IsSelected = num != 0;
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        public void SelectItem(CountryListItem country)
        {
            if (country == null)
                return;
            List<CountryListItem>.Enumerator enumerator = this.Countries.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    CountryListItem current = enumerator.Current;
                    int num = current.Id == country.Id ? 1 : 0;
                    current.IsSelected = num != 0;
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        public void LoadCountries()
        {
            this._isError = false;
            if (this._isLoaded)
            {
                return;
            }
            this._cts.Cancel();
            this._cts = new CancellationTokenSource();
            CancellationToken token = this._cts.Token;
            this.IsLoading = true;
            Action<BackendResult<CountriesResponse, ResultCode>> _9__1=null;
            this._loadExecutor.AddToDelayedExecution(delegate
            {
                if (token.IsCancellationRequested)
                {
                    this.IsLoading = false;
                    return;
                }
                bool flag = !AppGlobalStateManager.Current.IsUserLoginRequired();
                DatabaseService arg_4D_0 = DatabaseService.Instance;
                bool arg_4D_1 = flag;
                Action<BackendResult<CountriesResponse, ResultCode>> arg_4D_2;
                if ((arg_4D_2 = _9__1) == null)
                {
                    arg_4D_2 = (_9__1 = delegate(BackendResult<CountriesResponse, ResultCode> res)
                    {
                        if (token.IsCancellationRequested)
                        {
                            this.IsLoading = false;
                            return;
                        }
                        if (res.ResultCode != ResultCode.Succeeded)
                        {
                            this._isError = true;
                            this.IsLoading = false;
                            return;
                        }
                        Execute.ExecuteOnUIThread(delegate
                        {
                            CountryPickerViewModel arg_44_0 = this;
                            IEnumerable<Country> arg_3A_0 = res.ResultData.countries;
                            Func<Country, CountryListItem> arg_3A_1 = new Func<Country, CountryListItem>((country) => { return new CountryListItem(country); });

                            arg_44_0._allCountries = new List<CountryListItem>(Enumerable.Select<Country, CountryListItem>(arg_3A_0, arg_3A_1));
                            CountryPickerViewModel arg_8D_0 = this;
                            IEnumerable<Country> arg_83_0 = res.ResultData.countriesNearby;
                            Func<Country, CountryListItem> arg_83_1 = new Func<Country, CountryListItem>((country) => { return new CountryListItem(country); });

                            arg_8D_0._nearbyCountries = new List<CountryListItem>(Enumerable.Select<Country, CountryListItem>(arg_83_0, arg_83_1));
                            if (this._allowNoneSelection)
                            {
                                this._nearbyCountries.Insert(0, new CountryListItem(new Country
                                {
                                    id = 0,
                                    title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
                                }));
                            }
                            this.UpdateListWithQueryString();
                            this.UpdateSelectedItem();
                            this.IsLoading = false;
                            this._isLoaded = true;
                        });
                    });
                }
                arg_4D_0.GetCountries(arg_4D_1, arg_4D_2);
            });
        }
    }
}
