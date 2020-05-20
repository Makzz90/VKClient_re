namespace VKClient.Common.Shared
{
  public class HeaderOption
  {
    public long ID { get; set; }

    public string Name { get; set; }

    public override string ToString()
    {
      return this.Name;
    }
  }
}
