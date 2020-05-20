using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects.Maps;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC.MapAttachments
{
  public abstract class MapAttachmentUCBase : UserControlVirtualizable
  {
    private double _latitude;
    private double _longitude;

    protected Geo Geo { get; private set; }

    protected MapAttachmentUCBase()
    {
      base.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.OnTap));
    }

    private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (this._latitude == 0.0 && this._longitude == 0.0)
        return;
      Navigator.Current.NavigateToMap(false, this._latitude, this._longitude);
    }

    public void Initialize(Geo geo)
    {
      this.Geo = geo;
      if (!string.IsNullOrEmpty(geo.coordinates))
      {
        string[] strArray = geo.coordinates.Split(' ');
        double result1;
        double result2;
        if (strArray.Length <= 1 || !double.TryParse(strArray[0], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1) || !double.TryParse(strArray[1], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result2))
          return;
        this._latitude = result1;
        this._longitude = result2;
      }
      else
      {
        if (geo.place == null || geo.place.latitude <= 0.0 || geo.place.longitude <= 0.0)
          return;
        this._latitude = geo.place.latitude;
        this._longitude = geo.place.longitude;
      }
    }

    protected void ReverseGeocode(Action<string, string> callback)
    {
      MapsService.Current.ReverseGeocode(this._latitude, this._longitude, (Action<BackendResult<GoogleGeocodeResponse, ResultCode>>) (response =>
      {
        if (response.ResultCode != ResultCode.Succeeded)
        {
          callback("", "");
        }
        else
        {
          string str1 = "";
          string str2 = "";
          GoogleGeocodeResponse resultData = response.ResultData;
          if (resultData != null)
          {
            str1 = !string.IsNullOrEmpty(resultData.Route) ? resultData.Route : resultData.AdministrativeArea2;
            int num = string.Equals(RegionInfo.CurrentRegion.TwoLetterISORegionName, resultData.CountryISO, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
            str2 = resultData.AdministrativeArea1 != str1 ? resultData.AdministrativeArea1 : "";
            if (num == 0 && !string.IsNullOrEmpty(resultData.Country))
            {
              if (!string.IsNullOrEmpty(str2))
                str2 += ", ";
              str2 += resultData.Country;
            }
          }
          callback(str1, str2);
        }
      }));
    }

    protected static double GetMapHeight(double width)
    {
      return width * 9.0 / 16.0;
    }

    protected Uri GetMapUri()
    {
      return MapsService.Current.GetMapUri(this._latitude, this._longitude, 16, (int) base.Width, base.Width / MapAttachmentUCBase.GetMapHeight(base.Width));
    }
  }
}
