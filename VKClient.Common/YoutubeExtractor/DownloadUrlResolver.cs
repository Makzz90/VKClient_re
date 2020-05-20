using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
  public static class DownloadUrlResolver
  {
    private const string RateBypassFlag = "ratebypass";
    private const int CorrectSignatureLength = 81;
    private const string SignatureQuery = "signature";

    public static void DecryptDownloadUrl(VideoInfo videoInfo)
    {
      IDictionary<string, string> queryString = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);
      if (!queryString.ContainsKey("signature"))
        return;
      string signature = queryString["signature"];
      string decipheredSignature;
      try
      {
        decipheredSignature = DownloadUrlResolver.GetDecipheredSignature(videoInfo.HtmlPlayerVersion, signature);
      }
      catch (Exception ex)
      {
        throw new YoutubeParseException("Could not decipher signature", ex);
      }
      videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, "signature", decipheredSignature);
      videoInfo.RequiresDecryption = false;
    }

    public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
    {
      if (videoUrl == null)
        throw new ArgumentNullException("videoUrl");
      if (!DownloadUrlResolver.TryNormalizeYoutubeUrl(videoUrl, out videoUrl))
        throw new ArgumentException("URL is not a valid youtube URL!");
      try
      {
        JObject json = DownloadUrlResolver.LoadJson(videoUrl);
        string videoTitle = DownloadUrlResolver.GetVideoTitle(json);
        IEnumerable<VideoInfo> list = (IEnumerable<VideoInfo>) Enumerable.ToList<VideoInfo>(DownloadUrlResolver.GetVideoInfos(DownloadUrlResolver.ExtractDownloadUrls(json), videoTitle));
        string html5PlayerVersion = DownloadUrlResolver.GetHtml5PlayerVersion(json);
        IEnumerator<VideoInfo> enumerator = list.GetEnumerator();
        try
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            VideoInfo current = enumerator.Current;
            current.HtmlPlayerVersion = html5PlayerVersion;
            if (decryptSignature && current.RequiresDecryption)
              DownloadUrlResolver.DecryptDownloadUrl(current);
          }
        }
        finally
        {
          if (enumerator != null)
            ((IDisposable) enumerator).Dispose();
        }
        return list;
      }
      catch (Exception ex)
      {
        if (ex is WebException || ex is VideoNotAvailableException)
          throw;
        else
          DownloadUrlResolver.ThrowYoutubeParseException(ex, videoUrl);
      }
      return  null;
    }

    public static Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl, bool decryptSignature = true)
    {
        return Task.Run<IEnumerable<VideoInfo>>(() => DownloadUrlResolver.GetDownloadUrls(videoUrl, decryptSignature));
    }

    public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
    {
      url = ((string) url).Trim();
      url = ((string) url).Replace("youtu.be/", "youtube.com/watch?v=");
      url = ((string) url).Replace("www.youtube", "youtube");
      url = ((string) url).Replace("youtube.com/embed/", "youtube.com/watch?v=");
      if (((string) url).Contains("/v/"))
        url = string.Concat("http://youtube.com", ((string) new Uri(url).AbsolutePath).Replace("/v/", "/watch?v="));
      url = ((string) url).Replace("/watch#", "/watch?");
      string str1;
      if (!HttpHelper.ParseQueryString(url).TryGetValue("v", out str1))
      {
        normalizedUrl =  null;
        return false;
      }
      normalizedUrl = string.Concat("http://youtube.com/watch?v=", str1);
      return true;
    }

    private static IEnumerable<DownloadUrlResolver.ExtractionInfo> ExtractDownloadUrls(JObject json)
    {
      string[] strArray = (string[]) Enumerable.ToArray<string>(Enumerable.Concat<string>(((string) DownloadUrlResolver.GetStreamMap(json)).Split((char[]) new char[1]
      {
        ','
      }), ((string) DownloadUrlResolver.GetAdaptiveStreamMap(json)).Split((char[]) new char[1]
      {
        ','
      })));
      for (int index = 0; index < strArray.Length; ++index)
      {
        IDictionary<string, string> queryString = HttpHelper.ParseQueryString(strArray[index]);
        bool flag = false;
        string url;
        if (queryString.ContainsKey("s") || queryString.ContainsKey("sig"))
        {
          flag = queryString.ContainsKey("s");
          string str = queryString.ContainsKey("s") ? queryString["s"] : queryString["sig"];
          url = string.Concat(string.Format("{0}&{1}={2}", queryString["url"], "signature", str), queryString.ContainsKey("fallback_host") ? string.Concat("&fallback_host=", queryString["fallback_host"]) : string.Empty);
        }
        else
          url = queryString["url"];
        string str1 = HttpHelper.UrlDecode(HttpHelper.UrlDecode(url));
        if (!HttpHelper.ParseQueryString(str1).ContainsKey("ratebypass"))
          str1 = string.Concat(str1, string.Format("&{0}={1}", "ratebypass", "yes"));
        DownloadUrlResolver.ExtractionInfo extractionInfo = new DownloadUrlResolver.ExtractionInfo();
        extractionInfo.RequiresDecryption = flag;
        Uri uri = new Uri(str1);
        extractionInfo.Uri = uri;
        yield return extractionInfo;
      }
      strArray = (string[]) null;
    }

    private static string GetAdaptiveStreamMap(JObject json)
    {
      return ((json["args"]["adaptive_fmts"] ?? json["args"]["url_encoded_fmt_stream_map"])).ToString();
    }

    private static string GetDecipheredSignature(string htmlPlayerVersion, string signature)
    {
      if (((string) signature).Length == 81)
        return signature;
      return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
    }

    private static string GetHtml5PlayerVersion(JObject json)
    {
      return new Regex("player-(.+?).js").Match((json["assets"]["js"]).ToString()).Result("$1");
    }

    private static string GetStreamMap(JObject json)
    {
      JToken jtoken = json["args"]["url_encoded_fmt_stream_map"];
      string str = jtoken == null ?  null : (jtoken).ToString();
      if (str == null || ((string) str).Contains("been+removed"))
        throw new VideoNotAvailableException("Video is removed or has an age restriction.");
      return str;
    }

    private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<DownloadUrlResolver.ExtractionInfo> extractionInfos, string videoTitle)
    {
        List<VideoInfo> list = new List<VideoInfo>();
        using (IEnumerator<DownloadUrlResolver.ExtractionInfo> enumerator = extractionInfos.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                DownloadUrlResolver.ExtractionInfo current = enumerator.Current;
                string text = HttpHelper.ParseQueryString(current.Uri.Query)["itag"];
                int formatCode = int.Parse(text);
                VideoInfo videoInfo2 = Enumerable.SingleOrDefault<VideoInfo>(VideoInfo.Defaults, (VideoInfo videoInfo) => videoInfo.FormatCode == formatCode);
                if (videoInfo2 != null)
                {
                    videoInfo2 = new VideoInfo(videoInfo2)
                    {
                        DownloadUrl = current.Uri.ToString(),
                        Title = videoTitle,
                        RequiresDecryption = current.RequiresDecryption
                    };
                }
                else
                {
                    videoInfo2 = new VideoInfo(formatCode)
                    {
                        DownloadUrl = current.Uri.ToString()
                    };
                }
                list.Add(videoInfo2);
            }
        }
        return list;
    }

    private static string GetVideoTitle(JObject json)
    {
      JToken jtoken = json["args"]["title"];
      if (jtoken != null)
        return (jtoken).ToString();
      return string.Empty;
    }

    private static bool IsVideoUnavailable(string pageSource)
    {
      return ((string) pageSource).Contains("<div id=\"watch-player-unavailable\">");
    }

    private static JObject LoadJson(string url)
    {
      string str = HttpHelper.DownloadString(url);
      if (DownloadUrlResolver.IsVideoUnavailable(str))
        throw new VideoNotAvailableException();
      return JObject.Parse(new Regex("ytplayer\\.config\\s*=\\s*(\\{.+?\\});", RegexOptions.Multiline).Match(str).Result("$1"));
    }

    private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
    {
      throw new YoutubeParseException(string.Concat("Could not parse the Youtube page for URL ", videoUrl, "\nThis may be due to a change of the Youtube page structure.\nPlease report this bug at www.github.com/flagbug/YoutubeExtractor/issues"), innerException);
    }

    private class ExtractionInfo
    {
      public bool RequiresDecryption { get; set; }

      public Uri Uri { get; set; }
    }
  }
}
