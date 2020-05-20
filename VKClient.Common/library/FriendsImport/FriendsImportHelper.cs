using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.FriendsImport
{
  public static class FriendsImportHelper
  {
    public static void LoadData(IFriendsImportProvider provider, Action<FriendsImportResponse> completedCallback, Action<ResultCode> failedCallback)
    {
      provider.LoadExternalUserIds((Action<List<string>, bool>) ((userIds, succeeded) =>
      {
        if (!succeeded)
        {
          if (failedCallback == null)
            return;
          failedCallback(ResultCode.AccessDenied);
        }
        else
          AccountService.Instance.LookupContacts(provider.ServiceName, provider.MyContact, userIds, (Action<BackendResult<LookupContactsResponse, ResultCode>>) (result =>
          {
            FriendsImportResponse friendsImportResponse = new FriendsImportResponse();
            ResultCode resultCode = result.ResultCode;
            if (resultCode == ResultCode.Succeeded)
            {
              LookupContactsResponse resultData = result.ResultData;
              List<User> found = resultData.found;
              if (found != null)
              {
                foreach (User user in found)
                  friendsImportResponse.FoundUsers.Add((ISubscriptionItemHeader) new SubscriptionItemHeader(user, true));
              }
              if (provider.SupportInvitation)
              {
                List<User> other = resultData.other;
                if (other != null)
                {
                  List<InvitationItemHeader> source = new List<InvitationItemHeader>();
                  foreach (User user in other)
                  {
                    InvitationItemHeader invitationItemHeader = provider.InvitationConverterFunc(user.contact);
                    if (invitationItemHeader != null)
                      source.Add(invitationItemHeader);
                  }
                  friendsImportResponse.OtherUsers = new List<ISubscriptionItemHeader>(source.Distinct((Func<InvitationItemHeader, string>)(header => header.Subtitle)));
                }
              }
              if (completedCallback == null)
                return;
              completedCallback(friendsImportResponse);
            }
            else
            {
              if (failedCallback == null)
                return;
              failedCallback(resultCode);
            }
          }));
      }));
    }
  }
}
