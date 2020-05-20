using System;
using System.Collections.Generic;

namespace VKClient.Common.Library.FriendsImport
{
  public interface IFriendsImportProvider
  {
    string ServiceName { get; }

    Func<string, InvitationItemHeader> InvitationConverterFunc { get; }

    string MyContact { get; }

    bool SupportInvitation { get; }

    void Login();

    void CompleteLogin(Action<bool> callback);

    void LoadExternalUserIds(Action<List<string>, bool> callback);

    void InviteUser(string id, Action<bool> callback);
  }
}
