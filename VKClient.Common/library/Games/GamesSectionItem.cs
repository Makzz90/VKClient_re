namespace VKClient.Common.Library.Games
{
  public abstract class GamesSectionItem
  {
    public GamesSectionType Type { get; private set; }

    public object Data { get; set; }

    protected GamesSectionItem(GamesSectionType type)
    {
      this.Type = type;
    }
  }
}
