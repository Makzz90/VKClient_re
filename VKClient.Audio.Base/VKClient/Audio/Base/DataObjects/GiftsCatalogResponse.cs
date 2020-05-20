using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftsCatalogResponse
  {
    public int balance { get; set; }

    public List<GiftsSection> catalog { get; set; }
  }
}
