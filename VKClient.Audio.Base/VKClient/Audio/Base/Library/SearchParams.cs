using System.Collections.Generic;

namespace VKClient.Audio.Base.Library
{
  public class SearchParams
  {
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    public bool IsAnySet
    {
      get
      {
        return this._parameters.Count > 0;
      }
    }

    public T SetValue<T>(string key, T value, bool isDefault = false)
    {
      if (isDefault || EqualityComparer<T>.Default.Equals(value, default (T)))
      {
        if (this._parameters.ContainsKey(key))
          this._parameters.Remove(key);
      }
      else
        this._parameters[key] = value;
      return value;
    }

    public T GetValue<T>(string key)
    {
      if (!this._parameters.ContainsKey(key))
        return default (T);
      return (T) this._parameters[key];
    }

    public void ResetValue(string key)
    {
      if (!this._parameters.ContainsKey(key))
        return;
      this._parameters.Remove(key);
    }

    public SearchParams Copy()
    {
      return (SearchParams) this.MemberwiseClone();
    }
  }
}
