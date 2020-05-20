using System;
using System.Collections.Generic;

namespace VKClient.Audio.Base.Library
{
  public static class ServiceLocator
  {
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
      ServiceLocator._services[typeof (T)] = service;
    }

    public static T Resolve<T>()
    {
      Type key = typeof (T);
      if (!ServiceLocator._services.ContainsKey(key))
        throw new KeyNotFoundException();
      return (T) ServiceLocator._services[key];
    }
  }
}
