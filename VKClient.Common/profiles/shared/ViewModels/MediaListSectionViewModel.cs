using System;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class MediaListSectionViewModel : ViewModelBase
  {
    private const double CONTENT_LEFT_RIGHT_MARGIN = 8.0;
    private const double CONTAINER_MIN_HEIGHT = 60.0;

    public string Title { get; private set; }

    public string TitleCounter { get; private set; }

    public MediaListItemViewModelBase ListItemViewModel { get; private set; }

    public Action TapAction { get; private set; }

    public double ContainerWidth
    {
      get
      {
        return this.ListItemViewModel.ContainerWidth + 16.0;
      }
    }

    public double ContainerHeight
    {
      get
      {
        return this.ListItemViewModel.ContainerHeight + 60.0;
      }
    }

    public double ContainerDelta
    {
      get
      {
        return (this.ContainerWidth - this.ContainerHeight) / 2.0;
      }
    }

    public MediaListSectionViewModel(string title, int titleCounter, MediaListItemViewModelBase listItemViewModel, Action tapAction)
    {
      this.Title = title;
      this.TitleCounter = UIStringFormatterHelper.FormatForUIVeryShort((long) titleCounter);
      this.ListItemViewModel = listItemViewModel;
      this.TapAction = tapAction;
    }
  }
}
