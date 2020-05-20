using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GameLeaderboardItemHeader
  {
    private readonly GameLeaderboardItem _leaderboardItem;
    private readonly User _user;
    private readonly int _leaderboardType;
    private readonly int _position;

    public long UserId
    {
      get
      {
        if (this._user == null)
          return 0;
        return this._user.id;
      }
    }

    public string UserName
    {
      get
      {
        if (this._user == null)
          return "";
        return this._user.Name;
      }
    }

    public string UserIcon
    {
      get
      {
        if (this._user == null)
          return "";
        return this._user.photo_max;
      }
    }

    public int Position
    {
      get
      {
        return this._position;
      }
    }

    public string PointsStr
    {
      get
      {
        int result;
        if (this._leaderboardItem == null || !int.TryParse(this._leaderboardItem.points, out result))
          return "";
        return string.Format("{0} {1}", (object) result, (object) UIStringFormatterHelper.FormatNumberOfSomething(result, CommonResources.Games_OnePoint, CommonResources.Games_TwoFourPoints, CommonResources.Games_FivePoints, true, null, false));
      }
    }

    public string LevelStr
    {
      get
      {
        if (this._leaderboardItem == null)
          return "";
        return this._leaderboardItem.level.ToString() + " " + CommonResources.Games_Leaderboard_Level;
      }
    }

    public Visibility PointsStrVisibility
    {
      get
      {
        return this._leaderboardType != 2 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility LevelStrVisibility
    {
      get
      {
        return this._leaderboardType != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility OwnVisibility
    {
      get
      {
        return this._user == null || this._user.id != AppGlobalStateManager.Current.LoggedInUserId ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public GameLeaderboardItemHeader(GameLeaderboardItem leaderboardItem, User user, int leaderboardType, int position)
    {
      this._leaderboardItem = leaderboardItem;
      this._user = user;
      this._leaderboardType = leaderboardType;
      this._position = position;
    }
  }
}
