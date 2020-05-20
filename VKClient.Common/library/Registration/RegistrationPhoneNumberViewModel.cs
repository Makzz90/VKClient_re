using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

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
        this.NotifyPropertyChanged<Country>((Expression<Func<Country>>)(() => this.Country));
        if (this._country == null)
          return;
        Dictionary<string, long>.Enumerator enumerator = CountriesPhoneCodes.CodeToCountryDict.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            KeyValuePair<string, long> current = enumerator.Current;
            if (current.Value == this._country.id)
            {
              this._phonePrefix = current.Key;
              this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhonePrefix));
              this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhoneNumberString));
              this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsCompleted));
            }
          }
        }
        finally
        {
          enumerator.Dispose();
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
        this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhonePrefix));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhoneNumberString));
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsCompleted));
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
        this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhoneNumber));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhoneNumberString));
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsCompleted));
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
        {
            resultCallback.Invoke(true);
            return;
        }
        DatabaseService.Instance.GetNearbyCountries(delegate(BackendResult<VKList<Country>, ResultCode> res)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    this._initialized = true;
                    this.Country = Enumerable.FirstOrDefault<Country>(res.ResultData.items);
                }
                resultCallback.Invoke(res.ResultCode == ResultCode.Succeeded);
            });
        });
    }
  }
}
