namespace VKClient.Common.Library
{
  public interface ISearchableItemHeader<T>
  {
    bool IsLocalItem { get; }

    bool Matches(string searchString);
  }
}
