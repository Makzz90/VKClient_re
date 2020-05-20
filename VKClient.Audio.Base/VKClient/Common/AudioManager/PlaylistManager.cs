using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.AudioManager
{
  public class PlaylistManager
  {
    public static readonly string PlaylistFileName = "currentPlaylist";
    public static readonly string PlaybackSettingsFileName = "PlaybackSettings";
    public static readonly string PlaylistMetadataFileName = "playlistMetadata";
    public static readonly string SettingsMetadataFileName = "SettingsMetadata";
    public static readonly string BackupPlaylistFileName = "backupPlaylist";
    public static readonly string PlaylistMutexName = "playlistLockMutex";
    private static Mutex playlistAccessMutex = new Mutex(false, PlaylistManager.PlaylistMutexName);
    private static Mutex settingsAccessMutex = new Mutex(false, "SettingsMutex");
    private static Mutex metadataAccessMutex = new Mutex(false, "metadataMutex");
    private static object _lockObj = new object();
    private static PlaybackSettings _settingsCached;
    private static Playlist _playlistCached;

    private static DateTime GetLastChangedDate(string fileName)
    {
      try
      {
        PlaylistManager.metadataAccessMutex.WaitOne();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          if (!storeForApplication.FileExists(fileName))
            return DateTime.MinValue;
          using (BinaryReader reader = new BinaryReader((Stream) storeForApplication.OpenFile(fileName, FileMode.Open, FileAccess.Read)))
            return reader.ReadGeneric<Metadata>().LastUpdated;
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("PlaylistManager.GetLastChangedDate failed", ex);
        return DateTime.MinValue;
      }
      finally
      {
        PlaylistManager.metadataAccessMutex.ReleaseMutex();
      }
    }

    public static void Initialize()
    {
    }

    public static bool HaveAssignedTrack()
    {
      try
      {
        return BGAudioPlayerWrapper.Instance.Track != null;
      }
      catch (Exception )
      {
      }
      return false;
    }

    public static PlaybackSettings ReadPlaybackSettings(bool allowCached = false)
    {
      lock (PlaylistManager._lockObj)
      {
        if (allowCached && PlaylistManager._settingsCached != null)
          return PlaylistManager._settingsCached;
        PlaybackSettings playbackSettings = new PlaybackSettings();
        try
        {
          if (PlaylistManager._settingsCached != null && PlaylistManager._settingsCached.Metadata.LastUpdated == PlaylistManager.GetLastChangedDate(PlaylistManager.SettingsMetadataFileName))
            return PlaylistManager._settingsCached;
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            PlaylistManager.settingsAccessMutex.WaitOne();
            if (storeForApplication.FileExists(PlaylistManager.PlaybackSettingsFileName))
            {
              IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(PlaylistManager.PlaybackSettingsFileName, FileMode.Open, FileAccess.Read);
              if (storageFileStream != null)
              {
                using (BinaryReader reader = new BinaryReader((Stream) storageFileStream))
                  playbackSettings = reader.ReadGeneric<PlaybackSettings>();
                PlaylistManager._settingsCached = playbackSettings;
              }
            }
            PlaylistManager.settingsAccessMutex.ReleaseMutex();
          }
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("ReadPlaybackSettings failed", ex);
        }
        return playbackSettings;
      }
    }

    public static void WritePlaybackSettings(PlaybackSettings settings)
    {
      settings.Metadata = new Metadata()
      {
        LastUpdated = DateTime.Now
      };
      lock (PlaylistManager._lockObj)
      {
        try
        {
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            PlaylistManager.metadataAccessMutex.WaitOne();
            using (BinaryWriter writer = new BinaryWriter((Stream) storeForApplication.CreateFile(PlaylistManager.SettingsMetadataFileName)))
              writer.Write<Metadata>(settings.Metadata, false);
            PlaylistManager.metadataAccessMutex.ReleaseMutex();
            PlaylistManager.settingsAccessMutex.WaitOne();
            using (BinaryWriter writer = new BinaryWriter((Stream) storeForApplication.CreateFile(PlaylistManager.PlaybackSettingsFileName)))
              writer.Write<PlaybackSettings>(settings, false);
            PlaylistManager.settingsAccessMutex.ReleaseMutex();
            PlaylistManager._settingsCached = settings;
          }
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("WritePlaybackSettings failed", ex);
        }
      }
    }

    public static Playlist LoadTracksFromIsolatedStorage(bool allowCached = false)
    {
      Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage, allowCached = {0}", allowCached);
      lock (PlaylistManager._lockObj)
      {
        try
        {
          if (allowCached && PlaylistManager._playlistCached != null)
          {
            Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: returning data from 1st level cache");
            return PlaylistManager._playlistCached;
          }
          Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: _playlistCached is not null? = {0}" + (PlaylistManager._playlistCached != null).ToString());
          if (PlaylistManager._playlistCached != null)
          {
            Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: _playlistCached.Metadata.LastUpdated = {0}", PlaylistManager._playlistCached.Metadata.LastUpdated);
            DateTime lastChangedDate = PlaylistManager.GetLastChangedDate(PlaylistManager.PlaylistMetadataFileName);
            Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: Storemetadata.LastUpdated = {0}", lastChangedDate);
            if (PlaylistManager._playlistCached.Metadata.LastUpdated == lastChangedDate)
            {
              Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: returning data from 2nd level cache");
              return PlaylistManager._playlistCached;
            }
          }
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            try
            {
              PlaylistManager.playlistAccessMutex.WaitOne();
              if (!storeForApplication.FileExists(PlaylistManager.PlaylistFileName))
                return new Playlist();
              IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(PlaylistManager.PlaylistFileName, FileMode.Open, FileAccess.Read);
              if (storageFileStream == null)
                return new Playlist();
              using (BinaryReader reader = new BinaryReader((Stream) storageFileStream))
                return PlaylistManager._playlistCached = reader.ReadGeneric<Playlist>();
            }
            finally
            {
              PlaylistManager.playlistAccessMutex.ReleaseMutex();
            }
          }
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("Failed to read playlist", ex);
        }
        return new Playlist();
      }
    }

    public static void SetAudioAgentPlaylist(List<AudioObj> tracks, StatisticsActionSource actionSource)
    {
      PlaylistManager.SetAudioAgentPlaylistImpl(tracks, actionSource);
    }

    private static void SetAudioAgentPlaylistImpl(List<AudioObj> tracks, StatisticsActionSource actionSource)
    {
      if (tracks == null)
        return;
      tracks = tracks.Where<AudioObj>((Func<AudioObj, bool>) (track => track.content_restricted == 0)).ToList<AudioObj>();
      if (tracks.Count == 0)
        return;
      


        //IEnumerable<AudioObj> arg_59_0 = tracks;
        //Func<AudioObj, string> arg_59_1 = new Func<AudioObj, string>(a => a.UniqueId);
        //tracks = Enumerable.ToList<AudioObj>(arg_59_0.Distinct(arg_59_1));


//      tracks = tracks.Distinct<AudioObj>((Func<AudioObj, string>) (a => a.UniqueId)).ToList<AudioObj>();//todo: bug
      Playlist playlist1 = new Playlist();
      playlist1.Metadata = new Metadata()
      {
        LastUpdated = DateTime.Now,
        ActionSource = actionSource
      };
      List<AudioObj> audioObjList = tracks;
      playlist1.Tracks = audioObjList;
      Playlist playlist2 = playlist1;
      lock (PlaylistManager._lockObj)
      {
        try
        {
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            PlaylistManager.metadataAccessMutex.WaitOne();
            using (BinaryWriter writer = new BinaryWriter((Stream) storeForApplication.CreateFile(PlaylistManager.PlaylistMetadataFileName)))
              writer.Write<Metadata>(playlist2.Metadata, false);
            PlaylistManager.metadataAccessMutex.ReleaseMutex();
            PlaylistManager.playlistAccessMutex.WaitOne();
            using (BinaryWriter writer = new BinaryWriter((Stream) storeForApplication.CreateFile(PlaylistManager.PlaylistFileName)))
              writer.Write<Playlist>(playlist2, false);
            PlaylistManager.playlistAccessMutex.ReleaseMutex();
            PlaylistManager._playlistCached = playlist2;
          }
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("Failed to set playlist", ex);
        }
      }
    }
  }
}
