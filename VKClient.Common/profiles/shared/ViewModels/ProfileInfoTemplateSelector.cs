using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileInfoTemplateSelector : DataTemplateSelector
  {
    public DataTemplate RichTextTemplate { get; set; }

    public DataTemplate PlainTextTemplate { get; set; }

    public DataTemplate FullItemTemplate { get; set; }

    public DataTemplate ContactTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      ProfileInfoItem profileInfoItem = item as ProfileInfoItem;
      if (profileInfoItem == null)
        return  null;
      switch (profileInfoItem.Type)
      {
        case ProfileInfoItemType.RichText:
          return this.RichTextTemplate;
        case ProfileInfoItemType.PlainText:
          return this.PlainTextTemplate;
        case ProfileInfoItemType.Full:
          return this.FullItemTemplate;
        case ProfileInfoItemType.Contact:
          return this.ContactTemplate;
        default:
          return  null;
      }
    }
  }
}
