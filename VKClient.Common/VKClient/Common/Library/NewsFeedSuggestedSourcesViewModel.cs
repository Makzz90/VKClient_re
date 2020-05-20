using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class NewsFeedSuggestedSourcesViewModel : ViewModelBase, ICollectionDataProvider<VKList<UserOrGroupSource>, SubscriptionItemHeader>
  {
    private GenericCollectionViewModel<VKList<UserOrGroupSource>, SubscriptionItemHeader> _suggestedSourcesVM;

    public GenericCollectionViewModel<VKList<UserOrGroupSource>, SubscriptionItemHeader> SuggestedSourcesVM
    {
      get
      {
        return this._suggestedSourcesVM;
      }
    }

    public Func<VKList<UserOrGroupSource>, ListWithCount<SubscriptionItemHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<UserOrGroupSource>, ListWithCount<SubscriptionItemHeader>>) (l =>
        {
          ListWithCount<SubscriptionItemHeader> listWithCount = new ListWithCount<SubscriptionItemHeader>();
          foreach (UserOrGroupSource userOrGroupSource in l.items)
          {
            User user = userOrGroupSource.GetUser();
            SubscriptionItemHeader subscriptionItemHeader = (SubscriptionItemHeader) null;
            if (user != null)
            {
              subscriptionItemHeader = new SubscriptionItemHeader(user, false);
            }
            else
            {
              Group group = userOrGroupSource.GetGroup();
              if (group != null)
                subscriptionItemHeader = new SubscriptionItemHeader(group);
            }
            if (subscriptionItemHeader != null)
              listWithCount.List.Add(subscriptionItemHeader);
          }
          listWithCount.TotalCount = l.count;
          return listWithCount;
        });
      }
    }

    public NewsFeedSuggestedSourcesViewModel()
    {
      this._suggestedSourcesVM = new GenericCollectionViewModel<VKList<UserOrGroupSource>, SubscriptionItemHeader>((ICollectionDataProvider<VKList<UserOrGroupSource>, SubscriptionItemHeader>) this);
      this._suggestedSourcesVM.LoadCount = 200;
    }

    public void GetData(GenericCollectionViewModel<VKList<UserOrGroupSource>, SubscriptionItemHeader> caller, int offset, int count, Action<BackendResult<VKList<UserOrGroupSource>, ResultCode>> callback)
    {
      NewsFeedService.Current.GetSuggestedSources(offset, count, true, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<UserOrGroupSource>, SubscriptionItemHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPages;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePageFrm, CommonResources.TwoFourPagesFrm, CommonResources.FivePagesFrm, true, null, false);
    }
  }
}
