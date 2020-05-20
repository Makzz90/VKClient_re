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
        if (task.Status != TaskStatus.RanToCompletion || task.Result == null || !Enumerable.Any<VideoInfo>(task.Result))
        {
          resultCallback(false,  null);
        }
        else
        {
            VideoInfo videoInfo = (VideoInfo)Enumerable.FirstOrDefault<VideoInfo>(Enumerable.Concat<VideoInfo>(Enumerable.OrderBy<VideoInfo, int>(Enumerable.Where<VideoInfo>(task.Result, (Func<VideoInfo, bool>)(video =>
          {
            if (video.Resolution < preferredQuality)
              return false;
            if (video.VideoType != VideoType.Mp4)
              return video.VideoType == VideoType.Mobile;
            return true;
          })), (Func<VideoInfo, int>) (video => video.Resolution)), Enumerable.OrderBy<VideoInfo, int>(Enumerable.Where<VideoInfo>(task.Result, (Func<VideoInfo, bool>) (video =>
          {
            if (video.Resolution >= preferredQuality)
              return false;
            if (video.VideoType != VideoType.Mp4)
              return video.VideoType == VideoType.Mobile;
            return true;
          })), (Func<VideoInfo, int>)(video => -video.Resolution))));
          if (videoInfo == null)
            resultCallback(false,  null);
          else if (videoInfo.RequiresDecryption)
          {
            try
            {
              DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
              resultCallback(true, new Uri(videoInfo.DownloadUrl));
            }
            catch (Exception )
            {
              resultCallback(false,  null);
            }
          }
          else
            resultCallback(true, new Uri(videoInfo.DownloadUrl));
        }
      }));
    }
  }
}
