using VKClient.Common.Backend;

namespace VKClient.Audio.Base.Events
{
  public class ObjectLikedUnlikedEvent
  {
    public bool Liked { get; set; }

    public long OwnerId { get; set; }

    public long ItemId { get; set; }

    public LikeObjectType LikeObjType { get; set; }
  }
}
