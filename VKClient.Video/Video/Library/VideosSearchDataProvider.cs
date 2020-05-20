using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Video.Library
{
  public class VideosSearchDataProvider : ISearchDataProvider<VKClient.Common.Backend.DataObjects.Video, VideoHeader>
  {
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

    public IEnumerable<VideoHeader> LocalItems { get; private set; }

    public Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>> ConverterFunc
    {
        get
        {
            return (Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>>)(res =>
            {
                ListWithCount<VideoHeader> listWithCount = new ListWithCount<VideoHeader>()
                {
                    TotalCount = res.count
                };
                foreach (VKClient.Common.Backend.DataObjects.Video video in res.items)
                    listWithCount.List.Add(new VideoHeader(video, (List<MenuItemData>)null, res.profiles, res.groups, StatisticsActionSource.search, "", false, 0, 0L)
                    {
                        FromSearch = true
                    });
                return listWithCount;
            });
        }
    }

    public VideosSearchDataProvider(IEnumerable<VideoHeader> localItems)
    {
      this.LocalItems = localItems;
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true,  null, false);
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
      VideoService.Instance.SearchVideo(searchString, parameters, offset, count, callback);
    }
  }
}
