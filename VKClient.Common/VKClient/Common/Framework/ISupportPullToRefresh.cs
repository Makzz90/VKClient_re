using System;

namespace VKClient.Common.Framework
{
  public interface ISupportPullToRefresh
  {
    double PullPercentage { get; }

    Action OnPullPercentageChanged { get; set; }

    Action OnRefresh { get; set; }
  }
}
