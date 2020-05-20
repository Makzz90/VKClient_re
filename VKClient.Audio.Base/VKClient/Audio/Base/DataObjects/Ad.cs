using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class Ad
  {
    public string type { get; set; }

    public string age_restriction { get; set; }

    public string ad_data { get; set; }

    public string ad_data_impression { get; set; }

    public long time_to_live { get; set; }

    public WallPost post { get; set; }

    public List<AdStatistics> statistics { get; set; }

    public Ad()
    {
      this.statistics = new List<AdStatistics>();
    }
  }
}
