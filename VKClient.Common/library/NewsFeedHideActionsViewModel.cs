namespace VKClient.Common.Library
{
  public class NewsFeedHideActionsViewModel
  {
    private readonly string _type;
    private readonly long _ownerId;
    private readonly long _itemId;

    public NewsFeedHideActionsViewModel(string type, long ownerId, long itemId)
    {
      this._type = type;
      this._ownerId = ownerId;
      this._itemId = itemId;
    }

    public void Cancel()
    {
    }
  }
}
