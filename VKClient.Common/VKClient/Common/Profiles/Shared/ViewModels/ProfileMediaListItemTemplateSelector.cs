using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileMediaListItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate GenericTemplate { get; set; }

    public DataTemplate PhotoAlbumTemplate { get; set; }

    public DataTemplate VideoAlbumTemplate { get; set; }

    public DataTemplate SubscriptionsTemplate { get; set; }

    public DataTemplate PhotoTemplate { get; set; }

    public DataTemplate VideoTemplate { get; set; }

    public DataTemplate AudioTemplate { get; set; }

    public DataTemplate DiscussionsTemplate { get; set; }

    public DataTemplate ProductTemplate { get; set; }

    public DataTemplate EmptyDataTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      MediaListItemViewModelBase itemViewModelBase = item as MediaListItemViewModelBase;
      if (itemViewModelBase == null)
        return (DataTemplate) null;
      switch (itemViewModelBase.Type)
      {
        case ProfileMediaListItemType.Generic:
          return this.GenericTemplate;
        case ProfileMediaListItemType.PhotoAlbum:
          return this.PhotoAlbumTemplate;
        case ProfileMediaListItemType.VideoAlbum:
          return this.VideoAlbumTemplate;
        case ProfileMediaListItemType.Subscriptions:
          return this.SubscriptionsTemplate;
        case ProfileMediaListItemType.Photo:
          return this.PhotoTemplate;
        case ProfileMediaListItemType.Video:
          return this.VideoTemplate;
        case ProfileMediaListItemType.Audio:
          return this.AudioTemplate;
        case ProfileMediaListItemType.Discussions:
          return this.DiscussionsTemplate;
        case ProfileMediaListItemType.Product:
          return this.ProductTemplate;
        case ProfileMediaListItemType.EmptyData:
          return this.EmptyDataTemplate;
        default:
          return (DataTemplate) null;
      }
    }
  }
}
