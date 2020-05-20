using Facebook;
using Facebook.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VKClient.Common.Library.FriendsImport
{
  public class FacebookFriendsImportProvider : IFriendsImportProvider
  {
    private static FacebookFriendsImportProvider _instance;
    public const string REDIRECT_URI = "fb128749580520227://authorize";
    private string _userId;

    public static FacebookFriendsImportProvider Instance
    {
      get
      {
        return FacebookFriendsImportProvider._instance ?? (FacebookFriendsImportProvider._instance = new FacebookFriendsImportProvider());
      }
    }

    public string ServiceName
    {
      get
      {
        return "facebook";
      }
    }

    public Func<string, InvitationItemHeader> InvitationConverterFunc
    {
      get
      {
        return (Func<string, InvitationItemHeader>) null;
      }
    }

    public string MyContact
    {
      get
      {
        return this._userId;
      }
    }

    public bool SupportInvitation
    {
      get
      {
        return false;
      }
    }

    public void Login()
    {
      Session.ActiveSession.LoginWithBehavior("user_friends", FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
    }

    public void CompleteLogin(Action<bool> callback)
    {
      callback(true);
    }

    public async void LoadExternalUserIds(Action<List<string>, bool> callback)
    {
      try
      {
        string accessToken = Session.ActiveSession.CurrentAccessTokenData.AccessToken;
        if (string.IsNullOrEmpty(accessToken))
          return;
        FacebookClient facebookClient = new FacebookClient(accessToken);
        JsonObject jsonObject = await facebookClient.GetTaskAsync("me") as JsonObject;
        this._userId = jsonObject != null ? jsonObject["id"].ToString() : "";
        object taskAsync = await facebookClient.GetTaskAsync(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "/me/friends?limit=1000&fields=id", new object[0]));
        callback(((IEnumerable<object>) ((IDictionary<string, object>) taskAsync)["data"]).Select<object, string>((Func<object, string>) (friend => new GraphUser(friend).Id)).ToList<string>(), true);
        return;
      }
      catch
      {
      }
      callback((List<string>) null, false);
    }

    public void InviteUser(string id, Action<bool> callback)
    {
      callback(false);
    }
  }
}
