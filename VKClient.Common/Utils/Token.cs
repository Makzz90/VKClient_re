using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace VKClient.Common.Utils
{
  [DebuggerDisplay("Key = {Key}, Secret = {Secret}")]
  [DataContract]
  public abstract class Token
  {
    [DataMember(Order = 1)]
    public string Key { get; private set; }

    [DataMember(Order = 2)]
    public string Secret { get; private set; }

    [Obsolete("this is used for serialize")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Token()
    {
    }

    public Token(string key, string secret)
    {
      Precondition.NotNull(key, "key", "");
      Precondition.NotNull(secret, "secret", "");
      this.Key = key;
      this.Secret = secret;
    }
  }
}
