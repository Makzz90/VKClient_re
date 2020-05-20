using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public static class AudioTrackHelper
  {
    private static DateTime _lastTimeBroadcastAttempt = DateTime.MinValue;

    public static AudioTrack CreateTrack(AudioObj audio)
    {
      if (audio == null)
        return  null;
      string uriString = string.IsNullOrEmpty(audio.url) ? "https://vk.com/404.mp3" : audio.url;
      string localFileForUniqueId = AudioCacheManager.Instance.GetLocalFileForUniqueId(audio.UniqueId);
      if (localFileForUniqueId != null)
        uriString = CacheManager.GetFilePath(localFileForUniqueId, CacheManager.DataType.CachedData, "/");
      return new AudioTrack(new Uri(uriString, UriKind.RelativeOrAbsolute), audio.title ?? "NOTITLE", audio.artist ?? "NOARTIST", "",  null, audio.UniqueId + "|" + audio.duration, (EnabledPlayerControls) 31L);
    }

    public static void EnsureStopPlayingOnLogout()
    {
      try
      {
        if (!PlaylistManager.HaveAssignedTrack())
          return;
        BGAudioPlayerWrapper.Instance.Stop();
        BGAudioPlayerWrapper.Instance.Track =  null;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Failed to stop playing. ", ex);
      }
    }

    public static void PlayCurrentTrack(BackgroundAudioPlayer player, Action<bool> resultCallback = null, bool justPlay = false)
    {
      bool flag = false;
      try
      {
        player.Volume = 1.0;
        AudioTrack track = player.Track;
        if (track != null)
        {
          try
          {
            player.Play();
          }
          catch (Exception ex)
          {
            if (resultCallback == null)
              return;
            resultCallback(false);
            return;
          }
          if (justPlay)
          {
            resultCallback(true);
            return;
          }
        }
        if (track != null && track.Source !=  null)
        {
          //track.Source;
          AudioTrackHelper.EnsureAppGlobalStateInitialized();
          if (track.Source.OriginalString.StartsWith("http") && track.GetTagOwnerId() == AppGlobalStateManager.Current.LoggedInUserId && AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
          {
            PlayedFilesInfo playedFilesInfo = new PlayedFilesInfo();
            playedFilesInfo.Restore();
            Uri source = track.Source;
            string tag = track.Tag;
            playedFilesInfo.Add(source, tag);
          }
          Playlist playlist = PlaylistManager.LoadTracksFromIsolatedStorage(true);
          AccountService.Instance.StatsTrackEvents(StatsEventsTracker.ConvertToAppEvents((IEnumerable<object>) new List<AudioPlayEvent>()
          {
            new AudioPlayEvent()
            {
              OwnerAndAudioId = track.Tag,
              Source = playlist.Metadata.ActionSource
            }
          }), (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (result => {}));
          if (resultCallback == null)
            return;
          flag = true;
          resultCallback(true);
        }
        else
        {
          if (resultCallback == null)
            return;
          flag = true;
          resultCallback(true);
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Failed to start playing. ", ex);
        if (flag || resultCallback == null)
          return;
        resultCallback(false);
      }
    }

    public static void RefetchTrack(AudioTrack track, Action<AudioTrack> callback)
    {
      if (track != null && track.Source !=  null)
      {
        Uri currentSource = track.Source;
        AudioTrackHelper.EnsureAppGlobalStateInitialized();
        AudioService instance = AudioService.Instance;
        List<string> ids = new List<string>();
        ids.Add(track.GetTagId());
        Action<BackendResult<List<AudioObj>, ResultCode>> callback1 = (Action<BackendResult<List<AudioObj>, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded && res.ResultData != null && (res.ResultData.Count > 0 && currentSource.OriginalString != res.ResultData[0].url))
            callback(AudioTrackHelper.CreateTrack(res.ResultData[0]));
          else
            callback( null);
        });
        instance.GetAudio(ids, callback1);
      }
      else
        callback( null);
    }

    public static void EnsureAppGlobalStateInitialized()
    {
      if (AppGlobalStateManager.Current.IsInitialized)
        return;
      AppGlobalStateManager.Current.Initialize(false);
    }

    public static void BroadcastTrackIfNeeded(BackgroundAudioPlayer player, List<AudioObj> playlist = null, PlaybackSettings settings = null, bool allowCache = false, bool bypassChecks = false)
    {
      try
      {
          if (!bypassChecks && (player.PlayerState != Microsoft.Phone.BackgroundAudio.PlayState.Playing || (DateTime.Now - AudioTrackHelper._lastTimeBroadcastAttempt).TotalSeconds < 3.0))
          return;
        AudioTrackHelper._lastTimeBroadcastAttempt = DateTime.Now;
        if (settings == null)
        {
          settings = PlaylistManager.ReadPlaybackSettings(allowCache);
          if (!settings.Broadcast)
            return;
        }
        AudioTrack track = player.Track;
        if (track == null || track.GetTagId() == null)
          return;
        AudioTrackHelper.EnsureAppGlobalStateInitialized();
        AudioService.Instance.StatusSet("", track.GetTagId(), (Action<BackendResult<long, ResultCode>>) (res => {}));
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Broadcast track failed", ex);
      }
    }

    public static AudioTrack GetNextTrack(BackgroundAudioPlayer player, bool next, out bool startedNewCycle, PlaybackSettings settings = null, bool allowCache = false)
    {
      startedNewCycle = false;
      return AudioTrackHelper.CreateTrack(AudioTrackHelper.GetNextAudio(player, next, out startedNewCycle, settings, allowCache));
    }

    public static AudioObj GetNextAudio(BackgroundAudioPlayer player, bool next, out bool startedNewCycle, PlaybackSettings settings = null, bool allowCache = false)
    {
      startedNewCycle = false;
      Playlist playlist = PlaylistManager.LoadTracksFromIsolatedStorage(allowCache);
      List<AudioObj> tracks = playlist.Tracks;
      if (tracks.Count == 1)
        startedNewCycle = true;
      if (settings == null)
        settings = PlaylistManager.ReadPlaybackSettings(allowCache);
      tracks.FirstOrDefault<AudioObj>();
      int num1 = 0;
      AudioTrack track = player.Track;
      if (track != null)
      {
        AudioObj audioObj = tracks.FirstOrDefault<AudioObj>((Func<AudioObj, bool>) (t => t.UniqueId == track.GetTagId()));
        num1 = tracks.IndexOf(audioObj);
        if (settings.Shuffle)
          num1 = playlist.ShuffledIndexes.IndexOf(num1);
      }
      int num2 = num1;
      int index = !next ? num2 - 1 : num2 + 1;
      if (index < 0)
      {
        startedNewCycle = true;
        index = tracks.Count<AudioObj>() - 1;
      }
      if (index >= tracks.Count)
      {
        startedNewCycle = true;
        index = 0;
      }
      if (tracks.Count <= 0 || index >= tracks.Count)
        return  null;
      if (settings.Shuffle)
        index = playlist.ShuffledIndexes[index];
      return tracks[index];
    }

    public static TimeSpan GetPositionSafe()
    {
      try
      {
        return BGAudioPlayerWrapper.Instance.Position;
      }
      catch (Exception )
      {
      }
      return new TimeSpan();
    }
  }
}
