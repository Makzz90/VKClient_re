using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using VKClient.Common.Library;

namespace VKClient
{
  public class VKTelemetryInitializer : ITelemetryInitializer
  {
    public void Initialize(ITelemetry telemetry)
    {
      ISupportProperties supportProperties = telemetry as ISupportProperties;
      if (supportProperties == null)
        return;
      supportProperties.Properties["VK_User_ID"] = AppGlobalStateManager.Current.LoggedInUserId.ToString();
    }
  }
}
