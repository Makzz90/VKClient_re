using System.IO.IsolatedStorage;

namespace Microsoft.Phone.Applications.Common
{
  public static class ApplicationSettingHelper
  {
    public static TValue TryGetValueWithDefault<TValue>(string key, TValue defaultValue)
    {
      TValue obj1 = defaultValue;
      if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
      {
        object obj2 = IsolatedStorageSettings.ApplicationSettings[key];
        if (obj2 is TValue)
          obj1 = (TValue) obj2;
      }
      return obj1;
    }

    public static bool AddOrUpdateValue(string key, object value)
    {
      bool flag = false;
      if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
      {
        if (IsolatedStorageSettings.ApplicationSettings[key] != value)
        {
          IsolatedStorageSettings.ApplicationSettings[key]= value;
          flag = true;
        }
      }
      else
      {
        IsolatedStorageSettings.ApplicationSettings.Add(key, value);
        flag = true;
      }
      return flag;
    }

    public static void Save()
    {
      IsolatedStorageSettings.ApplicationSettings.Save();
    }
  }
}
