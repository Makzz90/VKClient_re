using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class MediaListItemViewModelBase : ViewModelBase
  {
    private const double CONTAINER_WIDTH_DEFAULT = 182.0;
    private const double CONTAINER_HEIGHT_DEFAULT = 120.0;

    public ProfileMediaListItemType Type { get; private set; }

    public abstract string Id { get; }

    public virtual double ContainerWidth
    {
      get
      {
        return 182.0;
      }
    }

    public virtual double ContainerHeight
    {
      get
      {
        return 120.0;
      }
    }

    public double ContainerDelta
    {
      get
      {
        return (this.ContainerWidth - this.ContainerHeight) / 2.0;
      }
    }

    public virtual Thickness ItemMargin
    {
      get
      {
        return new Thickness(0.0);
      }
    }

    protected MediaListItemViewModelBase(ProfileMediaListItemType type)
    {
      this.Type = type;
    }
  }
}
