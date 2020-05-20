using System;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Framework
{
    public abstract class ViewModelStatefulBase : ViewModelBase
    {
        private bool _isLoading;

        public GenericPageLoadInfoViewModel PageLoadInfoViewModel { get; private set; }

        public Action<ResultCode> ReloadCallback { get; set; }

        public void Reload(bool showOverlay = true)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            if (showOverlay)
                this.PageLoadInfoViewModel.LoadingState = PageLoadingState.Loading;
            else
                this.SetInProgress(true, "");
            this.Load((Action<ResultCode>)(resultCode =>
            {
                this._isLoading = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    bool flag = resultCode == ResultCode.Succeeded;
                    if (showOverlay)
                    {
                        if (!flag)
                            this.PageLoadInfoViewModel.Error = resultCode.GetErrorDescription();
                        this.PageLoadInfoViewModel.LoadingState = flag ? PageLoadingState.Loaded : PageLoadingState.LoadingFailed;
                    }
                    else
                        this.SetInProgress(false, "");
                }));
                Action<ResultCode> reloadCallback = this.ReloadCallback;
                if (reloadCallback == null)
                    return;
                int num = (int)resultCode;
                reloadCallback((ResultCode)num);
            }));
        }

        protected ViewModelStatefulBase()
        {
            this.PageLoadInfoViewModel = new GenericPageLoadInfoViewModel();
        }

        public abstract void Load(Action<ResultCode> callback);
    }
}
