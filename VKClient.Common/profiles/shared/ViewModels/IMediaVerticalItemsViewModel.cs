using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public interface IMediaVerticalItemsViewModel
  {
    string Title { get; }

    int Count { get; }

    bool CanDisplay { get; }

    List<MediaListItemViewModelBase> Items { get; }

    bool IsAllItemsVisible { get; }

    Action HeaderTapAction { get; }

    Action<MediaListItemViewModelBase> ItemTapAction { get; }

    void Init(IProfileData profileData);
  }
}
