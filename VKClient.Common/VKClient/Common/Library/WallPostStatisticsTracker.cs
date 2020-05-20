using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class WallPostStatisticsTracker
  {
    public static WallPostStatisticsTracker Instance = new WallPostStatisticsTracker();
    private static readonly int THRESHOLD_CNT = 10;
    private List<WallPost> _viewedWallPosts = new List<WallPost>();

    public void ReportViewed(WallPost wallPost)
    {
      if (this._viewedWallPosts.Any<WallPost>((Func<WallPost, bool>) (w =>
      {
        if (w.id == wallPost.id)
          return w.to_id == wallPost.to_id;
        return false;
      })))
        return;
      this._viewedWallPosts.Add(wallPost);
      this.SendStatsIfNeeded();
    }

    private void SendStatsIfNeeded()
    {
      if (this._viewedWallPosts.Count <= WallPostStatisticsTracker.THRESHOLD_CNT)
        return;
      WallService.Current.SendStats(this._viewedWallPosts);
      this._viewedWallPosts = new List<WallPost>();
    }
  }
}
