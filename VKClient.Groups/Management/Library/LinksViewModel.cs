using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
    public sealed class LinksViewModel : ViewModelBase, ICollectionDataProvider<List<Group>, LinkHeader>, IHandle<CommunityLinkAddedOrEdited>, IHandle
    {
        public readonly long CommunityId;

        public GenericCollectionViewModel<List<Group>, LinkHeader> Links { get; set; }

        public Func<List<Group>, ListWithCount<LinkHeader>> ConverterFunc
        {
            get
            {
                return (Func<List<Group>, ListWithCount<LinkHeader>>)(list =>
                {
                    List<GroupLink> links = list.First<Group>().links;
                    ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                    if (links != null)
                    {
                        listWithCount.TotalCount = links.Count;
                        foreach (GroupLink link in links)
                        {
                            LinkHeader linkHeader = new LinkHeader(link);
                            listWithCount.List.Add(linkHeader);
                        }
                    }
                    return listWithCount;
                });
            }
        }

        public LinksViewModel(long communityId)
        {
            this.CommunityId = communityId;
            this.Links = new GenericCollectionViewModel<List<Group>, LinkHeader>((ICollectionDataProvider<List<Group>, LinkHeader>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<List<Group>, LinkHeader> caller, int offset, int count, Action<BackendResult<List<Group>, ResultCode>> callback)
        {
            if (offset > 0)
                return;
            GroupsService.Current.GetCommunity(this.CommunityId, "links", callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<Group>, LinkHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoLinks;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneLinkFrm, CommonResources.TwoFourLinksFrm, CommonResources.FiveLinksFrm, true, (string)null, false);
        }

        public void DeleteLink(LinkHeader item)
        {
            this.SetInProgress(true, "");
            GroupsService.Current.DeleteLink(this.CommunityId, item.Id, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    this.Links.Delete(item);
                    EventAggregator.Current.Publish((object)new CommunityLinkDeleted()
                    {
                        CommunityId = this.CommunityId,
                        LinkId = item.Id
                    });
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                this.SetInProgress(false, "");
            }))));
        }

        public void Handle(CommunityLinkAddedOrEdited message)
        {
            if (this.CommunityId != message.CommunityId)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!message.IsEditing)
                {
                    this.Links.Insert(new LinkHeader(message.Link), 0);
                }
                else
                {
                    LinkHeader linkHeader = this.Links.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(link => link.Id == message.Link.id));
                    if (linkHeader == null)
                        return;
                    linkHeader.Title = message.Link.name;
                    linkHeader.Description = message.Link.desc;
                }
            }));
        }
    }
}
