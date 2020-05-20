using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace VKClient.Audio.Base.Utils
{
  public static class HttpExtensions
  {
    public static async Task<Stream> TryGetResponseStreamAsync(string uri)
    {
      try
      {
        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
        string get = HttpMethod.Get;
        httpWebRequest.Method = get;
        return (await httpWebRequest.GetResponseAsync()).GetResponseStream();
      }
      catch (Exception )
      {
      }
      return  null;
    }

    public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
    {
      TaskCompletionSource<HttpWebResponse> taskComplete = new TaskCompletionSource<HttpWebResponse>();
      request.BeginGetResponse((AsyncCallback) (asyncResponse =>
      {
        try
        {
          taskComplete.TrySetResult((HttpWebResponse) ((WebRequest) asyncResponse.AsyncState).EndGetResponse(asyncResponse));
        }
        catch (WebException ex)
        {
          taskComplete.TrySetResult((HttpWebResponse) ex.Response);
        }
      }), request);
      return taskComplete.Task;
    }
  }
}
