using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Profiles.Groups.ViewModels;
using VKClient.Common.Profiles.Users.ViewModels;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileMainInfoTemplateSelector : DataTemplateSelector
  {
    public DataTemplate UserTemplate { get; set; }

    public DataTemplate GroupTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is UserProfileHeaderViewModel)
        return this.UserTemplate;
      if (item is GroupProfileHeaderViewModel)
        return this.GroupTemplate;
      return  null;
    }
  }
}
