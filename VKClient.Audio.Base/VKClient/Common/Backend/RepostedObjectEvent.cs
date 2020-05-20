namespace VKClient.Common.Backend
{
  public class RepostedObjectEvent
  {
    public long owner_id { get; set; }

    public long obj_id { get; set; }

    public RepostObject rObj { get; set; }

    public RepostResult RepostResult { get; set; }

    public long groupId { get; set; }
  }
}
