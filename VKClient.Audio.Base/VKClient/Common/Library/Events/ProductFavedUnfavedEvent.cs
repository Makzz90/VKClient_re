using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class ProductFavedUnfavedEvent
  {
    public Product Product { get; set; }

    public bool IsFaved { get; set; }
  }
}
