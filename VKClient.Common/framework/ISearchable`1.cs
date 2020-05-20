using System;
using System.Collections.Generic;

namespace VKClient.Common.Framework
{
  public interface ISearchable<T>
  {
    bool SupportsLocalSearch { get; }

    string LocalGroupName { get; }

    string GlobalGroupName { get; }

    List<T> SearchLocally(string searchStr, Dictionary<string, string> searchParams);

    void SearchGlobally(string searchStr, Dictionary<string, string> searchParams, Action<bool, List<T>> searchCallback);

    string GetFooterTextForCount(int count);
  }
}
