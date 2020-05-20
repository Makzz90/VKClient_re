using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class FriendsRecommendationsList
  {
    public string next_from { get; set; }

    public User[] items { get; set; }
  }
}
