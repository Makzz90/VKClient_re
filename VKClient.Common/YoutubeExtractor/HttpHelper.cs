using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    internal static class HttpHelper
    {
        public static string DownloadString(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Headers["User-Agent"]= "Mozilla/4.0";
            request.Method=("GET");
            Task<WebResponse> arg_84_0 = Task.Factory.FromAsync<WebResponse>(new Func<AsyncCallback, object, IAsyncResult>(request.BeginGetResponse), (IAsyncResult asyncResult) => request.EndGetResponse(asyncResult), null);
            Func<Task<WebResponse>, string> arg_84_1 = new Func<Task<WebResponse>, string>((t) => { return HttpHelper.ReadStreamFromResponse(t.Result); });

            return arg_84_0.ContinueWith<string>(arg_84_1).Result;
        }

        public static string HtmlDecode(string value)
        {
            return WebUtility.HtmlDecode(value);
        }

        public static IDictionary<string, string> ParseQueryString(string s)
        {
            if (((string)s).Contains("?"))
                s = ((string)s).Substring(((string)s).IndexOf('?') + 1);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string input in Regex.Split(s, "&"))
            {
                string[] strArray = Regex.Split(input, "=");
                dictionary.Add(strArray[0], strArray.Length == 2 ? HttpHelper.UrlDecode(strArray[1]) : string.Empty);
            }
            return (IDictionary<string, string>)dictionary;
        }

        public static string ReplaceQueryStringParameter(string currentPageUrl, string paramToReplace, string newValue)
        {
            IDictionary<string, string> queryString = HttpHelper.ParseQueryString(currentPageUrl);
            string index = paramToReplace;
            string str = newValue;
            queryString[index] = str;
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            IEnumerator<KeyValuePair<string, string>> enumerator = queryString.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, string> current = enumerator.Current;
                    if (!flag)
                        stringBuilder.Append("&");
                    stringBuilder.Append(current.Key);
                    stringBuilder.Append("=");
                    stringBuilder.Append(current.Value);
                    flag = false;
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
            return (new UriBuilder(currentPageUrl)
            {
                Query = (stringBuilder).ToString()
            }).ToString();
        }

        public static string UrlDecode(string url)
        {
            return WebUtility.UrlDecode(url);
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            try
            {
                StreamReader streamReader = new StreamReader(responseStream);
                try
                {
                    return streamReader.ReadToEnd();
                }
                finally
                {
                    if (streamReader != null)
                        streamReader.Dispose();
                }
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Dispose();
            }
        }
    }
}
