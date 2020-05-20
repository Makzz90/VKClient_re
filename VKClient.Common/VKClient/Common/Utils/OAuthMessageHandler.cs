using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VKClient.Common.Utils
{
  public class OAuthMessageHandler : DelegatingHandler
  {
    private string consumerKey;
    private string consumerSecret;
    private Token token;
    private IEnumerable<KeyValuePair<string, string>> parameters;

    public OAuthMessageHandler(string consumerKey, string consumerSecret, Token token = null, IEnumerable<KeyValuePair<string, string>> optionalOAuthHeaderParameters = null)
      : this((HttpMessageHandler) new HttpClientHandler(), consumerKey, consumerSecret, token, optionalOAuthHeaderParameters)
    {
    }

    public OAuthMessageHandler(HttpMessageHandler innerHandler, string consumerKey, string consumerSecret, Token token = null, IEnumerable<KeyValuePair<string, string>> optionalOAuthHeaderParameters = null)
      : base(innerHandler)
    {
      this.consumerKey = consumerKey;
      this.consumerSecret = consumerSecret;
      this.token = token;
      this.parameters = optionalOAuthHeaderParameters ?? Enumerable.Empty<KeyValuePair<string, string>>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      IEnumerable<KeyValuePair<string, string>> sendParameter = this.parameters;
      if (request.Method == HttpMethod.Post && request.Content is FormUrlEncodedContent)
      {
        IEnumerable<KeyValuePair<string, string>> queryString = Utility.ParseQueryString(await request.Content.ReadAsStringAsync().ConfigureAwait(false), true);
        sendParameter = sendParameter.Concat<KeyValuePair<string, string>>(queryString);
        request.Content = (HttpContent) new OAuthMessageHandler.FormUrlEncodedContentEx(queryString);
      }
      request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", OAuthUtility.BuildBasicParameters(this.consumerKey, this.consumerSecret, request.RequestUri.OriginalString, request.Method, this.token, sendParameter).Concat<KeyValuePair<string, string>>(this.parameters).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (p => p.Key + "=" + p.Value.Wrap("\""))).ToString<string>(","));
      return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private class FormUrlEncodedContentEx : ByteArrayContent
    {
      public FormUrlEncodedContentEx(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        : base(OAuthMessageHandler.FormUrlEncodedContentEx.GetContentByteArray(nameValueCollection))
      {
        this.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
      }

      private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (KeyValuePair<string, string> nameValue in nameValueCollection)
        {
          if (stringBuilder.Length > 0)
            stringBuilder.Append('&');
          stringBuilder.Append(OAuthMessageHandler.FormUrlEncodedContentEx.Encode(nameValue.Key));
          stringBuilder.Append('=');
          stringBuilder.Append(OAuthMessageHandler.FormUrlEncodedContentEx.Encode(nameValue.Value));
        }
        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
      }

      private static string Encode(string data)
      {
        if (string.IsNullOrEmpty(data))
          return string.Empty;
        return data.UrlEncode().Replace("%20", "+");
      }
    }
  }
}
