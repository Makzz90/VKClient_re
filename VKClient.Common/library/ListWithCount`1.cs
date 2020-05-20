namespace VKClient.Common.Library
{
  public class ListWithCount<T>
  {
    public System.Collections.Generic.List<T> List { get; set; }

    public int TotalCount { get; set; }

    public ListWithCount()
    {
      this.List = new System.Collections.Generic.List<T>();
      this.TotalCount = -1;
    }
  }
}
