using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Photos.Library
{
  public class PhotoAlbumTemplateSelector : DataTemplateSelector
  {
    public DataTemplate SystemAlbumTemplate { get; set; }

    public DataTemplate UserAlbumTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      AlbumHeader albumHeader = item as AlbumHeader;
      if (albumHeader != null && albumHeader.AlbumType == AlbumType.NormalAlbum)
        return this.UserAlbumTemplate;
      return this.SystemAlbumTemplate;
    }
  }
}
