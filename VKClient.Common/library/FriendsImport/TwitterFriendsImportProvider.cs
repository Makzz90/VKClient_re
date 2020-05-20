using Hammock.Authentication.OAuth;
using Hammock.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using VKClient.Common.Library.FriendsImport.Twitter;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using Windows.System;

namespace VKClient.Common.Library.FriendsImport
{
    public class TwitterFriendsImportProvider : IFriendsImportProvider
    {
        private static TwitterFriendsImportProvider _instance;
        private List<TwitterUser> _users;
        private List<TwitterUser> _followers;
        private string _oauthToken;
        private string _oauthVerifier;
        private string _userId;
        private string _accessToken;
        private string _accessTokenSecret;
        private TwitterClient _twitterClient;
        private const string CONSUMER_KEY = "f5dowbYEtg4PfEOQZEdnHoHdT";
        private const string CONSUMER_KEY_SECRET = "cCosLgG0c7IeJL7qElkR9tMCMWPjDdMPDHGMY2QlOoHCbza09b";
        private const string REQUEST_TOKEN_URI = "https://api.twitter.com/oauth/request_token";
        private const string AUTHORIZE_URI = "https://api.twitter.com/oauth/authorize";
        private const string ACCESS_TOKEN_URI = "https://api.twitter.com/oauth/access_token";
        public const string REDIRECT_URI = "com.vk.vkclient://twitter-oauth/callback";
        private const string OAUTH_VERSION = "1.0a";

        public static TwitterFriendsImportProvider Instance
        {
            get
            {
                return TwitterFriendsImportProvider._instance ?? (TwitterFriendsImportProvider._instance = new TwitterFriendsImportProvider());
            }
        }

        public string ServiceName
        {
            get
            {
                return "twitter";
            }
        }

        public Func<string, InvitationItemHeader> InvitationConverterFunc
        {
            get
            {
                return (Func<string, InvitationItemHeader>)(id =>
                {
                    TwitterUser user = (TwitterUser)Enumerable.FirstOrDefault<TwitterUser>(this._followers, (Func<TwitterUser, bool>)(u => u.id.ToString() == id));
                    if (user == null)
                        return null;
                    return new InvitationItemHeader(user.name, string.Format("@{0}", user.screen_name), user.profile_image_url, (Action<Action<bool>>)(callback => this.InviteUser(user.screen_name, callback)));
                });
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
                return true;
            }
        }

        private TwitterFriendsImportProvider()
        {
            OAuthUtility.ComputeHash = (OAuthUtility.HashFunction)((key, buffer) =>
            {
                HMACSHA1 hmacshA1 = new HMACSHA1(key);
                try
                {
                    return ((HashAlgorithm)hmacshA1).ComputeHash(buffer);
                }
                finally
                {
                    if (hmacshA1 != null)
                        ((IDisposable)hmacshA1).Dispose();
                }
            });
        }

        public void Login()
        {
            OAuthWebQuery requestTokenQuery = TwitterFriendsImportProvider.GetRequestTokenQuery();
            EventHandler<WebQueryResponseEventArgs> eventHandler = (async (sender, args) =>
            {
                try
                {
                    this._oauthToken = TwitterFriendsImportProvider.GetQueryParameters(new StreamReader(args.Response).ReadToEnd())["oauth_token"];
                    await Launcher.LaunchUriAsync(new Uri(string.Format("{0}?oauth_token={1}", "https://api.twitter.com/oauth/authorize", this._oauthToken)));
                }
                catch
                {
                }
            });
            requestTokenQuery.QueryResponse += eventHandler;
            requestTokenQuery.RequestAsync("https://api.twitter.com/oauth/request_token", null);
        }

        private static OAuthWebQuery GetRequestTokenQuery()
        {
            OAuthWorkflow oauthWorkflow = new OAuthWorkflow();
            oauthWorkflow.ConsumerKey = "f5dowbYEtg4PfEOQZEdnHoHdT";
            oauthWorkflow.ConsumerSecret = "cCosLgG0c7IeJL7qElkR9tMCMWPjDdMPDHGMY2QlOoHCbza09b";
            oauthWorkflow.SignatureMethod = OAuthSignatureMethod.HmacSha1;
            oauthWorkflow.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;
            oauthWorkflow.RequestTokenUrl = "https://api.twitter.com/oauth/request_token";
            oauthWorkflow.Version = "1.0a";
            oauthWorkflow.CallbackUrl = "com.vk.vkclient://twitter-oauth/callback";
            
            OAuthWebQuery oauthWebQuery = new OAuthWebQuery(oauthWorkflow.BuildRequestTokenInfo((WebMethod)0), false);
            oauthWebQuery.HasElevatedPermissions = true;
            return oauthWebQuery;
        }

        private static Dictionary<string, string> GetQueryParameters(string response)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string str1 = response;
            char[] chArray = new char[1] { '&' };
            foreach (string str2 in ((string)str1).Split((char[])chArray))
            {
                if (((string)str2).Contains("="))
                {
                    string[] strArray = ((string)str2).Split((char[])new char[1] { '=' });
                    if (((string)strArray[0]).Contains("?"))
                        strArray[0] = ((string)strArray[0]).Replace("?", "");
                    dictionary.Add(strArray[0], HttpUtility.UrlDecode(strArray[1]));
                }
            }
            return dictionary;
        }

        public void SetAuthCompletionData(string oauthToken, string oauthVerifier)
        {
            this._oauthToken = oauthToken;
            this._oauthVerifier = oauthVerifier;
        }

        public void CompleteLogin(Action<bool> callback)
        {
            OAuthWebQuery accessTokenQuery = this.GetAccessTokenQuery();
            EventHandler<WebQueryResponseEventArgs> eventHandler = (EventHandler<WebQueryResponseEventArgs>)((sender, e) =>
            {
                try
                {
                    Dictionary<string, string> queryParameters = TwitterFriendsImportProvider.GetQueryParameters(new StreamReader(e.Response).ReadToEnd());
                    this._userId = queryParameters["user_id"];
                    this._accessToken = queryParameters["oauth_token"];
                    this._accessTokenSecret = queryParameters["oauth_token_secret"];
                    this._twitterClient = new TwitterClient("f5dowbYEtg4PfEOQZEdnHoHdT", "cCosLgG0c7IeJL7qElkR9tMCMWPjDdMPDHGMY2QlOoHCbza09b", new AccessToken(this._accessToken, this._accessTokenSecret));
                    callback(true);
                }
                catch (Exception )
                {
                    callback(false);
                }
            });
            accessTokenQuery.QueryResponse += eventHandler;
            string url = "https://api.twitter.com/oauth/access_token";
            // ISSUE: variable of the null type

            accessTokenQuery.RequestAsync(url, null);
        }

        private OAuthWebQuery GetAccessTokenQuery()
        {
            OAuthWorkflow oauthWorkflow = new OAuthWorkflow();
            oauthWorkflow.ConsumerKey = "f5dowbYEtg4PfEOQZEdnHoHdT";
            oauthWorkflow.ConsumerSecret = "cCosLgG0c7IeJL7qElkR9tMCMWPjDdMPDHGMY2QlOoHCbza09b";
            oauthWorkflow.SignatureMethod = OAuthSignatureMethod.HmacSha1;
            oauthWorkflow.ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader;
            oauthWorkflow.AccessTokenUrl = "https://api.twitter.com/oauth/access_token";
            oauthWorkflow.Version = "1.0a";
            string oauthToken = this._oauthToken;
            oauthWorkflow.Token = oauthToken;
            string oauthVerifier = this._oauthVerifier;
            oauthWorkflow.Verifier = oauthVerifier;
            int num1 = 1;
            OAuthWebQuery oauthWebQuery = new OAuthWebQuery(oauthWorkflow.BuildAccessTokenInfo((WebMethod)num1), false);
            int num2 = 1;
            oauthWebQuery.HasElevatedPermissions = num2 != 0;
            return oauthWebQuery;
        }

        public async void LoadExternalUserIds(Action<List<string>, bool> callback)
        {
            try
            {
                TwitterGenericResponse<List<TwitterUser>> friendsResponse = (TwitterGenericResponse<List<TwitterUser>>)await this._twitterClient.GetUsers("friends");
                TwitterGenericResponse<List<TwitterUser>> users = (TwitterGenericResponse<List<TwitterUser>>)await this._twitterClient.GetUsers("followers");
                List<TwitterUser> twitterUserList = new List<TwitterUser>();
                if (friendsResponse.Data != null)
                    twitterUserList.AddRange((IEnumerable<TwitterUser>)friendsResponse.Data);
                this._users = new List<TwitterUser>((IEnumerable<TwitterUser>)twitterUserList);
                if (users.Data != null)
                {
                    this._followers = new List<TwitterUser>((IEnumerable<TwitterUser>)users.Data);
                    twitterUserList.AddRange((IEnumerable<TwitterUser>)this._followers);
                    this._users = (List<TwitterUser>)Enumerable.ToList<TwitterUser>(Enumerable.Distinct<TwitterUser>(twitterUserList, (IEqualityComparer<TwitterUser>)new TwitterUserComparer()));
                }
                callback.Invoke((List<string>)Enumerable.ToList<string>(Enumerable.Select<TwitterUser, string>(this._users, (Func<TwitterUser, string>)(u => u.id.ToString()))), true);
                friendsResponse = null;
            }
            catch
            {
                callback.Invoke(null, false);
            }
        }

        public async void InviteUser(string id, Action<bool> callback)
        {
            //int num;
            if (this._twitterClient == null)
            {
                callback(false);
            }
            else
            {
                try
                {
                    string str = (string)await ((HttpResponseMessage)await this._twitterClient.SendDirectMessage(id, CommonResources.InviteToVK)).Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(str))
                    {
                        callback(false);
                        return;
                    }
                    TwitterErrorResponse twitterErrorResponse = JsonConvert.DeserializeObject<TwitterErrorResponse>(str);
                    if (twitterErrorResponse != null && twitterErrorResponse.errors.Count > 0)
                    {
                        callback(false);
                        return;
                    }
                    callback(true);
                    return;
                }
                catch
                {
                }
                callback(false);
            }
        }
    }
}
