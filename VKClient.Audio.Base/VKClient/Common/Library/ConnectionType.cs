namespace VKClient.Common.Library
{
  public class ConnectionType
  {
      public string Type { get; private set; }

      public string Subtype { get; private set; }

    public ConnectionType(string type, string subtype)
    {
      this.Type = type;
      this.Subtype = subtype;
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", this.Type, this.Subtype);
    }
  }
}
