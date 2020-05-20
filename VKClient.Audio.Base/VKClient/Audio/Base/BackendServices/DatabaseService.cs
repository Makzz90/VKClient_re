using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
  public class DatabaseService
  {
    private static DatabaseService _instance;
    private CountriesResponse _cachedCountries;

    public static DatabaseService Instance
    {
      get
      {
        if (DatabaseService._instance == null)
          DatabaseService._instance = new DatabaseService();
        return DatabaseService._instance;
      }
    }

    public void GetNearbyCountries(Action<BackendResult<VKList<Country>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["need_all"] = "0";
      parameters["count"] = "500";
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Country>>("database.getCountries", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCountries(bool useExecute, Action<BackendResult<CountriesResponse, ResultCode>> callback)
    {
      if (this._cachedCountries != null)
        callback(new BackendResult<CountriesResponse, ResultCode>(ResultCode.Succeeded, this._cachedCountries));
      if (useExecute)
        this.GetCountriesListWithExecute(callback);
      else
        this.GetCountriesList(callback);
    }

    private void GetCountriesList(Action<BackendResult<CountriesResponse, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Country>>("database.getCountries", new Dictionary<string, string>(), (Action<BackendResult<VKList<Country>, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          Dictionary<string, string> parameters = new Dictionary<string, string>();
          parameters["need_all"] = "1";
          parameters["count"] = "500";
          VKRequestsDispatcher.DispatchRequestToVK<VKList<Country>>("database.getCountries", parameters, (Action<BackendResult<VKList<Country>, ResultCode>>) (res1 =>
          {
            CountriesResponse resultData =  null;
            if (res.ResultCode == ResultCode.Succeeded)
            {
              resultData = new CountriesResponse()
              {
                countriesNearby = res.ResultData.items,
                countries = res1.ResultData.items
              };
              this._cachedCountries = resultData;
            }
            callback(new BackendResult<CountriesResponse, ResultCode>(res1.ResultCode, resultData));
          }),  null, false, true, new CancellationToken?(),  null);
        }
        else
          callback(new BackendResult<CountriesResponse, ResultCode>(res.ResultCode,  null));
      }),  null, false, true, new CancellationToken?(),  null);
    }

    private void GetCountriesListWithExecute(Action<BackendResult<CountriesResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = "var countriesNearby = API.database.getCountries().items;\r\n                                        var countries = API.database.getCountries({\"need_all\": 1, \"count\": 500}).items;\r\n                                        return {\"countriesNearby\": countriesNearby, \"countries\": countries};";
      VKRequestsDispatcher.DispatchRequestToVK<CountriesResponse>("execute", parameters, (Action<BackendResult<CountriesResponse, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          this._cachedCountries = res.ResultData;
        callback(res);
      }),  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCities(string q, long countryId, bool needAll, int offset, int count, Action<BackendResult<VKList<City>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = q;
      parameters["country_id"] = countryId.ToString();
      if (needAll)
        parameters["need_all"] = "1";
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<City>>("database.getCities", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
