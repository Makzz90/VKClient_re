using System;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Groups.Management.Library
{
  public sealed class MainViewModel : ViewModelBase
  {
    private bool _isAdministrator;

    public long Id { get; private set; }

    public GroupType Type { get; private set; }

    public Visibility RequestsVisibility
    {
      get
      {
        if (this.Type != GroupType.Group)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility InvitationsVisibility
    {
      get
      {
        if (this.Type == GroupType.PublicPage)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility AdministrationSectionsVisibility
    {
      get
      {
        return this._isAdministrator.ToVisiblity();
      }
    }

    public string MembersTitle
    {
      get
      {
        if (this.Type == GroupType.PublicPage)
          return CommonResources.Management_Followers;
        return CommonResources.Management_Members;
      }
    }

    public MainViewModel(long id, GroupType type, bool isAdministrator)
    {
      this.Id = id;
      this.Type = type;
      this._isAdministrator = isAdministrator;
      this.NotifyPropertyChanged<Visibility>((() => this.AdministrationSectionsVisibility));
    }
  }
}
