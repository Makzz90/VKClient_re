namespace VKClient.Common.Backend.DataObjects
{
  public class FeedbackCopyInfo
  {
    public long owner_id
    {
      get
      {
        return this.from_id;
      }
      set
      {
        this.from_id = value;
      }
    }

    public long from_id { get; set; }

    public long id { get; set; }
  }
}
