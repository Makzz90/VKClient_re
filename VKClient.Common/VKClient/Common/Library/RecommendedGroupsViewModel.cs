using System;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class RecommendedGroupsViewModel : ViewModelBase, ICollectionDataProvider<VKList<Group>, SubscriptionItemHeader>, ICollectionDataProvider<GroupCatalogInfoExtended, CatalogCategoryHeader>
  {
    private string _categoryTitle = "";
    private int _categoryId;
    private GenericCollectionViewModel<VKList<Group>, SubscriptionItemHeader> _recommendations;
    private GenericCollectionViewModel<GroupCatalogInfoExtended, CatalogCategoryHeader> _catalogCategories;

    public bool HaveCatalog
    {
      get
      {
        return this._catalogCategories.Collection.Count > 0;
      }
    }

    public GenericCollectionViewModel<VKList<Group>, SubscriptionItemHeader> Recommendations
    {
      get
      {
        return this._recommendations;
      }
    }

    public GenericCollectionViewModel<GroupCatalogInfoExtended, CatalogCategoryHeader> CatalogCategories
    {
      get
      {
        return this._catalogCategories;
      }
    }

    public string RecommendationsListTitle
    {
      get
      {
        if (this._categoryId != 0)
          return this._categoryTitle.ToLowerInvariant();
        return CommonResources.RecommendedGroups_Recommendations;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.RecommendedGroups_Title.ToUpperInvariant();
      }
    }

    public Func<VKList<Group>, ListWithCount<SubscriptionItemHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Group>, ListWithCount<SubscriptionItemHeader>>) (vkList => new ListWithCount<SubscriptionItemHeader>()
        {
          TotalCount = vkList.items.Count,
          List = vkList.items.Select<Group, SubscriptionItemHeader>((Func<Group, SubscriptionItemHeader>) (g => new SubscriptionItemHeader(g))).ToList<SubscriptionItemHeader>()
        });
      }
    }

    Func<GroupCatalogInfoExtended, ListWithCount<CatalogCategoryHeader>> ICollectionDataProvider<GroupCatalogInfoExtended, CatalogCategoryHeader>.ConverterFunc
    {
      get
      {
        return (Func<GroupCatalogInfoExtended, ListWithCount<CatalogCategoryHeader>>) (gcie =>
        {
          ListWithCount<CatalogCategoryHeader> listWithCount = new ListWithCount<CatalogCategoryHeader>();
          if (!gcie.categories.IsNullOrEmpty())
          {
            listWithCount.TotalCount = gcie.categories.Count - 1;
            listWithCount.List = gcie.categories.Where<GroupCatalogCategoryPreview>((Func<GroupCatalogCategoryPreview, bool>) (c => (uint) c.id > 0U)).Select<GroupCatalogCategoryPreview, CatalogCategoryHeader>((Func<GroupCatalogCategoryPreview, CatalogCategoryHeader>) (gccp => new CatalogCategoryHeader(gccp))).ToList<CatalogCategoryHeader>();
          }
          return listWithCount;
        });
      }
    }

    public RecommendedGroupsViewModel(int categoryId = 0, string categortyTitle = "")
    {
      this._categoryId = categoryId;
      this._categoryTitle = categortyTitle;
      this._recommendations = new GenericCollectionViewModel<VKList<Group>, SubscriptionItemHeader>((ICollectionDataProvider<VKList<Group>, SubscriptionItemHeader>) this);
      this._catalogCategories = new GenericCollectionViewModel<GroupCatalogInfoExtended, CatalogCategoryHeader>((ICollectionDataProvider<GroupCatalogInfoExtended, CatalogCategoryHeader>) this);
    }

    public void GetData(GenericCollectionViewModel<VKList<Group>, SubscriptionItemHeader> caller, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      if (this._categoryId == 0)
        this.CatalogCategories.LoadData(false, false, (Action<BackendResult<GroupCatalogInfoExtended, ResultCode>>) null, false);
      GroupsService.Current.GetCatalog(this._categoryId, (Action<BackendResult<VKList<Group>, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.CatalogIsNotAvailable || res.ResultCode == ResultCode.CatalogCategoriesAreNotAvailable)
        {
          res.ResultCode = ResultCode.Succeeded;
          res.ResultData = new VKList<Group>();
        }
        callback(res);
      }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Group>, SubscriptionItemHeader> caller, int count)
    {
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<GroupCatalogInfoExtended, CatalogCategoryHeader> caller, int offset, int count, Action<BackendResult<GroupCatalogInfoExtended, ResultCode>> callback)
    {
      GroupsService.Current.GetCatalogCategoriesPreview(callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GroupCatalogInfoExtended, CatalogCategoryHeader> caller, int count)
    {
      return "";
    }
  }
}
