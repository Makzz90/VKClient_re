using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
  public class AudioSearchDataProvider : ISearchDataProvider<AudioObj, AudioHeader>
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

    public IEnumerable<AudioHeader> LocalItems { get; private set; }

    public Func<VKList<AudioObj>, ListWithCount<AudioHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<AudioObj>, ListWithCount<AudioHeader>>) (res =>
        {
          ListWithCount<AudioHeader> listWithCount = new ListWithCount<AudioHeader>()
          {
            TotalCount = res.count
          };
          foreach (AudioObj track in res.items)
            listWithCount.List.Add(new AudioHeader(track, 0L));
          return listWithCount;
        });
      }
    }

    public AudioSearchDataProvider(IEnumerable<AudioHeader> localItems)
    {
      this.LocalItems = localItems;
    }

    public string GetFooterTextForCount(int count)
    {
      if (count <= 0)
        return AudioResources.NoTracks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true,  null, false);
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<AudioObj>, ResultCode>> callback)
    {
      AudioService.Instance.SearchTracks(searchString, offset, count, callback);
    }
  }
}
