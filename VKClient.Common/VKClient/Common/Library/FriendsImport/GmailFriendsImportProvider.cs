using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VKClient.Common.Library.FriendsImport.Gmail;
using VKClient.Common.Localization;
using Windows.System;

namespace VKClient.Common.Library.FriendsImport
{
    public class GmailFriendsImportProvider : IFriendsImportProvider
    {
        private static readonly List<string> _scopes = new List<string>()
        {
          "https://www.googleapis.com/auth/contacts.readonly",
          "https://www.googleapis.com/auth/userinfo.email"
        };
        private readonly List<GoogleUser> _users = new List<GoogleUser>();
        private static GmailFriendsImportProvider _instance;
        private const string CLIENT_ID = "190525020719-gtptudf9k8tmf74d5pte39lci249b9gl.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "5ZQJQr35y4a4AX27A8DNc3oz";
        public const string REDIRECT_URI = "com.vk.vkclient:/gmail-oauth/code";
        private string _code;
        private string _email;
        private string _accessToken;

        public static GmailFriendsImportProvider Instance
        {
            get
            {
                return GmailFriendsImportProvider._instance ?? (GmailFriendsImportProvider._instance = new GmailFriendsImportProvider());
            }
        }

        public string ServiceName
        {
            get
            {
                return "email";
            }
        }

        public Func<string, InvitationItemHeader> InvitationConverterFunc
        {
            get
            {
                return (Func<string, InvitationItemHeader>)(email =>
                {
                    GoogleUser user = this._users.FirstOrDefault<GoogleUser>((Func<GoogleUser, bool>)(u => u.Email == email));
                    if (user == null)
                        return (InvitationItemHeader)null;
                    return new InvitationItemHeader(user.Name, user.Email, user.Photo, (Action<Action<bool>>)(callback => this.InviteUser(user.Email, callback)));
                });
            }
        }

        public string MyContact
        {
            get
            {
                return this._email;
            }
        }

        public bool SupportInvitation
        {
            get
            {
                return true;
            }
        }

        public async void Login()//omg_re async
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code", "190525020719-gtptudf9k8tmf74d5pte39lci249b9gl.apps.googleusercontent.com", "com.vk.vkclient:/gmail-oauth/code", WebUtility.UrlEncode(string.Join(" ", (IEnumerable<string>)GmailFriendsImportProvider._scopes)))));
        }

        public void SetCode(string code)
        {
            this._code = code;
        }

        public async void CompleteLogin(Action<bool> callback)
        {
            GmailAccessTokenResponse accessToken = await this.GetAccessToken();
            if (accessToken != null)
            {
                this._accessToken = accessToken.access_token;
                callback(true);
            }
            else
                callback(false);
        }

        private async Task<GmailAccessTokenResponse> GetAccessToken()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                List<KeyValuePair<string, string>> keyValuePairList = new List<KeyValuePair<string, string>>();
                keyValuePairList.Add(new KeyValuePair<string, string>("code", this._code));
                keyValuePairList.Add(new KeyValuePair<string, string>("client_id", "190525020719-gtptudf9k8tmf74d5pte39lci249b9gl.apps.googleusercontent.com"));
                keyValuePairList.Add(new KeyValuePair<string, string>("client_secret", "5ZQJQr35y4a4AX27A8DNc3oz"));
                keyValuePairList.Add(new KeyValuePair<string, string>("redirect_uri", "com.vk.vkclient:/gmail-oauth/code"));
                keyValuePairList.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                httpClient.BaseAddress = new Uri("https://www.googleapis.com");
                HttpContent content = (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)keyValuePairList);
                return JsonConvert.DeserializeObject<GmailAccessTokenResponse>(await (await httpClient.PostAsync("/oauth2/v3/token", content)).Content.ReadAsStringAsync());
            }
            catch
            {
            }
            return (GmailAccessTokenResponse)null;
        }

        public async void LoadExternalUserIds(Action<List<string>, bool> callback)
        {
            if (string.IsNullOrEmpty(this._accessToken))
                return;
            try
            {
                JObject jobject = JObject.Parse(await (await new HttpClient().GetAsync(new Uri(string.Format("https://www.google.com/m8/feeds/contacts/default/full?alt=json&max-results=1000&access_token={0}", (object)this._accessToken)))).Content.ReadAsStringAsync());
                string index1 = "feed";
                JToken jtoken1 = jobject[index1][(object)"id"][(object)"$t"];
                if (jtoken1 != null)
                    this._email = jtoken1.ToString();
                string index2 = "feed";
                foreach (JToken jtoken2 in (IEnumerable<JToken>)jobject[index2][(object)"entry"].Children().ToList<JToken>())
                {
                    JToken jtoken3 = jtoken2[(object)"gd$email"];
                    if (jtoken3 != null)
                    {
                        List<JToken> list = jtoken3.Children().ToList<JToken>();
                        if (list.Count > 0)
                        {
                            JToken jtoken4 = list[0][(object)"address"];
                            if (jtoken4 != null)
                            {
                                string str1 = string.Empty;
                                string str2 = string.Empty;
                                string str3 = string.Empty;
                                if (jtoken2[(object)"id"] != null && jtoken2[(object)"id"][(object)"$t"] != null)
                                    str1 = jtoken2[(object)"id"][(object)"$t"].ToString();
                                if (jtoken2[(object)"title"] != null && jtoken2[(object)"title"][(object)"$t"] != null)
                                    str2 = jtoken2[(object)"title"][(object)"$t"].ToString();
                                if (string.IsNullOrEmpty(str2))
                                    str2 = jtoken4.ToString();
                                JToken jtoken5 = jtoken2[(object)"link"];
                                if (jtoken5 != null)
                                {
                                    JToken jtoken6 = jtoken5.Children().ToList<JToken>().FirstOrDefault<JToken>((Func<JToken, bool>)(link =>
                                    {
                                        if (link[(object)"type"] != null && link[(object)"rel"] != null && link[(object)"type"].ToString() == "image/*")
                                            return link[(object)"rel"].ToString().EndsWith("#photo");
                                        return false;
                                    }));
                                    if (jtoken6 != null)
                                        str3 = string.Format("{0}?access_token={1}", jtoken6["href"], this._accessToken);
                                }
                                List<GoogleUser> googleUserList = this._users;
                                GoogleUser googleUser = new GoogleUser();
                                googleUser.Id = str1;
                                googleUser.Name = str2;
                                string @string = jtoken4.ToString();
                                googleUser.Email = @string;
                                string str4 = str3;
                                googleUser.Photo = str4;
                                googleUserList.Add(googleUser);
                            }
                        }
                    }
                }
                callback(this._users.Select<GoogleUser, string>((Func<GoogleUser, string>)(u => u.Email)).ToList<string>(), true);
                return;
            }
            catch
            {
            }
            callback((List<string>)null, false);
        }

        public void InviteUser(string id, Action<bool> callback)
        {
            new EmailComposeTask()
            {
                Subject = CommonResources.VK,
                Body = CommonResources.InviteToVK,
                To = id
            }.Show();
            callback(false);
        }
    }
}
