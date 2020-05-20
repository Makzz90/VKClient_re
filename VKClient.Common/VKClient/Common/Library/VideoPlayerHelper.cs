using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Media;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class VideoPlayerHelper
  {
      public static string GetVideoUri(VKClient.Common.Backend.DataObjects.Video video, int resolution, out bool isExternal)
    {
      isExternal = false;
      if (video == null || video.files == null || video.files.Count == 0)
        return "";
      KeyValuePair<string, string> keyValuePair = video.files.Last<KeyValuePair<string, string>>();
      if (resolution > 0)
      {
        foreach (KeyValuePair<string, string> file in video.files)
        {
          if (file.Key.Contains(resolution.ToString()))
          {
            keyValuePair = file;
            break;
          }
        }
      }
      string str = keyValuePair.Value;
      isExternal = keyValuePair.Key == "external";
      if (keyValuePair.Key.StartsWith("flv"))
        return "";
      return str;
    }

      public static bool CanPlayVideo(VKClient.Common.Backend.DataObjects.Video video)
    {
      bool isExternal;
      return VideoPlayerHelper.GetVideoUri(video, 0, out isExternal) != string.Empty;
    }

      public static void PlayVideo(VKClient.Common.Backend.DataObjects.Video video, Action callback, int resolution, StatisticsActionSource actionSource = StatisticsActionSource.news, string videoContext = "")
    {
      if ((video != null ? video.files : (Dictionary<string, string>) null) == null || video.files.Count == 0)
      {
        callback();
        ExtendedMessageBox.ShowSafe(video == null || video.processing != 0 ? CommonResources.ProcessingVideoError : CommonResources.Conversation_VideoIsNotAvailable);
      }
      else
      {
        bool isExternal=false;//
        string uri = VideoPlayerHelper.GetVideoUri(video, resolution, out isExternal);
        if (string.IsNullOrEmpty(uri))
        {
          callback();
          ExtendedMessageBox.ShowSafe(video.processing == 0 ? CommonResources.Conversation_VideoIsNotAvailable : CommonResources.ProcessingVideoError);
        }
        else
        {
          EventAggregator current = EventAggregator.Current;
          VideoPlayEvent videoPlayEvent = new VideoPlayEvent();
          videoPlayEvent.Position = StatisticsVideoPosition.start;
          int num1 = resolution;
          videoPlayEvent.quality = num1;
          string globallyUniqueId = video.GloballyUniqueId;
          videoPlayEvent.id = globallyUniqueId;
          int num2 = (int) actionSource;
          videoPlayEvent.Source = (StatisticsActionSource) num2;
          string str = videoContext;
          videoPlayEvent.Context = str;
          current.Publish((object) videoPlayEvent);
          ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
          {
            Thread.Sleep(500);
            VideoPlayerHelper.PlayVideo(uri, isExternal, resolution, callback);
          }));
        }
      }
    }

    private static void PlayVideo(string uri, bool isExternal, int resolution, Action callback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!isExternal)
        {
          callback();
          VideoPlayerHelper.LaunchMediaPlayer(new Uri(uri, UriKind.Absolute));
        }
        else
        {
          string lowerInvariant = uri.ToLowerInvariant();
          if (lowerInvariant.Contains("youtube"))
            VideoPlayerHelper.PlayYouTube(uri, resolution, callback);
          else if (lowerInvariant.Contains("instagram.com/"))
          {
            callback();
            VideoPlayerHelper.LaunchMediaPlayer(new Uri(uri, UriKind.Absolute));
          }
          else
          {
            callback();
            Navigator.Current.NavigateToWebUri(uri, false, false);
          }
        }
      }));
    }

    private static void PlayYouTube(string uri, int resolution, Action callback)
    {
      YouTubeExtractorAdapter.GetYouTubeVideoUri(uri, resolution, (Action<bool, Uri>) ((success, resultUri) => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!success)
        {
          Navigator.Current.NavigateToWebUri(uri, false, false);
          callback();
        }
        else
          JsonWebRequest.GetHttpStatusCode(resultUri.ToString(), (Action<HttpStatusCode>) (statusCode =>
          {
            if (statusCode == HttpStatusCode.OK)
              VideoPlayerHelper.LaunchMediaPlayer(resultUri);
            else
              Navigator.Current.NavigateToWebUri(uri, false, false);
            callback();
          }));
      }))));
    }

    private static void LaunchMediaPlayer(Uri uri)
    {
      new MediaPlayerLauncher()
      {
        Media = uri,
        Controls = MediaPlaybackControls.All,
        Location = MediaLocationType.Data
      }.Show();
    }
  }
}
