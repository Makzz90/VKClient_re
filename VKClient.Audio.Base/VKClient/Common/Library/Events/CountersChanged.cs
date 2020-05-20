using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class CountersChanged
  {
    private OwnCounters _counters;

    public OwnCounters Counters
    {
      get
      {
        return this._counters;
      }
    }

    public CountersChanged(OwnCounters counters)
    {
      this._counters = counters;
    }
  }
}
