using System;
using VKClient.Common.Backend.DataObjects;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace VKClient.Audio.Base
{
  public interface IAppStateInfo
  {
    StartState StartState { get; }

    ShareOperation ShareOperation { get; set; }

    void ReportException(Exception exc);

    void HandleSuccessfulLogin(AutorizationData logInInfo, bool navigate = true);
  }
}
