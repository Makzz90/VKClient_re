using System.Collections.Generic;

namespace VKClient.Common.Framework
{
  public class ParametersRepository
  {
    private static Dictionary<string, object> _parametersDict = new Dictionary<string, object>();

    public static void SetParameterForId(string paramId, object parameter)
    {
      ParametersRepository._parametersDict[paramId] = parameter;
    }

    public static object GetParameterForIdAndReset(string paramId)
    {
      if (!ParametersRepository._parametersDict.ContainsKey(paramId))
        return null;
      object obj = ParametersRepository._parametersDict[paramId];
      ParametersRepository._parametersDict.Remove(paramId);
      return obj;
    }

    public static bool Contains(string key)
    {
      return ParametersRepository._parametersDict.ContainsKey(key);
    }

    public static void Clear()
    {
      ParametersRepository._parametersDict.Clear();
    }
  }
}
