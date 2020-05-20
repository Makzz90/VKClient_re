using System;

namespace VKClient.Common.Framework
{
  public interface IEventAggregator
  {
    Action<Action> PublicationThreadMarshaller { get; set; }

    void Subscribe(object instance);

    void Unsubscribe(object instance);

    void Publish(object message);

    void Publish(object message, Action<Action> marshal);
  }
}
