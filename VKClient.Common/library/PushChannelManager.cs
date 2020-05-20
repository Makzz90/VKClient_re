using Microsoft.Phone.Notification;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PushChannelManager
  {
    private static PushChannelManager _instance;
    private int _retryTries;

    public static PushChannelManager Instance
    {
      get
      {
        if (PushChannelManager._instance == null)
          PushChannelManager._instance = new PushChannelManager();
        return PushChannelManager._instance;
      }
    }

    public void OpenChannel()
    {
      HttpNotificationChannel notificationChannel1 = this.TryFindChannel();
      if (notificationChannel1 != null && notificationChannel1.ChannelUri ==  null)
      {
        notificationChannel1.Close();
        notificationChannel1 =  null;
      }
      if (notificationChannel1 == null)
      {
        HttpNotificationChannel notificationChannel2 = new HttpNotificationChannel(VKConstants.HttpPushNotificationName, "push.vk.com");
        notificationChannel2.ChannelUriUpdated += (new EventHandler<NotificationChannelUriEventArgs>(this.PushChannel_ChannelUriUpdated));
        notificationChannel2.ErrorOccurred += (new EventHandler<NotificationChannelErrorEventArgs>(this.PushChannel_ErrorOccurred));
        notificationChannel2.ShellToastNotificationReceived += (new EventHandler<NotificationEventArgs>(this.pushChannel_ShellToastNotificationReceived));
        notificationChannel2.Open();
        notificationChannel2.BindToShellToast();
        notificationChannel2.BindToShellTile();
      }
      else
      {
        notificationChannel1.ChannelUriUpdated += (new EventHandler<NotificationChannelUriEventArgs>(this.PushChannel_ChannelUriUpdated));
        notificationChannel1.ErrorOccurred += (new EventHandler<NotificationChannelErrorEventArgs>(this.PushChannel_ErrorOccurred));
        notificationChannel1.ShellToastNotificationReceived += (new EventHandler<NotificationEventArgs>(this.pushChannel_ShellToastNotificationReceived));
        if (!(notificationChannel1.ChannelUri !=  null))
          return;
        this.FireChannelUriUpdatedEvent(notificationChannel1.ChannelUri);
      }
    }

    public void CloseChannel()
    {
      HttpNotificationChannel channel = this.TryFindChannel();
      if (channel == null)
        return;
      try
      {
        channel.UnbindToShellToast();
        channel.UnbindToShellTile();
        channel.Close();
      }
      catch (Exception )
      {
        Logger.Instance.Error("Failed to close channel");
      }
    }

    private void pushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
    {
      foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) e.Collection)
        ;
      IDictionary<string, string> args = e.Collection;
      if (!args.ContainsKey("wp:Param"))
        return;
      Dictionary<string, string> parameters = args["wp:Param"].ParseQueryString();
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        Action tapCallback =  null;
        if (parameters.ContainsKey("device_token") && parameters.ContainsKey("url"))
          tapCallback = (Action) (() => Navigator.Current.NavigateToWebViewPage(parameters["url"], false));
        if (parameters.ContainsKey("confirm_hash"))
          tapCallback = (Action) (() => VKRequestsDispatcher.DispatchRequestToVK<object>("account.validateAction", new Dictionary<string, string>()
          {
            {
              "confirm",
              MessageBox.Show(args["wp:Text2"], CommonResources.VK, (MessageBoxButton) 1) == MessageBoxResult.OK ? "1" : "0"
            },
            {
              "hash",
              parameters["confirm_hash"]
            }
          }, (Action<BackendResult<object, ResultCode>>) (f => {}),  null, false, true, new CancellationToken?(),  null));
        if (tapCallback == null)
          return;
        AppNotificationUC.Instance.ShowAndHideLater(MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/NotificationVK.png", true, ""), CommonResources.VK, args["wp:Text2"], tapCallback,  null);
      }));
    }

    private void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
    {
      this.FireChannelUriUpdatedEvent(e.ChannelUri);
    }

    private void FireChannelUriUpdatedEvent(Uri uri)
    {
      EventAggregator.Current.Publish(new ChannelUriUpdatedEvent()
      {
        ChannelUri = uri
      });
    }

    private void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
    {
      Logger.Instance.Error(string.Format("A push notification {0} error occurred.  {1} ({2}) {3}", e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData));
      if (e.ErrorType == ChannelErrorType.ChannelOpenFailed && this._retryTries <= 3)
      {
        this._retryTries = this._retryTries + 1;
        this.OpenChannel();
      }
      if (e.ErrorType != ChannelErrorType.PayloadFormatError || this._retryTries > 3)
        return;
      this._retryTries = this._retryTries + 1;
      this.OpenChannel();
    }

    private HttpNotificationChannel TryFindChannel()
    {
      try
      {
        return HttpNotificationChannel.Find(VKConstants.HttpPushNotificationName);
      }
      catch (Exception )
      {
        return  null;
      }
    }
  }
}
