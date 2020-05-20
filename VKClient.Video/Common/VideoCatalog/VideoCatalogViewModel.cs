using System;
using System.Collections.Specialized;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Video.VideoCatalog;

namespace VKClient.Common.VideoCatalog
{
  public class VideoCatalogViewModel : ViewModelBase, ICollectionDataProvider<GetCatalogResponse, CatalogCategoryViewModel>
  {
    private string _next = "";
    private GenericCollectionViewModel<GetCatalogResponse, CatalogCategoryViewModel> _categoriesVM;
    private UserVideosViewModel _userVideosVM;

    public GenericCollectionViewModel<GetCatalogResponse, CatalogCategoryViewModel> CategoriesVM
    {
      get
      {
        return this._categoriesVM;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Videos.ToUpperInvariant();
      }
    }

    public UserVideosViewModel UserVideosVM
    {
      get
      {
        return this._userVideosVM;
      }
    }

    public Func<GetCatalogResponse, ListWithCount<CatalogCategoryViewModel>> ConverterFunc
    {
      get
      {
        return (Func<GetCatalogResponse, ListWithCount<CatalogCategoryViewModel>>) (gcr =>
        {
          this._next = gcr.next ?? "";
          ListWithCount<CatalogCategoryViewModel> listWithCount = new ListWithCount<CatalogCategoryViewModel>();
          foreach (VideoCatalogCategory c in gcr.items)
          {
            if (!(c.id == "my") && c.items.Count != 0)
              listWithCount.List.Add(new CatalogCategoryViewModel(c, gcr.profiles, gcr.groups));
          }
          return listWithCount;
        });
      }
    }

    public VideoCatalogViewModel()
    {
      this._categoriesVM = new GenericCollectionViewModel<GetCatalogResponse, CatalogCategoryViewModel>((ICollectionDataProvider<GetCatalogResponse, CatalogCategoryViewModel>) this);
      this._categoriesVM.Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Collection_CollectionChanged);
      this._categoriesVM.LoadCount = 5;
      this._categoriesVM.ReloadCount = 10;
      this._userVideosVM = new UserVideosViewModel(AppGlobalStateManager.Current.LoggedInUserId);
    }

    private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count > 0)
        this._categoriesVM.MergedCollection.Merge((INotifyCollectionChanged) (e.NewItems[0] as CatalogCategoryViewModel).CatalogItemsWithHeaderAndFooter);
      if (e.Action != NotifyCollectionChangedAction.Reset)
        return;
      this._categoriesVM.MergedCollection.Reset();
    }

    public void LoadData(bool refresh)
    {
      this._categoriesVM.LoadData(refresh, false, (Action<BackendResult<GetCatalogResponse, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<GetCatalogResponse, CatalogCategoryViewModel> caller, int offset, int count, Action<BackendResult<GetCatalogResponse, ResultCode>> callback)
    {
      if (offset > 0 && this._next == "")
        callback(new BackendResult<GetCatalogResponse, ResultCode>(ResultCode.Succeeded, new GetCatalogResponse()));
      else
        VideoService.Instance.GetVideoCatalog(count, 16, offset > 0 ? this._next : "", callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GetCatalogResponse, CatalogCategoryViewModel> caller, int count)
    {
      return "";
    }
  }
}
