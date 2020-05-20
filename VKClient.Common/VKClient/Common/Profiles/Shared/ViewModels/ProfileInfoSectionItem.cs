using System.Collections.Generic;
using System.Windows;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileInfoSectionItem
  {
    public string Title { get; set; }

    public List<ProfileInfoItem> Items { get; set; }

    public Visibility TitleVisibility
    {
      get
      {
        return !string.IsNullOrEmpty(this.Title) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility DividerVisibility { get; set; }

    public ProfileInfoSectionItem(string title)
    {
      this.Title = title;
    }

    public ProfileInfoSectionItem()
    {
    }
  }
}
