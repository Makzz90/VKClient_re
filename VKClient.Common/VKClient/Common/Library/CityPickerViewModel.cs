using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class CityPickerViewModel : ViewModelBase, ICollectionDataProvider2<VKList<City>, CityListItem>
    {
        private string _query;
        private readonly GenericCollectionViewModel2<VKList<City>, CityListItem> _searchVM;
        private readonly long _countryId;
        private readonly City _selectedCity;
        private readonly bool _allowNoneSelection;

        public string Query
        {
            get
            {
                return this._query;
            }
            set
            {
                int num = !string.IsNullOrWhiteSpace(this._query) ? 0 : (string.IsNullOrWhiteSpace(value) ? 1 : 0);
                this._query = value;
                if (num != 0)
                    return;
                this._searchVM.LoadData(true, false, false, true, (Action<List<CityListItem>>)null, (Action<BackendResult<VKList<City>, ResultCode>>)null, false);
            }
        }

        public GenericCollectionViewModel2<VKList<City>, CityListItem> SearchVM
        {
            get
            {
                return this._searchVM;
            }
        }

        public Func<VKList<City>, ListWithCount<CityListItem>> ConverterFunc
        {
            get
            {
                return (Func<VKList<City>, ListWithCount<CityListItem>>)(res =>
                {
                    ListWithCount<CityListItem> listWithCount = new ListWithCount<CityListItem>()
                    {
                        TotalCount = res.count
                    };
                    if (string.IsNullOrWhiteSpace(this._query))
                    {
                        ObservableCollection<CityListItem> collection = this.SearchVM.Collection;
                        Func<CityListItem, bool> predicate = (Func<CityListItem, bool>)(c => c.Id == 0L);
                        //Func<CityListItem, bool> predicate = null;
                        if (collection.FirstOrDefault<CityListItem>(predicate) == null && this._allowNoneSelection)
                        {
                            CityListItem cityListItem = new CityListItem(new City()
                            {
                                id = 0L,
                                title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
                            });
                            if (this._selectedCity == null || this._selectedCity.id == 0L)
                                cityListItem.IsSelected = true;
                            listWithCount.List.Add(cityListItem);
                        }
                    }
                    foreach (City city in res.items)
                    {
                        CityListItem cityListItem = new CityListItem(city);
                        if (this._selectedCity != null && city.id == this._selectedCity.id)
                            cityListItem.IsSelected = true;
                        listWithCount.List.Add(cityListItem);
                    }
                    return listWithCount;
                });
            }
        }

        public CityPickerViewModel(long countryId, City selectedCity = null, bool allowNoneSelection = true)
        {
            this._countryId = countryId;
            this._selectedCity = selectedCity;
            this._allowNoneSelection = allowNoneSelection;
            this._searchVM = new GenericCollectionViewModel2<VKList<City>, CityListItem>((ICollectionDataProvider2<VKList<City>, CityListItem>)this);
        }

        public void SelectItem(CityListItem city)
        {
            if (city == null)
                return;
            foreach (CityListItem cityListItem in (Collection<CityListItem>)this._searchVM.Collection)
            {
                int num = cityListItem.Id == city.Id ? 1 : 0;
                cityListItem.IsSelected = num != 0;
            }
        }

        public void GetData(GenericCollectionViewModel2<VKList<City>, CityListItem> caller, int offset, int count, Action<BackendResult<VKList<City>, ResultCode>> callback)
        {
            DatabaseService.Instance.GetCities(this._query, this._countryId, !string.IsNullOrWhiteSpace(this._query), offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<City>, CityListItem> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoCities;
            if (string.IsNullOrWhiteSpace(this._query) && this._allowNoneSelection)
                --count;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCityFrm, CommonResources.TwoFourCitiesFrm, CommonResources.FiveCitiesFrm, true, null, false);
        }
    }
}
