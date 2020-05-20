using System;
using System.Collections.Generic;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.MoneyTransfers.Library
{
  public sealed class TransfersListViewModel : ViewModelBase, ICollectionDataProvider<VKList<MoneyTransfer>, MoneyTransferViewModel>, IHandle<MoneyTransferSentEvent>, IHandle
  {
      public GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel> Inbox { get; private set; }

      public GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel> Outbox { get; private set; }

    public Func<VKList<MoneyTransfer>, ListWithCount<MoneyTransferViewModel>> ConverterFunc
    {
      get
      {
        return (Func<VKList<MoneyTransfer>, ListWithCount<MoneyTransferViewModel>>) (list =>
        {
          ListWithCount<MoneyTransferViewModel> listWithCount = new ListWithCount<MoneyTransferViewModel>() { TotalCount = list.count, List = new List<MoneyTransferViewModel>() };
          foreach (MoneyTransfer transfer in list.items)
          {
            bool flag = false;
            foreach (User profile in list.profiles)
            {
              if (profile.id == (transfer.IsOutbox ? transfer.to_id : transfer.from_id))
              {
                listWithCount.List.Add(new MoneyTransferViewModel(transfer, profile));
                flag = true;
                break;
              }
            }
            if (!flag)
            {
              foreach (Group group in list.groups)
              {
                if (group.id == -(transfer.IsOutbox ? transfer.to_id : transfer.from_id))
                {
                  User user = new User() { id = -group.id, first_name = group.name, photo_max = group.photo_max };
                  listWithCount.List.Add(new MoneyTransferViewModel(transfer, user));
                  break;
                }
              }
            }
          }
          return listWithCount;
        });
      }
    }

    public TransfersListViewModel()
    {
      this.Inbox = new GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel>((ICollectionDataProvider<VKList<MoneyTransfer>, MoneyTransferViewModel>) this);
      this.Outbox = new GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel>((ICollectionDataProvider<VKList<MoneyTransfer>, MoneyTransferViewModel>) this);
      EventAggregator.Current.Subscribe(this);
    }

    public void GetData(GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel> caller, int offset, int count, Action<BackendResult<VKList<MoneyTransfer>, ResultCode>> callback)
    {
      MoneyTransfersService.GetTransfersList(caller == this.Inbox ? 2 : 1, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<MoneyTransfer>, MoneyTransferViewModel> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoTransfers;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneTransferForm, CommonResources.TwoFourTransfersForm, CommonResources.FiveTransfersForm, true,  null, false);
    }

    public void Handle(MoneyTransferSentEvent message)
    {
      this.Outbox.LoadData(true, false,  null, true);
    }
  }
}
