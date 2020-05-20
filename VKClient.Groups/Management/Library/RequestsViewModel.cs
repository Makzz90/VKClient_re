using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
    public sealed class RequestsViewModel : ViewModelBase, ICollectionDataProvider<VKList<User>, FriendHeader>
    {
        public readonly long CommunityId;

        public GenericCollectionViewModel<VKList<User>, FriendHeader> Requests { get; private set; }

        public Func<VKList<User>, ListWithCount<FriendHeader>> ConverterFunc
        {
            get
            {
                Func<VKList<User>, ListWithCount<FriendHeader>> arg_1F_0 = new Func<VKList<User>, ListWithCount<FriendHeader>>((list) =>
                    {
                        ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
                        listWithCount.TotalCount = list.count;
                        using (List<User>.Enumerator enumerator = list.items.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                User current = enumerator.Current;
                                listWithCount.List.Add(new FriendHeader(current, false));
                            }
                        }
                        return listWithCount;
                    });

                return arg_1F_0;
            }
        }

        public RequestsViewModel(long communityId)
        {
            this.CommunityId = communityId;
            this.Requests = new GenericCollectionViewModel<VKList<User>, FriendHeader>((ICollectionDataProvider<VKList<User>, FriendHeader>)this)
            {
                LoadCount = 60,
                ReloadCount = 100
            };
        }

        public void GetData(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
        {
            GroupsService.Current.GetRequests(this.CommunityId, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoRequests;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.RequestOneForm, CommonResources.RequestTwoForm, CommonResources.RequestFiveForm, true, null, false);
        }

        public void HandleRequest(FriendHeader item, bool isAcception)
        {
            base.SetInProgress(true, "");
            GroupsService.Current.HandleRequest(this.CommunityId, item.UserId, isAcception, delegate(BackendResult<int, ResultCode> result)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (result.ResultCode == ResultCode.Succeeded)
                    {
                        this.Requests.Delete(item);
                    }
                    else
                    {
                        VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                    }
                    this.SetInProgress(false, "");
                });
            });
        }

    }
}
