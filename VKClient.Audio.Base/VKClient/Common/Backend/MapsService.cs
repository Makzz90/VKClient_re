using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VKClient.Audio.Base.DataObjects.Maps;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class MapsService
  {
    private static readonly string _mapUriFormat = "https://maps.googleapis.com/maps/api/staticmap?center={0}&zoom={1}&size={2}x{3}&scale={4}&sensor=true&language={5}";
    private static readonly string _reverseGeocodeUriFormat = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0}&language={1}";
    public const string KEY = "AsAOCzjdoO4A8lKbpU4hZzrs4piUJ0g4jQZ-FbL4AUmy_cbfoOQaqN5usCNwG0Ua";
    public const string APPLICATION_ID = "55677f7c-3dab-4a57-95b2-4efd44a0e692";
    public const string AUTHENTICATION_TOKEN = "1jh4FPILRSo9J1ADKx2CgA";
    private const int MAX_MAP_WIDTH = 640;

    public static MapsService Current = new MapsService();

    public Uri GetMapUri(double latitude, double longitude, int zoomLevel, int width, double heightDivisionFactor)
    {
      int num1 = width * ScaleFactor.GetRealScaleFactor() / 100;
      int num2 = 640;
      int num3 = num1 > num2 ? 2 : 1;
      int num4 = num3;
      int num5 = num1 / num4;
      int num6 = (int) ((double) num5 / heightDivisionFactor);
      return new Uri(string.Format(MapsService._mapUriFormat, this.CoordinatesToString(latitude, longitude), zoomLevel, num5, num6, num3, CultureInfo.CurrentCulture));
    }

    public void ReverseGeocodeToAddress(double latitude, double longitude, Action<BackendResult<string, ResultCode>> callback)
    {
      JsonWebRequest.SendHTTPRequestAsync(string.Format(MapsService._reverseGeocodeUriFormat, this.CoordinatesToString(latitude, longitude), CultureInfo.CurrentCulture), (Action<JsonResponseData>) (data =>
      {
        if (!data.IsSucceeded)
        {
          callback(new BackendResult<string, ResultCode>(ResultCode.CommunicationFailed));
        }
        else
        {
          GoogleGeocode googleGeocode = JsonConvert.DeserializeObject<GoogleGeocode>(data.JsonString);
          if (googleGeocode.results != null && googleGeocode.results.Count > 0)
          {
            GoogleGeocodeResult googleGeocodeResult = googleGeocode.results.FirstOrDefault<GoogleGeocodeResult>((Func<GoogleGeocodeResult, bool>) (r =>
            {
              if (r.address_components != null)
                return r.address_components.Count > 0;
              return false;
            }));
            if (googleGeocodeResult != null)
            {
              callback(new BackendResult<string, ResultCode>(ResultCode.Succeeded, googleGeocodeResult.formatted_address));
              return;
            }
          }
          callback(new BackendResult<string, ResultCode>(ResultCode.UnknownError));
        }
      }),  null);
    }

    public void ReverseGeocode(double latitude, double longitude, Action<BackendResult<GoogleGeocodeResponse, ResultCode>> callback)
    {
      JsonWebRequest.SendHTTPRequestAsync(string.Format(MapsService._reverseGeocodeUriFormat, this.CoordinatesToString(latitude, longitude), CultureInfo.CurrentCulture), (Action<JsonResponseData>) (data =>
      {
        if (!data.IsSucceeded)
        {
          callback(new BackendResult<GoogleGeocodeResponse, ResultCode>(ResultCode.CommunicationFailed));
        }
        else
        {
          GoogleGeocode googleGeocode = JsonConvert.DeserializeObject<GoogleGeocode>(data.JsonString);
          if (googleGeocode.results != null && googleGeocode.results.Count > 0)
          {
            GoogleGeocodeResult googleGeocodeResult = googleGeocode.results.FirstOrDefault<GoogleGeocodeResult>((Func<GoogleGeocodeResult, bool>) (r =>
            {
              if (r.address_components != null)
                return r.address_components.Count > 0;
              return false;
            }));
            if (googleGeocodeResult != null)
            {
              List<GoogleAddressComponent> addressComponents = googleGeocodeResult.address_components;
              string str1 = "";
              string str2 = "";
              string str3 = "";
              string str4 = "";
              string str5 = "";
              foreach (GoogleAddressComponent addressComponent in addressComponents)
              {
                if (addressComponent.types != null && addressComponent.types.Count != 0)
                {
                  List<string> types = addressComponent.types;
                  if (types.Contains("route") && string.IsNullOrEmpty(str1))
                    str1 = addressComponent.long_name;
                  else if (types.Contains("administrative_area_level_1") && string.IsNullOrEmpty(str2))
                    str2 = addressComponent.long_name;
                  else if (types.Contains("administrative_area_leve_2") && string.IsNullOrEmpty(str3))
                    str3 = addressComponent.long_name;
                  else if (types.Contains("country") && string.IsNullOrEmpty(str4))
                  {
                    str4 = addressComponent.long_name;
                    str5 = addressComponent.short_name;
                  }
                }
              }
              callback(new BackendResult<GoogleGeocodeResponse, ResultCode>(ResultCode.Succeeded, new GoogleGeocodeResponse()
              {
                Route = str1,
                AdministrativeArea1 = str2,
                AdministrativeArea2 = str3,
                Country = str4,
                CountryISO = str5
              }));
              return;
            }
          }
          callback(new BackendResult<GoogleGeocodeResponse, ResultCode>(ResultCode.UnknownError));
        }
      }),  null);
    }

    private string CoordinatesToString(double latitude, double longitude)
    {
      return string.Format("{0},{1}", latitude.ToString((IFormatProvider) CultureInfo.InvariantCulture), longitude.ToString((IFormatProvider) CultureInfo.InvariantCulture));
    }
  }
}
