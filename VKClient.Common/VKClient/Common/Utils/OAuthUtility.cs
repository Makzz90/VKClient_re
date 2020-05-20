using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

//https://github.com/neuecc/AsyncOAuth/blob/master/AsyncOAuth/OAuthUtility.cs

namespace VKClient.Common.Utils
{
    public static class OAuthUtility
    {
        private static readonly Random random = new Random();

        public static OAuthUtility.HashFunction ComputeHash { private get; set; }

        private static string GenerateSignature(string consumerSecret, Uri uri, HttpMethod method, Token token, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (ComputeHash == null)
            {
                throw new InvalidOperationException("ComputeHash is null, must initialize before call OAuthUtility.HashFunction = /* your computeHash code */ at once.");
            }

            var hmacKeyBase = consumerSecret.UrlEncode() + "&" + ((token == null) ? "" : token.Secret).UrlEncode();

            // escaped => unescaped[]
            var queryParams = Utility.ParseQueryString(uri.GetComponents(UriComponents.Query | UriComponents.KeepDelimiter, UriFormat.UriEscaped));

            var stringParameter = parameters
                .Where(x => x.Key.ToLower() != "realm")
                .Concat(queryParams)
                .Select(p => new { Key = p.Key.UrlEncode(), Value = p.Value.UrlEncode() })
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .ThenBy(p => p.Value, StringComparer.Ordinal)
                .Select(p => p.Key + "=" + p.Value)
                .ToString("&");
            var signatureBase = method.ToString() +
                "&" + uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped).UrlEncode() +
                "&" + stringParameter.UrlEncode();

            var hash = ComputeHash(Encoding.UTF8.GetBytes(hmacKeyBase), Encoding.UTF8.GetBytes(signatureBase));
            return Convert.ToBase64String(hash).UrlEncode();
        }

        public static IEnumerable<KeyValuePair<string, string>> BuildBasicParameters(string consumerKey, string consumerSecret, string url, HttpMethod method, Token token = null, IEnumerable<KeyValuePair<string, string>> optionalParameters = null)
        {
            Precondition.NotNull((object)url, "url", "");
            List<KeyValuePair<string, string>> first = new List<KeyValuePair<string, string>>(7)
              {
                new KeyValuePair<string, string>("oauth_consumer_key", consumerKey),
                new KeyValuePair<string, string>("oauth_nonce", OAuthUtility.random.Next().ToString()),
                new KeyValuePair<string, string>("oauth_timestamp", DateTime.UtcNow.ToUnixTime().ToString()),
                new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"),
                new KeyValuePair<string, string>("oauth_version", "1.0")
              };
            if (token != null)
                first.Add(new KeyValuePair<string, string>("oauth_token", token.Key));
            if (optionalParameters == null)
                optionalParameters = Enumerable.Empty<KeyValuePair<string, string>>();
            string signature = OAuthUtility.GenerateSignature(consumerSecret, new Uri(url), method, token, first.Concat<KeyValuePair<string, string>>(optionalParameters));
            first.Add(new KeyValuePair<string, string>("oauth_signature", signature));
            return (IEnumerable<KeyValuePair<string, string>>)first;
        }

        public static HttpClient CreateOAuthClient(string consumerKey, string consumerSecret, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> optionalOAuthHeaderParameters = null)
        {
            return new HttpClient((HttpMessageHandler)new OAuthMessageHandler(consumerKey, consumerSecret, (Token)accessToken, optionalOAuthHeaderParameters));
        }

        public static HttpClient CreateOAuthClient(HttpMessageHandler innerHandler, string consumerKey, string consumerSecret, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> optionalOAuthHeaderParameters = null)
        {
            return new HttpClient((HttpMessageHandler)new OAuthMessageHandler(innerHandler, consumerKey, consumerSecret, (Token)accessToken, optionalOAuthHeaderParameters));
        }

        public delegate byte[] HashFunction(byte[] key, byte[] buffer);
    }
}
