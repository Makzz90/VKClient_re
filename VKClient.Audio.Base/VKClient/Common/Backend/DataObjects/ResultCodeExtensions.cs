namespace VKClient.Common.Backend.DataObjects
{
  public static class ResultCodeExtensions
  {
    public static bool IsCancelResultCode(this ResultCode resultCode)
    {
      return resultCode == ResultCode.ConfirmationCancelled || resultCode == ResultCode.BalanceRefillCancelled;
    }
  }
}
