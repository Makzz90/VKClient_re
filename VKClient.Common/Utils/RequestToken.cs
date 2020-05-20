using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VKClient.Common.Utils
{
  [DataContract]
  public class RequestToken : Token
  {
    [Obsolete("this is used for serialize")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public RequestToken()
    {
    }

    public RequestToken(string key, string secret)
      : base(key, secret)
    {
    }
  }
}
