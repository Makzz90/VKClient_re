using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationInterestingPagesViewModel : ViewModelBase, ICompleteable
  {
    private NewsFeedSuggestedSourcesViewModel _suggestedSourcesVM;
    private bool _isLoadedData;

    public NewsFeedSuggestedSourcesViewModel SuggestedSourcesVM
    {
      get
      {
        return this._suggestedSourcesVM;
      }
    }

    public bool IsCompleted
    {
      get
      {
        return true;
      }
    }

    public RegistrationInterestingPagesViewModel()
    {
      this._suggestedSourcesVM = new NewsFeedSuggestedSourcesViewModel();
    }

    internal void EnsureLoadData()
    {
      if (this._isLoadedData)
        return;
      this._suggestedSourcesVM.SuggestedSourcesVM.LoadData(true, false, (Action<BackendResult<VKList<UserOrGroupSource>, ResultCode>>) null, false);
      this._isLoadedData = true;
    }
  }
}
