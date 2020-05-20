using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace VKClient.Common.Library.Media
{
  public static class YouTubeExtractorAdapter
  {
    public static void GetYouTubeVideoUri(string uri, int preferredQuality, Action<bool, Uri> resultCallback)
    {
      if (preferredQuality <= 0)
        preferredQuality = 480;
      DownloadUrlResolver.GetDownloadUrlsAsync(uri, false).ContinueWith((Action<Task<IEnumerable<VideoInfo>>>) (task =>
      {
        if (task.Status != TaskStatus.RanToCompletion || task.Result == null || !task.Result.Any<VideoInfo>())
        {
          resultCallback(false, (Uri) null);
        }
        else
        {
          VideoInfo videoInfo = task.Result.Where<VideoInfo>((Func<VideoInfo, bool>) (video =>
          {
            if (video.Resolution < preferredQuality)
              return false;
            if (video.VideoType != VideoType.Mp4)
              return video.VideoType == VideoType.Mobile;
            return true;
          })).OrderBy<VideoInfo, int>((Func<VideoInfo, int>) (video => video.Resolution)).Concat<VideoInfo>((IEnumerable<VideoInfo>) task.Result.Where<VideoInfo>((Func<VideoInfo, bool>) (video =>
          {
            if (video.Resolution >= preferredQuality)
              return false;
            if (video.VideoType != VideoType.Mp4)
              return video.VideoType == VideoType.Mobile;
            return true;
          })).OrderBy<VideoInfo, int>((Func<VideoInfo, int>) (video => -video.Resolution))).FirstOrDefault<VideoInfo>();
          if (videoInfo == null)
            resultCallback(false, (Uri) null);
          else if (videoInfo.RequiresDecryption)
          {
            try
            {
              DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
              resultCallback(true, new Uri(videoInfo.DownloadUrl));
            }
            catch
            {
              resultCallback(false, null);
            }
          }
          else
            resultCallback(true, new Uri(videoInfo.DownloadUrl));
        }
      }));
    }
  }
}
