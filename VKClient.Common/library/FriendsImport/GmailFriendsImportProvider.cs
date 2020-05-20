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
    private static readonly List<string> _scopes = new List<string>() { "https://www.googleapis.com/auth/contacts.readonly", "https://www.googleapis.com/auth/userinfo.email" };
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
        return (Func<string, InvitationItemHeader>) (email =>
        {
            GoogleUser user = (GoogleUser)Enumerable.FirstOrDefault<GoogleUser>(this._users, (Func<GoogleUser, bool>)(u => u.Email == email));
          if (user == null)
            return  null;
          return new InvitationItemHeader(user.Name, user.Email, user.Photo, (Action<Action<bool>>) (callback => this.InviteUser(user.Email, callback)));
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

    public void Login()
    {
      Launcher.LaunchUriAsync(new Uri(string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code", "190525020719-gtptudf9k8tmf74d5pte39lci249b9gl.apps.googleusercontent.com", "com.vk.vkclient:/gmail-oauth/code", WebUtility.UrlEncode(string.Join(" ", (IEnumerable<string>) GmailFriendsImportProvider._scopes)))));
    }

    public void SetCode(string code)
    {
      this._code = code;
    }

    public async void CompleteLogin(Action<bool> callback)
    {
      GmailAccessTokenResponse accessToken = (GmailAccessTokenResponse) await this.GetAccessToken();
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
        httpClient.BaseAddress=(new Uri("https://www.googleapis.com"));
        HttpContent httpContent = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) keyValuePairList);
        return JsonConvert.DeserializeObject<GmailAccessTokenResponse>((string) await ((HttpResponseMessage) await httpClient.PostAsync("/oauth2/v3/token", httpContent)).Content.ReadAsStringAsync());
      }
      catch (Exception )
      {
      }
      return  null;
    }

    public async void LoadExternalUserIds(Action<List<string>, bool> callback)
    {
      if (string.IsNullOrEmpty(this._accessToken))
        return;
      try
      {
        JObject jobject = JObject.Parse((string) await ((HttpResponseMessage) await new HttpClient().GetAsync(new Uri(string.Format("https://www.google.com/m8/feeds/contacts/default/full?alt=json&max-results=1000&access_token={0}", this._accessToken)))).Content.ReadAsStringAsync());
        string index1 = "feed";
        JToken jtoken1 = jobject[index1]["id"]["$t"];
        if (jtoken1 != null)
          this._email = (jtoken1).ToString();
        string index2 = "feed";
        IEnumerator<JToken> enumerator = ((IEnumerable<JToken>) Enumerable.ToList<JToken>(jobject[index2]["entry"].Children())).GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            JToken current = enumerator.Current;
            JToken jtoken2 = current["gd$email"];
            if (jtoken2 != null)
            {
              List<JToken> list = (List<JToken>) Enumerable.ToList<JToken>(jtoken2.Children());
              if (list.Count > 0)
              {
                JToken jtoken3 = list[0]["address"];
                if (jtoken3 != null)
                {
                  string empty1 = string.Empty;
                  string empty2 = string.Empty;
                  string str1 = string.Empty;
                  if (current["id"] != null && current["id"]["$t"] != null)
                    empty1 = (current["id"]["$t"]).ToString();
                  if (current["title"] != null && current["title"]["$t"] != null)
                    empty2 = (current["title"]["$t"]).ToString();
                  if (string.IsNullOrEmpty(empty2))
                    empty2 = (jtoken3).ToString();
                  JToken jtoken4 = current["link"];
                  if (jtoken4 != null)
                  {
                      JToken jtoken5 = (JToken)Enumerable.FirstOrDefault<JToken>(Enumerable.ToList<JToken>(jtoken4.Children()), (Func<JToken, bool>)(link =>
                    {
                      if (link["type"] != null && link["rel"] != null && (link["type"]).ToString() == "image/*")
                        return ((string) (link["rel"]).ToString()).EndsWith("#photo");
                      return false;
                    }));
                    if (jtoken5 != null)
                      str1 = string.Format("{0}?access_token={1}", jtoken5["href"], this._accessToken);
                  }
                  List<GoogleUser> users = this._users;
                  GoogleUser googleUser = new GoogleUser();
                  googleUser.Id = empty1;
                  googleUser.Name = empty2;
                  string str2 = (jtoken3).ToString();
                  googleUser.Email = str2;
                  string str3 = str1;
                  googleUser.Photo = str3;
                  users.Add(googleUser);
                }
              }
            }
          }
        }
        finally
        {
          if (enumerator != null)
            enumerator.Dispose();
        }
        callback.Invoke((List<string>)Enumerable.ToList<string>(Enumerable.Select<GoogleUser, string>(this._users, (Func<GoogleUser, string>)(u => u.Email))), true);
        return;
      }
      catch
      {
      }
      callback.Invoke( null, false);
    }

    public void InviteUser(string id, Action<bool> callback)
    {
      EmailComposeTask emailComposeTask = new EmailComposeTask();
      string vk = CommonResources.VK;
      emailComposeTask.Subject = vk;
      string inviteToVk = CommonResources.InviteToVK;
      emailComposeTask.Body = inviteToVk;
      string str = id;
      emailComposeTask.To = str;
      emailComposeTask.Show();
      callback(false);
    }
  }
}
