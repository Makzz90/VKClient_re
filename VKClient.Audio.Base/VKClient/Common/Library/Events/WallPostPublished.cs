using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class WallPostPublished
  {
    public WallPost WallPost { get; set; }

    public bool IsPostponed { get; set; }

    public bool IsSuggested { get; set; }
  }
}
