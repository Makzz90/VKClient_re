using System.Windows;

namespace VKClient.Common.Library
{
  public interface ISearchWallPostsViewModel
  {
    Visibility NewsVisibility { get; }

    Visibility TrendsVisibility { get; }

    int ItemsCount { get; }

    void Search(string query);

    void Refresh();
  }
}
