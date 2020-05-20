using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VKClient.Common.Utils
{
  [DataContract]
  public class AccessToken : Token
  {
    [Obsolete("this is used for serialize")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AccessToken()
    {
    }

    public AccessToken(string key, string secret)
      : base(key, secret)
    {
    }
  }
}
