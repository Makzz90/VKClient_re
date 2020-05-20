using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public static class ShareContentDataProviderManager
  {
    private const string DATA_PROVIDER_KEY = "ShareContentDataProvider";

    public static void StoreDataProvider(IShareContentDataProvider dataProvider)
    {
      ParametersRepository.SetParameterForId("ShareContentDataProvider", dataProvider);
    }

    public static IShareContentDataProvider RetrieveDataProvider()
    {
      return ParametersRepository.GetParameterForIdAndReset("ShareContentDataProvider") as IShareContentDataProvider;
    }
  }
}
