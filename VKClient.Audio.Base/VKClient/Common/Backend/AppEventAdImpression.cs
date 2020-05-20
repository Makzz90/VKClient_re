namespace VKClient.Common.Backend
{
  public class AppEventAdImpression : AppEventBase
  {
    public override string e
    {
      get
      {
        return "";
      }
    }

    public string event_type
    {
      get
      {
        return "impression";
      }
    }

    public string ad_data_impression { get; set; }
  }
}
