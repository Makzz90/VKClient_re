using System.Collections.Generic;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public abstract class GenericSearchViewModelBase : ViewModelBase
  {
    public abstract string SearchString { get; set; }

    public abstract Dictionary<string, string> Parameters { get; set; }

    public abstract void LoadData(bool refresh = false, bool suppressLoadingMessage = false, bool suppressSystemTrayProgress = false, bool clearCollectionOnRefresh = false, bool instantLoad = false);

    public abstract void LoadMoreIfNeeded(object linkedItem);
  }
}
