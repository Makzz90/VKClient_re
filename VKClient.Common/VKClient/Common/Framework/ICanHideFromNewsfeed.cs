using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Framework
{
  public interface ICanHideFromNewsfeed
  {
    NewsFeedIgnoreItemData GetIgnoreItemData();
  }
}
