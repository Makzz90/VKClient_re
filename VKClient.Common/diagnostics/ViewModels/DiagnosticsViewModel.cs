using System;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

using VKClient.Common.Library;

namespace VKClient.Common.Diagnostics.ViewModels
{
    public class DiagnosticsViewModel : ViewModelBase
    {
        private bool _isSending;

        public void SendData()
        {
            if (this._isSending)
                return;
            this._isSending = true;
            this.SetInProgress(true, "");
            AppsService.Instance.SendLog(Logger.Instance.GetLog(), (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._isSending = false;
                this.SetInProgress(false, "");
                ResultCode resultCode = result.ResultCode;
                if (resultCode == ResultCode.Succeeded)
                    new GenericInfoUC().ShowAndHideLater(CommonResources.LogSent, null);
                else
                    GenericInfoUC.ShowBasedOnResult(resultCode, "", null);
            }))));
        }

        //
        public bool IsLogsEnabled
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.IsLogsEnabled;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.IsLogsEnabled)
                    return;
                AppGlobalStateManager.Current.GlobalState.IsLogsEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsLogsEnabled));
                AppGlobalStateManager.Current.SaveState();
            }
        }
        //
    }
}
