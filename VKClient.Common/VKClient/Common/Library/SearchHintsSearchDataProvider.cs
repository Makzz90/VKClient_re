using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class SearchHintsSearchDataProvider : ISearchDataProvider<SearchHint, SearchHintHeader>
  {
    private static bool _preloadedSearchHintsLoaded;
    private static List<SearchHint> _preloadedSearchHints;
    private static List<SearchHintHeader> _preloadedSearchHintsHeaders;

    public string LocalGroupName
    {
      get
      {
        return "";
      }
    }

    public string GlobalGroupName
    {
      get
      {
        return CommonResources.GlobalSearch.ToUpperInvariant();
      }
    }

    public IEnumerable<SearchHintHeader> LocalItems
    {
      get
      {
        return (IEnumerable<SearchHintHeader>) SearchHintsSearchDataProvider._preloadedSearchHintsHeaders;
      }
    }

    public Func<VKList<SearchHint>, ListWithCount<SearchHintHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<SearchHint>, ListWithCount<SearchHintHeader>>) (res =>
        {
          ListWithCount<SearchHintHeader> listWithCount = new ListWithCount<SearchHintHeader>()
          {
            TotalCount = res.count
          };
          foreach (SearchHint searchHint in res.items)
            listWithCount.List.Add(new SearchHintHeader(searchHint));
          return listWithCount;
        });
      }
    }

    public static void Reset()
    {
      SearchHintsSearchDataProvider._preloadedSearchHintsLoaded = false;
      SearchHintsSearchDataProvider._preloadedSearchHints = (List<SearchHint>) null;
      SearchHintsSearchDataProvider._preloadedSearchHintsHeaders = (List<SearchHintHeader>) null;
    }

    public string GetFooterTextForCount(int count)
    {
      return "";
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<SearchHint>, ResultCode>> callback)
    {
      if (offset == 0 && string.IsNullOrWhiteSpace(searchString) && SearchHintsSearchDataProvider._preloadedSearchHintsLoaded)
      {
        Action<BackendResult<VKList<SearchHint>, ResultCode>> action = callback;
        int num = 0;
        VKList<SearchHint> resultData = new VKList<SearchHint>();
        resultData.items = SearchHintsSearchDataProvider._preloadedSearchHints;
        int count1 = SearchHintsSearchDataProvider._preloadedSearchHints.Count;
        resultData.count = count1;
        BackendResult<VKList<SearchHint>, ResultCode> backendResult = new BackendResult<VKList<SearchHint>, ResultCode>((ResultCode) num, resultData);
        action(backendResult);
      }
      else
      {
        string query = searchString;
        SearchService.Instance.GetSearchHints(searchString, (Action<BackendResult<List<SearchHint>, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            if (string.IsNullOrEmpty(query))
            {
              if (!SearchHintsSearchDataProvider._preloadedSearchHintsLoaded)
              {
                SearchHintsSearchDataProvider._preloadedSearchHints = new List<SearchHint>((IEnumerable<SearchHint>) result.ResultData);
                SearchHintsSearchDataProvider._preloadedSearchHintsHeaders = new List<SearchHintHeader>(result.ResultData.Select<SearchHint, SearchHintHeader>((Func<SearchHint, SearchHintHeader>) (searchHint => new SearchHintHeader(searchHint))));
                SearchHintsSearchDataProvider._preloadedSearchHintsLoaded = true;
              }
            }
            else
            {
              string internalLink = SearchHintsSearchDataProvider.TryGetInternalLink(query);
              if (!string.IsNullOrEmpty(internalLink))
                result.ResultData.Add(new SearchHint()
                {
                  type = "internal_link",
                  global = 1,
                  description = internalLink
                });
              result.ResultData.Add(new SearchHint()
              {
                type = "extended_search",
                global = 1
              });
            }
            callback(new BackendResult<VKList<SearchHint>, ResultCode>(result.ResultCode, new VKList<SearchHint>()
            {
              count = result.ResultData.Count,
              items = result.ResultData
            }));
          }
          else
            callback(new BackendResult<VKList<SearchHint>, ResultCode>(result.ResultCode, (VKList<SearchHint>) null));
        }));
      }
    }

    private static string TryGetInternalLink(string query)
    {
      query = query.ToLowerInvariant();
      bool isVKMe;
      string shortVkUrl = SearchHintsSearchDataProvider.GetShortVKUrl(query, out isVKMe);
      if (shortVkUrl != null)
        query = shortVkUrl.Substring(shortVkUrl.LastIndexOf("/", StringComparison.InvariantCulture) + 1);
      if (!Regex.IsMatch(query, "(?![0-9_]+)^[a-zA-Z0-9_]*$"))
        return "";
      if (!string.IsNullOrEmpty(query))
        query = (!isVKMe ? "vk.com/" : "vk.me/") + query;
      return query;
    }

    private static string GetShortVKUrl(string uri, out bool isVKMe)
    {
      isVKMe = false;
      uri = uri.ToLowerInvariant();
      uri = uri.Replace("http://", "").Replace("https://", "");
      if (uri.StartsWith("m.") || uri.StartsWith("t.") || uri.StartsWith("0."))
        uri = uri.Remove(0, 2);
      if (uri.StartsWith("www.") || uri.StartsWith("new."))
        uri = uri.Remove(0, 4);
      if (uri.StartsWith("vk.com/") || uri.StartsWith("vkontakte.ru/"))
        return uri;
      if (!uri.StartsWith("vk.me/"))
        return null;
      isVKMe = true;
      return uri;
    }
  }
}
