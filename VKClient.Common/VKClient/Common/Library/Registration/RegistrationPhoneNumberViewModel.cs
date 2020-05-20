using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationPhoneNumberViewModel : ViewModelBase, ICompleteable, IBinarySerializable
  {
    private string _phonePrefix = "";
    private string _phoneNumber = "";
    private Country _country;
    private bool _initialized;

    public string PhoneNumberString
    {
      get
      {
        return this.PhonePrefix + this.PhoneNumber;
      }
    }

    public Country Country
    {
      get
      {
        return this._country;
      }
      set
      {
        this._country = value;
        this.NotifyPropertyChanged<Country>((Expression<Func<Country>>) (() => this.Country));
        if (this._country == null)
          return;
        foreach (KeyValuePair<string, long> keyValuePair in CountriesPhoneCodes.CodeToCountryDict)
        {
          if (keyValuePair.Value == this._country.id)
          {
            this._phonePrefix = keyValuePair.Key;
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhonePrefix));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhoneNumberString));
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsCompleted));
          }
        }
      }
    }

    public string PhonePrefix
    {
      get
      {
        return this._phonePrefix;
      }
      set
      {
        this._phonePrefix = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhonePrefix));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhoneNumberString));
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsCompleted));
      }
    }

    public string PhoneNumber
    {
      get
      {
        return this._phoneNumber;
      }
      set
      {
        this._phoneNumber = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhoneNumber));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.PhoneNumberString));
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsCompleted));
      }
    }

    public bool IsCompleted
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.PhoneNumber))
          return !string.IsNullOrWhiteSpace(this.PhonePrefix);
        return false;
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._phonePrefix);
      writer.WriteString(this._phoneNumber);
      writer.Write<Country>(this._country, false);
      writer.Write(this._initialized);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._phonePrefix = reader.ReadString();
      this._phoneNumber = reader.ReadString();
      this._country = reader.ReadGeneric<Country>();
      this._initialized = reader.ReadBoolean();
    }

    public void EnsureInitialized(Action<bool> resultCallback)
    {
      if (this._initialized)
        resultCallback(true);
      else
        DatabaseService.Instance.GetNearbyCountries((Action<BackendResult<VKList<Country>, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
          if (res.ResultCode == ResultCode.Succeeded)
          {
            this._initialized = true;
            this.Country = res.ResultData.items.FirstOrDefault<Country>();
          }
          resultCallback(res.ResultCode == ResultCode.Succeeded);
        }))));
    }
  }
}
