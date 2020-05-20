using System;

namespace VKClient.Common.Framework
{
    public abstract class ViewModelStatefulBase : ViewModelBase
    {
        public GenericPageLoadInfoViewModel PageLoadInfoViewModel { get; set; }

        public void Reload()
        {
            this.PageLoadInfoViewModel.LoadingState = PageLoadingState.Loading;
            this.Load((Action<bool>)(loaded => Execute.ExecuteOnUIThread((Action)(() => this.PageLoadInfoViewModel.LoadingState = loaded ? PageLoadingState.Loaded : PageLoadingState.LoadingFailed))));
        }

        public abstract void Load(Action<bool> callback);

        protected ViewModelStatefulBase()
        {
            this.PageLoadInfoViewModel = new GenericPageLoadInfoViewModel();
        }
    }
}
