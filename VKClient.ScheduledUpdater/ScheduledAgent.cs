using Microsoft.Phone.Scheduler;
using System;
using System.Diagnostics;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.ScheduledUpdater
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        public ScheduledAgent()
        {
            if (!ScheduledAgent._classInitialized)
            {
                ScheduledAgent._classInitialized = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException+=(new EventHandler<ApplicationUnhandledExceptionEventArgs>(this.ScheduledAgent_UnhandledException));
                });
            }
        }

        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error("UNHANDLED exception in ScheduledAgent" + e.ExceptionObject);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        protected override async void OnInvoke(ScheduledTask task)
        {
            Logger.Instance.Info(("Entering ScheduledAgent.OnInvoke, task.Name=" + task.Name) ?? "", new object[0]);
            try
            {
                AppGlobalStateManager.Current.Initialize(true);
                if (AppGlobalStateManager.Current.LoggedInUserId != 0L)
                {
                    if (task.Name == "ExtensibilityTaskAgent")
                    {
                        await SocialDataManager.Instance.ProcessSocialOperationsQueue();
                        this.NotifyComplete();
                    }
                    else
                    {
                        SecondaryTileManager.Instance.UpdateAllExistingTiles(delegate(bool resSecondary)
                        {
                            CountersService.Instance.GetCountersWithLastMessage(delegate(BackendResult<CountersWithMessageInfo, ResultCode> res)
                            {
                                if (res.ResultCode == ResultCode.Succeeded)
                                {
                                    string content = "";
                                    string content2 = "";
                                    string content3 = "";
                                    if (res.ResultData.LastMessage != null && BaseFormatterHelper.UnixTimeStampToDateTime((double)res.ResultData.LastMessage.date, true) > AppGlobalStateManager.Current.GlobalState.LastDeactivatedTime)
                                    {
                                        MessageHeaderFormatterHelper.FormatForTileIntoThreeStrings(res.ResultData.LastMessage, res.ResultData.User, out content, out content2, out content3);
                                    }
                                    int messages = res.ResultData.Counters.messages;
                                    TileManager.Instance.SetContentAndCount(content, content2, content3, messages, delegate
                                    {
                                        base.NotifyComplete();
                                    });
                                    return;
                                }
                                base.NotifyComplete();
                            });
                        });
                    }
                }
            }
            catch (Exception var_2_FE)
            {
                Logger.Instance.Error("ScheduledAgent.OnInvoke failed", var_2_FE);
                this.NotifyComplete();
            }
        }
    }
}
