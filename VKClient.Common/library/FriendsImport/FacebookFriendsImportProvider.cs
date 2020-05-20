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
                return null;
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
                {
                    return;
                }
                FacebookClient facebookClient = new FacebookClient(accessToken);
                JsonObject jsonObject = (await facebookClient.GetTaskAsync("me")) as JsonObject;
                this._userId = ((jsonObject != null) ? jsonObject["id"].ToString() : "");
                string path = string.Format(CultureInfo.InvariantCulture, "/me/friends?limit=1000&fields=id", new object[0]);
                IEnumerable<object> arg_192_0 = (IEnumerable<object>)((IDictionary<string, object>)(await facebookClient.GetTaskAsync(path)))["data"];
                Func<object, string> arg_192_1 = new Func<object, string>((friend) => { return new GraphUser(friend).Id; });

                List<string> list = Enumerable.ToList<string>(Enumerable.Select<object, string>(arg_192_0, arg_192_1));
                callback.Invoke(list, true);
                return;
            }
            catch
            {
            }
            callback.Invoke(null, false);
        }

        public void InviteUser(string id, Action<bool> callback)
        {
            callback(false);
        }
    }
}
