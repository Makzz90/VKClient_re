using System.Collections.Generic;

namespace VKClient.Common.Utils
{
  public static class ListExtensions2
  {
    public static List<T> Sublist<T>(this List<T> list, int begin, int end)
    {
      List<T> objList = new List<T>();
      for (int index = begin; index < end; ++index)
        objList.Add(list[index]);
      return objList;
    }
  }
}
