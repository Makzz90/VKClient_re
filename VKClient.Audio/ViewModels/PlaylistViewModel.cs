using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
  public class PlaylistViewModel : ViewModelBase, ICollectionDataProvider<List<AudioObj>, AudioHeader>
  {
    private GenericCollectionViewModel<List<AudioObj>, AudioHeader> _audios;

    public GenericCollectionViewModel<List<AudioObj>, AudioHeader> Audios
    {
      get
      {
        return this._audios;
      }
    }

    public bool Shuffle { get; set; }

    public Func<List<AudioObj>, ListWithCount<AudioHeader>> ConverterFunc
    {
      get
      {
        return (Func<List<AudioObj>, ListWithCount<AudioHeader>>) (list =>
        {
          ListWithCount<AudioHeader> listWithCount = new ListWithCount<AudioHeader>();
          listWithCount.TotalCount = list.Count;
          List<AudioHeader> audioHeaderList = new List<AudioHeader>((IEnumerable<AudioHeader>)Enumerable.Select<AudioObj, AudioHeader>(list, (Func<AudioObj, AudioHeader>)(i => new AudioHeader(i, 0L))));
          listWithCount.List = audioHeaderList;
          return listWithCount;
        });
      }
    }

    public PlaylistViewModel()
    {
      this._audios = new GenericCollectionViewModel<List<AudioObj>, AudioHeader>((ICollectionDataProvider<List<AudioObj>, AudioHeader>) this);
      EventAggregator.Current.Subscribe(this);
    }

    public void GetData(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int offset, int count, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
    {
      Playlist playlist = PlaylistManager.LoadTracksFromIsolatedStorage(true);
      if (playlist.Tracks == null)
        playlist.Tracks=(new List<AudioObj>());
      List<AudioObj> resultData = playlist.Tracks;
      if (this.Shuffle)
      {
        resultData = new List<AudioObj>();
        List<int>.Enumerator enumerator = playlist.ShuffledIndexes.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            int current = enumerator.Current;
            resultData.Add(playlist.Tracks[current]);
          }
        }
        finally
        {
          enumerator.Dispose();
        }
      }
      callback(new BackendResult<List<AudioObj>, ResultCode>(ResultCode.Succeeded, resultData));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int count)
    {
      if (count <= 0)
        return AudioResources.NoTracks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true,  null, false);
    }
  }
}
