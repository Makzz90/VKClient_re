using System;
using System.Collections.Generic;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogItemsHorizontalViewModel : ICollectionDataProvider<GetCatalogSectionResponse, CatalogItemViewModel>
  {
    private VideoCatalogCategory _videoCategory;
    private List<User> _knownUsers;
    private List<Group> _knownGroups;
    private GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> _catalogItemsVM;
    private string _next;

    public GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> CatalogItemsVM
    {
      get
      {
        return this._catalogItemsVM;
      }
    }

    public Func<GetCatalogSectionResponse, ListWithCount<CatalogItemViewModel>> ConverterFunc
    {
      get
      {
        return (Func<GetCatalogSectionResponse, ListWithCount<CatalogItemViewModel>>) (gcmr =>
        {
          ListWithCount<CatalogItemViewModel> listWithCount = new ListWithCount<CatalogItemViewModel>();
          foreach (VideoCatalogItem videoCatalogItem in gcmr.items)
          {
            CatalogItemViewModel catalogItemViewModel = this.CreateCatalogItemViewModel(videoCatalogItem, gcmr.profiles, gcmr.groups);
            listWithCount.List.Add(catalogItemViewModel);
          }
          this._next = gcmr.next;
          return listWithCount;
        });
      }
    }

    public CatalogItemsHorizontalViewModel(VideoCatalogCategory c, List<User> knownUsers, List<Group> knownGroups)
    {
      this._videoCategory = c;
      this._knownUsers = knownUsers;
      this._knownGroups = knownGroups;
      this._next = this._videoCategory.next;
      this._catalogItemsVM = new GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel>((ICollectionDataProvider<GetCatalogSectionResponse, CatalogItemViewModel>) this);
      this.Initialize();
    }

    public void Initialize()
    {
      ListWithCount<CatalogItemViewModel> listWithCount = new ListWithCount<CatalogItemViewModel>();
      foreach (VideoCatalogItem videoCatalogItem in this._videoCategory.items)
      {
        CatalogItemViewModel catalogItemViewModel = this.CreateCatalogItemViewModel(videoCatalogItem, this._knownUsers, this._knownGroups);
        listWithCount.List.Add(catalogItemViewModel);
      }
      this._catalogItemsVM.ReadData(listWithCount);
    }

    public void GetData(GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> caller, int offset, int count, Action<BackendResult<GetCatalogSectionResponse, ResultCode>> callback)
    {
      VideoService.Instance.GetVideoCatalogSection(this._videoCategory.id, this._next, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> caller, int count)
    {
      return "";
    }

    private CatalogItemViewModel CreateCatalogItemViewModel(VideoCatalogItem item, List<User> users, List<Group> groups)
    {
      return new CatalogItemViewModel(item, users, groups, false)
      {
        ActionSource = new StatisticsActionSource?(StatisticsActionSource.video_catalog),
        VideoContext = this._videoCategory.id
      };
    }
  }
}
