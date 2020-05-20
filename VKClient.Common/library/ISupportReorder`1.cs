namespace VKClient.Common.Library
{
  public interface ISupportReorder<T> where T : class
  {
    void Reordered(T item, T before, T after);
  }
}
