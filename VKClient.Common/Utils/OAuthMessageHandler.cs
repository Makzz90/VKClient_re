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
            : this((HttpMessageHandler)new HttpClientHandler(), consumerKey, consumerSecret, token, optionalOAuthHeaderParameters)
        {
        }

        public OAuthMessageHandler(HttpMessageHandler innerHandler, string consumerKey, string consumerSecret, Token token = null, IEnumerable<KeyValuePair<string, string>> optionalOAuthHeaderParameters = null)
            : base(innerHandler)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.token = token;
            this.parameters = optionalOAuthHeaderParameters ?? (IEnumerable<KeyValuePair<string, string>>)Enumerable.Empty<KeyValuePair<string, string>>();
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<KeyValuePair<string, string>> enumerable = this.parameters;
            if (request.Method == HttpMethod.Post && request.Content is FormUrlEncodedContent)
            {
                IEnumerable<KeyValuePair<string, string>> enumerable2 = Utility.ParseQueryString(await request.Content.ReadAsStringAsync().ConfigureAwait(false), true);
                enumerable = Enumerable.Concat<KeyValuePair<string, string>>(enumerable, enumerable2);
                request.Content = new OAuthMessageHandler.FormUrlEncodedContentEx(enumerable2);
            }
            IEnumerable<KeyValuePair<string, string>> arg_16E_0 = Enumerable.Concat<KeyValuePair<string, string>>(OAuthUtility.BuildBasicParameters(this.consumerKey, this.consumerSecret, request.RequestUri.OriginalString, request.Method, this.token, enumerable), this.parameters);
            Func<KeyValuePair<string, string>, string> arg_16E_1 = new Func<KeyValuePair<string, string>, string>((p) => { return p.Key + "=" + p.Value.Wrap("\""); });

            string parameter = Enumerable.Select<KeyValuePair<string, string>, string>(arg_16E_0, arg_16E_1).ToString(",");
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", parameter);
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
                IEnumerator<KeyValuePair<string, string>> enumerator = nameValueCollection.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, string> current = enumerator.Current;
                        if (stringBuilder.Length > 0)
                            stringBuilder.Append('&');
                        stringBuilder.Append(OAuthMessageHandler.FormUrlEncodedContentEx.Encode(current.Key));
                        stringBuilder.Append('=');
                        stringBuilder.Append(OAuthMessageHandler.FormUrlEncodedContentEx.Encode(current.Value));
                    }
                }
                finally
                {
                    if (enumerator != null)
                        enumerator.Dispose();
                }
                return Encoding.UTF8.GetBytes((stringBuilder).ToString());
            }

            private static string Encode(string data)
            {
                if (string.IsNullOrEmpty(data))
                    return string.Empty;
                return ((string)data.UrlEncode()).Replace("%20", "+");
            }
        }
    }
}
