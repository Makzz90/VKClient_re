using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class InstantUpdatesManager
  {
    private DelayedExecutor _de = new DelayedExecutor(1000);
    private static InstantUpdatesManager _current;

    public static InstantUpdatesManager Current
    {
      get
      {
        if (InstantUpdatesManager._current == null)
          InstantUpdatesManager._current = new InstantUpdatesManager();
        return InstantUpdatesManager._current;
      }
    }

    public event InstantUpdatesManager.UpdatesReceivedEventHandler ReceivedUpdates;

    public void Restart()
    {
      Logger.Instance.Info("InstantUpdatesManager.Restart");
      if (!BackendServices.LongPollServerService.HaveToken())
      {
        Logger.Instance.Info("InstantUpdatesManager.Restart: no token is currently available");
      }
      else
      {
        string t = VKRequestsDispatcher.Token;
        BackendServices.LongPollServerService.GetLongPollServer((Action<BackendResult<LongPollServerResponse, ResultCode>>) (result =>
        {
          Logger.Instance.Info("InstantUpdatesManager.Restart callback result = {0}", result.ResultCode);
          if (result.ResultCode == ResultCode.Succeeded)
            this.RunRequestsLoop(t, result.ResultData);
          else
            this._de.AddToDelayedExecution((Action) (() => this.Restart()));
        }));
      }
    }

    private void RunRequestsLoop(string token, LongPollServerResponse requestsSettings)
    {
      try
      {
        LongPollServerService.Instance.GetUpdates(requestsSettings.server, requestsSettings.key, requestsSettings.ts, (Action<BackendResult<UpdatesResponse, LongPollResultCode>>) (res =>
        {
          Logger.Instance.Info("InstantUpdatesManager.RunRequestsLoop callback result = {0}", res.ResultCode);
          if (VKRequestsDispatcher.Token != token)
            return;
          if (res.ResultCode == LongPollResultCode.RequireNewPollServer)
            this.Restart();
          if (res.ResultCode == LongPollResultCode.Succeeded)
          {
            this.LogReceivedUpdates(res.ResultData);
            this.ProcessReceivedData(res.ResultData.Updates);
            AppGlobalStateManager.Current.GlobalState.LastTS = res.ResultData.ts;
            requestsSettings.ts = res.ResultData.ts;
            this.RunRequestsLoop(token, requestsSettings);
          }
          if (res.ResultCode != LongPollResultCode.CommunicationFailed)
            return;
          this.Restart();
        }));
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("RunRequestLoop failed", ex);
        this.Restart();
      }
    }

    private void ProcessReceivedData(List<LongPollServerUpdateData> updateData)
    {
      this.EnrichUpdateData(updateData, (Action) (() =>
      {
        try
        {
          // ISSUE: reference to a compiler-generated field
          InstantUpdatesManager.UpdatesReceivedEventHandler receivedUpdates = this.ReceivedUpdates;
          if (receivedUpdates == null)
            return;
          List<LongPollServerUpdateData> updates = updateData;
          receivedUpdates(updates);
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("InstantUpdatesManager ReceivedUpdates", ex);
        }
      }));
    }

    private void EnrichUpdateData(List<LongPollServerUpdateData> updateData, Action callback)
    {
      List<long> list = updateData.Where<LongPollServerUpdateData>((Func<LongPollServerUpdateData, bool>) (u =>
      {
        if (u.UpdateType != LongPollServerUpdateType.MessageHasBeenAdded || !u.hasAttachOrForward && !u.isChat || u.message != null)
          return u.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored;
        return true;
      })).Select<LongPollServerUpdateData, long>((Func<LongPollServerUpdateData, long>) (u => u.message_id)).ToList<long>();
      List<int> associatedUsersIds = updateData.Where<LongPollServerUpdateData>((Func<LongPollServerUpdateData, bool>) (u =>
      {
        if (u.UpdateType != LongPollServerUpdateType.MessageHasBeenAdded)
          return u.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored;
        return true;
      })).Select<LongPollServerUpdateData, int>((Func<LongPollServerUpdateData, int>) (u => (int) u.user_id)).Distinct<int>().ToList<int>();
      BackendServices.MessagesService.GetMessages(list, (Action<BackendResult<MessageListResponse, ResultCode>>) (resMessages =>
      {
        if (resMessages.ResultCode != ResultCode.Succeeded)
          return;
        associatedUsersIds.AddRange((IEnumerable<int>) Message.GetAssociatedUserIds(resMessages.ResultData.Messages, true).Select<long, int>((Func<long, int>) (l => (int) l)).ToList<int>());
        associatedUsersIds = associatedUsersIds.Distinct<int>().ToList<int>();
        UsersService.Instance.GetUsers(associatedUsersIds.Select<int, long>((Func<int, long>) (uid => (long) uid)).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>) (resUsers =>
        {
          if (resMessages.ResultCode != ResultCode.Succeeded || resUsers.ResultCode != ResultCode.Succeeded)
            return;
          updateData.ForEach((Action<LongPollServerUpdateData>) (d =>
          {
            Message message = resMessages.ResultData.Messages.FirstOrDefault<Message>((Func<Message, bool>) (m => (long) m.mid == d.message_id));
            if (message != null && d.user_id == 0L)
              d.user_id = (long) message.uid;
            d.user = resUsers.ResultData.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == d.user_id));
            if (d.message == null)
            {
              if (message == null)
                d.message = new Message()
                {
                  uid = (int) d.user_id,
                  body = d.text,
                  read_state = 0,
                  mid = (int) d.message_id,
                  date = (int) d.timestamp,
                  @out = d.@out ? 1 : 0,
                  chat_id = (int) d.chat_id
                };
              else
                d.message = message;
            }
            AppGlobalStateManager.Current.GlobalState.MaxMessageId = d.message_id;
          }));
          callback();
        }));
      }));
    }

    private void LogReceivedUpdates(UpdatesResponse updatesResponse)
    {
      StringBuilder stringBuilder = new StringBuilder("Received the following updates:" + Environment.NewLine);
      foreach (LongPollServerUpdateData update in updatesResponse.Updates)
        stringBuilder = stringBuilder.Append(update.ToString() + Environment.NewLine);
    }

    public delegate void UpdatesReceivedEventHandler(List<LongPollServerUpdateData> updates);
  }
}
