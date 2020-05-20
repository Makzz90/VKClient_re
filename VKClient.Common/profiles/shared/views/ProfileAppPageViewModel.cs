using System;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class ProfileAppPageViewModel : ViewModelStatefulBase
  {
    private readonly long _appId;
    private readonly long _ownerId;
    private readonly string _utmParams;

    public Uri ViewUri { get; private set; }

    public string OriginalUrl { get; private set; }

    public string ScreenTitle { get; private set; }

    public ProfileAppPageViewModel(long appId, long ownerId, string utmParams = "")
    {
      this._appId = appId;
      this._ownerId = ownerId;
      this._utmParams = utmParams;
    }

    public override void Load(Action<ResultCode> callback)
    {
      AppsService.Instance.GetEmbeddedUrl(this._appId, this._ownerId, (Action<BackendResult<EmbeddedUrlResponse, ResultCode>>) (result =>
      {
        ResultCode resultCode = result.ResultCode;
        if (resultCode == ResultCode.Succeeded)
        {
          EmbeddedUrlResponse resultData = result.ResultData;
          string viewUrl = resultData.view_url;
          if (!string.IsNullOrEmpty(viewUrl))
            this.ViewUri = this.AppendUtmParams(viewUrl).ConvertToUri();
          string originalUrl = resultData.original_url;
          if (!string.IsNullOrEmpty(originalUrl))
            this.OriginalUrl = this.AppendUtmParams(originalUrl);
          this.ScreenTitle = resultData.screen_title;
        }
        Action<ResultCode> action = callback;
        if (action == null)
          return;
        int num = (int) resultCode;
        action((ResultCode) num);
      }));
    }

    private string AppendUtmParams(string url)
    {
      if (!string.IsNullOrEmpty(this._utmParams))
      {
        if (!this._utmParams.StartsWith("#"))
          url += url.Contains("?") ? "&" : "?";
        url += this._utmParams;
      }
      return url;
    }

    public void ReportApp()
    {
      ReportContentHelper.ReportApp(this._appId, this._ownerId);
    }

    public void CopyOriginalUrlToClipboard()
    {
      Clipboard.SetText(this.OriginalUrl);
    }
  }
}
