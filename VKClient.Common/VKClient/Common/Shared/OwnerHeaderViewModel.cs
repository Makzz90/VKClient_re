using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Shared
{
  public class OwnerHeaderViewModel : ViewModelBase
  {
    private string _title = "";
    private string _subtitle = "";
    private string _imageUri = "";
    private long _ownerId;

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
      }
    }

    public string Subtitle
    {
      get
      {
        return this._subtitle;
      }
      set
      {
        this._subtitle = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Subtitle));
      }
    }

    public string ImageUri
    {
      get
      {
        return this._imageUri;
      }
      set
      {
        this._imageUri = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.ImageUri));
      }
    }

    public OwnerHeaderViewModel(User u)
    {
      this.InitilizeWithUser(u);
    }

    public OwnerHeaderViewModel(Group g)
    {
      this.InitializeWithGroup(g);
    }

    public OwnerHeaderViewModel(long ownerId, List<User> knownUsers, List<Group> knownGroups)
    {
      if (ownerId > 0L)
        this.InitilizeWithUser(knownUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.id == ownerId)));
      else
        this.InitializeWithGroup(knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -ownerId)));
    }

    public void HandleTap()
    {
      if (this._ownerId > 0L)
        Navigator.Current.NavigateToUserProfile(this._ownerId, this.Title, "", false);
      else
        Navigator.Current.NavigateToGroup(-this._ownerId, this.Title, false);
    }

    private void InitializeWithGroup(Group g)
    {
      if (g == null)
        return;
      this._ownerId = -g.id;
      this.Title = g.name;
      this.Subtitle = UIStringFormatterHelper.FormatNumberOfSomething(g.members_count, CommonResources.OneFollowerFrm, CommonResources.TwoFourFollowersFrm, CommonResources.FiveFollowersFrm, true, null, false);
      this.ImageUri = g.photo_200;
    }

    private void InitilizeWithUser(User u)
    {
      if (u == null)
        return;
      this._ownerId = u.id;
      this.Title = u.Name;
      this.Subtitle = "";
      this.ImageUri = u.photo_max;
    }
  }
}
