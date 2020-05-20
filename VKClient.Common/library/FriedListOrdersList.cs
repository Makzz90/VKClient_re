using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class FriedListOrdersList
  {
    private static List<BGType> _list;

    public static List<BGType> GetOrders()
    {
      if (FriedListOrdersList._list == null)
      {
        BGType bgType1 = new BGType();
        bgType1.id = 0;
        string lowerInvariant1 = CommonResources.Privacy_General_ByFirstName.ToLowerInvariant();
        bgType1.name = lowerInvariant1;
        BGType bgType2 = bgType1;
        BGType bgType3 = new BGType();
        bgType3.id = 1;
        string lowerInvariant2 = CommonResources.Privacy_General_ByLastName.ToLowerInvariant();
        bgType3.name = lowerInvariant2;
        BGType bgType4 = bgType3;
        FriedListOrdersList._list = new List<BGType>()
        {
          bgType2,
          bgType4
        };
      }
      return FriedListOrdersList._list;
    }
  }
}
