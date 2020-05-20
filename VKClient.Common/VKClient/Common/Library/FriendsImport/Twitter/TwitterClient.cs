using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterClient
  {
    private readonly string _consumerKey;
    private readonly string _consumerKeySecret;
    private readonly AccessToken _accessToken;

    public TwitterClient(string consumerKey, string consumerKeySecret, AccessToken accessToken)
    {
      this._consumerKey = consumerKey;
      this._consumerKeySecret = consumerKeySecret;
      this._accessToken = accessToken;
    }

    public async Task<TwitterGenericResponse<List<TwitterUser>>> GetUsers(string usersType)
    {
      TwitterGenericResponse<List<TwitterUser>> usersResponse = new TwitterGenericResponse<List<TwitterUser>>();
      string cursor = "";
      do
      {
        try
        {
          string requestUrl = string.Format("https://api.twitter.com/1.1/{0}/list.json?count=200&skip_status=true&include_user_entities=false", (object) usersType);
          if (this.IsCursorValid(cursor))
            requestUrl = requestUrl + "&cursor=" + cursor;
          string str = await (await this.PerformGetRequest(requestUrl)).Content.ReadAsStringAsync();
          TwitterErrorResponse twitterErrorResponse = JsonConvert.DeserializeObject<TwitterErrorResponse>(str);
          if (twitterErrorResponse != null && twitterErrorResponse.errors.Count > 0)
          {
            usersResponse.Error = twitterErrorResponse.errors.First<TwitterError>();
            break;
          }
          TwitterUsersBackendResponse usersBackendResponse = JsonConvert.DeserializeObject<TwitterUsersBackendResponse>(str);
          if (usersBackendResponse.users != null)
          {
            if (usersResponse.Data == null)
              usersResponse.Data = new List<TwitterUser>();
            usersResponse.Data.AddRange((IEnumerable<TwitterUser>) usersBackendResponse.users);
          }
          string nextCursorStr = usersBackendResponse.next_cursor_str;
          if (this.IsCursorValid(nextCursorStr))
            cursor = nextCursorStr;
          else
            break;
        }
        catch
        {
          break;
        }
      }
      while (this.IsCursorValid(cursor));
      return usersResponse;
    }

    private bool IsCursorValid(string cursor)
    {
      if (!string.IsNullOrEmpty(cursor))
        return cursor != "0";
      return false;
    }

    public async Task<HttpResponseMessage> SendDirectMessage(string screenName, string text)
    {
      return await this.PerformPostRequest("https://api.twitter.com/1.1/direct_messages/new.json", new List<KeyValuePair<string, string>>()
      {
        new KeyValuePair<string, string>("screen_name", screenName),
        new KeyValuePair<string, string>("text", text)
      });
    }

    private async Task<HttpResponseMessage> PerformGetRequest(string requestUrl)
    {
      try
      {
        return await OAuthUtility.CreateOAuthClient(this._consumerKey, this._consumerKeySecret, this._accessToken, (IEnumerable<KeyValuePair<string, string>>) null).GetAsync(requestUrl);
      }
      catch
      {
      }
      return (HttpResponseMessage) null;
    }

    private async Task<HttpResponseMessage> PerformPostRequest(string requestUrl, List<KeyValuePair<string, string>> parameters)
    {
      try
      {
        HttpClient oauthClient = OAuthUtility.CreateOAuthClient(this._consumerKey, this._consumerKeySecret, this._accessToken, (IEnumerable<KeyValuePair<string, string>>) null);
        FormUrlEncodedContent urlEncodedContent1 = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) parameters);
        string requestUri = requestUrl;
        FormUrlEncodedContent urlEncodedContent2 = urlEncodedContent1;
        return await oauthClient.PostAsync(requestUri, (HttpContent) urlEncodedContent2);
      }
      catch
      {
      }
      return (HttpResponseMessage) null;
    }
  }
}
