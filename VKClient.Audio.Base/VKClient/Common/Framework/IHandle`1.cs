namespace VKClient.Common.Framework
{
  public interface IHandle<TMessage> : IHandle
  {
    void Handle(TMessage message);
  }
}
