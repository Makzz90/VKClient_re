using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public interface IMediaHorizontalItemsViewModel
  {
    string Title { get; }

    int Count { get; }

    bool CanDisplay { get; }

    double ContainerWidth { get; }

    double ContainerHeight { get; }

    Thickness ContainerMargin { get; }

    bool IsAllItemsVisible { get; }

    Action HeaderTapAction { get; }

    Action<MediaListItemViewModelBase> ItemTapAction { get; }

    void Init(IProfileData profileData);

    void Unload();

    void Reload();

    void LoadMoreItems(object linkedItem);
  }
}
