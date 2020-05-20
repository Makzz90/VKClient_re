namespace VKClient.Common.Library
{
  public class AccessType
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public override string ToString()
    {
      return this.Name;
    }
  }
}
