namespace VKClient.Common.Backend
{
  public class Cancellation
  {
    public bool IsSet { get; private set; }

    public void Set()
    {
      this.IsSet = true;
    }
  }
}
