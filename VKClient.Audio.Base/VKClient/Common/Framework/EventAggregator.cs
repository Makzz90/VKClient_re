using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
    public class EventAggregator : IEventAggregator
    {
        public static Action<Action> DefaultPublicationThreadMarshaller = (Action<Action>)(action => Execute.ExecuteOnUIThread(action));
        private readonly List<EventAggregator.Handler> handlers = new List<EventAggregator.Handler>();
        private static EventAggregator _current;

        public static EventAggregator Current
        {
            get
            {
                if (EventAggregator._current == null)
                    EventAggregator._current = new EventAggregator();
                return EventAggregator._current;
            }
        }

        public Action<Action> PublicationThreadMarshaller { get; set; }

        public EventAggregator()
        {
            this.PublicationThreadMarshaller = EventAggregator.DefaultPublicationThreadMarshaller;
        }

        public virtual void Subscribe(object instance)
        {
            lock (this.handlers)
            {
                if (this.handlers.Any<EventAggregator.Handler>((Func<EventAggregator.Handler, bool>)(x => x.Matches(instance))))
                    return;
                this.handlers.Add(new EventAggregator.Handler(instance));
            }
        }

        public virtual void Unsubscribe(object instance)
        {
            lock (this.handlers)
            {
                EventAggregator.Handler handler = this.handlers.FirstOrDefault<EventAggregator.Handler>((Func<EventAggregator.Handler, bool>)(x => x.Matches(instance)));
                if (handler == null)
                    return;
                this.handlers.Remove(handler);
            }
        }

        public virtual void Publish(object message)
        {
            this.Publish(message, this.PublicationThreadMarshaller);
        }

        public virtual void Publish(object message, Action<Action> marshal)
        {
            EventAggregator.Handler[] toNotify;
            lock (this.handlers)
                toNotify = this.handlers.ToArray();
            marshal((Action)(() =>
            {
                Type messageType = message.GetType();
                List<EventAggregator.Handler> list = ((IEnumerable<EventAggregator.Handler>)toNotify).Where<EventAggregator.Handler>((Func<EventAggregator.Handler, bool>)(handler => !handler.Handle(messageType, message))).ToList<EventAggregator.Handler>();
                if (!list.Any<EventAggregator.Handler>())
                    return;
                lock (this.handlers)
                    list.Apply<EventAggregator.Handler>((Action<EventAggregator.Handler>)(x => this.handlers.Remove(x)));
            }));
        }

        protected class Handler
        {
            private readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();
            private readonly WeakReference reference;

            public Handler(object handler)
            {
                this.reference = new WeakReference(handler);
                foreach (Type type in ((IEnumerable<Type>)handler.GetType().GetInterfaces()).Where<Type>((Func<Type, bool>)(x =>
                {
                    if (typeof(IHandle).IsAssignableFrom(x))
                        return x.IsGenericType;
                    return false;
                })))
                {
                    Type genericArgument = type.GetGenericArguments()[0];
                    string name = "Handle";
                    MethodInfo method = type.GetMethod(name);
                    this.supportedHandlers[genericArgument] = method;
                }
            }

            public bool Matches(object instance)
            {
                return this.reference.Target == instance;
            }

            public bool Handle(Type messageType, object message)
            {
                object target = this.reference.Target;
                if (target == null)
                    return false;
                foreach (KeyValuePair<Type, MethodInfo> supportedHandler in this.supportedHandlers)
                {
                    if (supportedHandler.Key.IsAssignableFrom(messageType))
                    {
                        supportedHandler.Value.Invoke(target, new object[1]
            {
              message
            });
                        return true;
                    }
                }
                return true;
            }
        }
    }
}
