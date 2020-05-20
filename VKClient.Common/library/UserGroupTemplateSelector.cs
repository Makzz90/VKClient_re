using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class UserGroupTemplateSelector : DataTemplateSelector
  {
    public DataTemplate UserTemplate { get; set; }

    public DataTemplate GroupTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      UserGroupHeader userGroupHeader = item as UserGroupHeader;
      if (userGroupHeader != null && userGroupHeader.GroupHeader != null)
        return this.GroupTemplate;
      return this.UserTemplate;
    }
  }
}
