using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Framework
{
  public static class ResultCodeExtensions
  {
    public static string GetErrorDescription(this ResultCode resultCode)
    {
      string str = CommonResources.SignUp_Error_Unknown;
      if (resultCode <= ResultCode.AccessDenied)
      {
        if (resultCode != ResultCode.CommunicationFailed)
        {
          if (resultCode == ResultCode.AccessDenied)
            str = CommonResources.Error_AccessDenied;
        }
        else
          str = CommonResources.FailedToConnectError;
      }
      else if (resultCode != ResultCode.ProductNotFound)
      {
        if (resultCode == ResultCode.VideoNotFound)
          str = CommonResources.CannotLoadVideo;
      }
      else
        str = CommonResources.CannotLoadProduct;
      return str;
    }
  }
}
