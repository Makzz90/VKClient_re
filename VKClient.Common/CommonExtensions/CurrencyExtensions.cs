using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.CommonExtensions
{
  public static class CurrencyExtensions
  {
    public static string GetFormatStr(this Currency currency)
    {
      return currency.id.GetFormatStr();
    }

    public static string GetFormatStr(this int currencyId)
    {
      if (currencyId == Currency.RUB.id)
        return "{0} " + CommonResources.Currency_RUB;
      if (currencyId == Currency.UAH.id)
        return "{0} " + CommonResources.Currency_UAH;
      if (currencyId == Currency.KZT.id)
        return "{0} " + CommonResources.Currency_KZT;
      if (currencyId == Currency.EUR.id)
        return CommonResources.Currency_EUR + "{0}";
      if (currencyId == Currency.USD.id)
        return CommonResources.Currency_USD + "{0}";
      return "{0}";
    }

    public static string GetCurrencyDesc(this string currencyName)
    {
      if (currencyName == Currency.RUB.name)
        return CommonResources.Currency_RUB;
      if (currencyName == Currency.UAH.name)
        return CommonResources.Currency_UAH;
      if (currencyName == Currency.KZT.name)
        return CommonResources.Currency_KZT;
      if (currencyName == Currency.EUR.name)
        return CommonResources.Currency_EUR;
      if (currencyName == Currency.USD.name)
        return CommonResources.Currency_USD;
      return currencyName;
    }
  }
}
