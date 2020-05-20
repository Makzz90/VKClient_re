namespace VKClient.Common.Library
{
  public interface IRemovableWithText
  {
    string Text { get; set; }

    void Remove();
  }
}
