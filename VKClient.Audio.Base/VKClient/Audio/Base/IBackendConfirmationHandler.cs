using System;

namespace VKClient.Audio.Base
{
  public interface IBackendConfirmationHandler
  {
    void Confirm(string confirmationText, Action<bool> callback);
  }
}
