using VKClient.Audio.Base.Library;

namespace VKClient.Audio.Base.Events
{
  public class SearchParamsUpdated
  {
    public SearchParams SearchParams { get; private set; }

    public SearchParamsUpdated(SearchParams searchParams)
    {
      this.SearchParams = searchParams;
    }
  }
}
