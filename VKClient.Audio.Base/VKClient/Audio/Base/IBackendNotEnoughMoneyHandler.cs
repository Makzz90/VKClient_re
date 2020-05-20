using System;

namespace VKClient.Audio.Base
{
  public interface IBackendNotEnoughMoneyHandler
  {
    void RequestBalanceRefill(Action refilledCallback, Action cancelledCallback);
  }
}
