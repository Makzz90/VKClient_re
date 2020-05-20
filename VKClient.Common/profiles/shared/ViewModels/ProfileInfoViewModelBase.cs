using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Profiles.Shared.Views;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileInfoViewModelBase : ViewModelBase
  {
    protected const string ICONS_PATH_ENDPOINT = "/Resources/Profile/";

    public virtual List<InfoListItem> InfoItems
    {
      get
      {
        if (DesignerProperties.IsInDesignTool)
          return this.GetDesignData();
        return  null;
      }
    }

    public virtual Visibility WikiPageVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public virtual Visibility LinkVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public virtual string WikiPageName
    {
      get
      {
        return "";
      }
    }

    public virtual string Link
    {
      get
      {
        return "";
      }
    }

    private List<InfoListItem> GetDesignData()
    {
      return new List<InfoListItem>() { new InfoListItem() { IconUrl = "/Resources/Profile/ProfileStatus.png", Text = "May my enemies live long so they can see p..." }, new InfoListItem() { IconUrl = "/Resources/Profile/ProfileFriends.png", Text = "680 друзей · 5 общих" }, new InfoListItem() { IconUrl = "/Resources/Profile/ProfileFollowers.png", Text = "1 389 подписчиков" }, new InfoListItem() { IconUrl = "/Resources/Profile/ProfileBirthday.png", Text = "День рождения: 25 декабря" }, new InfoListItem() { IconUrl = "/Resources/Profile/ProfileHome.png", Text = "Город: Санкт-Петербург" }, new InfoListItem() { IconUrl = "/Resources/Profile/ProfileWork.png", Text = "Команда ВКонтакте" } };
    }

    public void ShowFullInfoPopup()
    {
      ProfileInfoFullViewModel fullInfoViewModel = this.GetFullInfoViewModel();
      if (fullInfoViewModel == null)
        return;
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num = 0;
      dialogService.HideOnNavigation = num != 0;
      FullInfoUC fullInfoUc1 = new FullInfoUC();
      ProfileInfoFullViewModel infoFullViewModel = fullInfoViewModel;
      ((FrameworkElement) fullInfoUc1).DataContext = infoFullViewModel;
      FullInfoUC fullInfoUc2 = fullInfoUc1;
      dialogService.Child = (FrameworkElement) fullInfoUc2;
      // ISSUE: variable of the null type

      dialogService.Show(null);
    }

    protected virtual ProfileInfoFullViewModel GetFullInfoViewModel()
    {
      return  null;
    }

    public virtual void OpenWikiPage()
    {
    }

    public virtual void OpenLink()
    {
    }
  }
}
