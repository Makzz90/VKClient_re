namespace VKClient.Common.Library
{
  public class BGType
  {
    public int id { get; set; }

    public string name { get; set; }

    public override string ToString()
    {
      return this.name;
    }
  }
}
