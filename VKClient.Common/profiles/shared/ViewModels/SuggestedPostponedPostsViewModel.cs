using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class SuggestedPostponedPostsViewModel : ViewModelBase, IHandle<WallPostPostponed>, IHandle, IHandle<WallPostSuggested>, IHandle<WallPostPublished>, IHandle<WallPostPostponedPublished>, IHandle<WallPostDeleted>
  {
    private readonly long _ownerId;
    private int _suggestedPostsCount;
    private int _postponedPostsCount;

    public Action UpdatedCallback { get; set; }

    public bool CanDisplay
    {
      get
      {
          if (this.SuggestedVisibility != Visibility.Visible)
          return this.PostponedVisibility == 0;
        return true;
      }
    }

    public Visibility SuggestedVisibility
    {
      get
      {
        if (this._suggestedPostsCount <= 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility PostponedVisibility
    {
      get
      {
        if (this._postponedPostsCount <= 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SeparatorVisibility
    {
      get
      {
          if (this.SuggestedVisibility != Visibility.Visible || this.PostponedVisibility != Visibility.Visible)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public int SuggestedPostsCount
    {
      get
      {
        return this._suggestedPostsCount;
      }
      set
      {
        this._suggestedPostsCount = value;
        this.UpdateProperties();
      }
    }

    public int PostponedPostsCount
    {
      get
      {
        return this._postponedPostsCount;
      }
      set
      {
        this._postponedPostsCount = value;
        this.UpdateProperties();
      }
    }

    public string SuggestedPostsStr
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._suggestedPostsCount, CommonResources.SuggestedNews_OneSuggestedPostFrm, CommonResources.SuggestedNews_TwoFourSuggestedPostsFrm, CommonResources.SuggestedNews_FiveSuggestedPostsFrm, false,  null, false);
      }
    }

    public string PostponedPostsStr
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._postponedPostsCount, CommonResources.PostponedNews_OnePostponedPostFrm, CommonResources.PostponedNews_TwoFourPostponedPostsFrm, CommonResources.PostponedNews_FivePostponedPostsFrm, false,  null, false);
      }
    }

    public SuggestedPostponedPostsViewModel(IProfileData profileData)
    {
      this._ownerId = profileData is UserData ? profileData.Id : -profileData.Id;
      this._suggestedPostsCount = profileData.suggestedPostsCount;
      this._postponedPostsCount = profileData.postponedPostsCount;
      EventAggregator.Current.Subscribe(this);
    }

    private void UpdateProperties()
    {
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SuggestedVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.PostponedVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SeparatorVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.SuggestedPostsCount);
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.PostponedPostsCount);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.SuggestedPostsStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.PostponedPostsStr);
      if (this.UpdatedCallback == null)
        return;
      this.UpdatedCallback();
    }

    public void OpenSuggestedPostsPage()
    {
      Navigator.Current.NavigateToSuggestedPostponedPostsPage(Math.Abs(this._ownerId), this._ownerId < 0, 0);
    }

    public void OpenPostponedPostsPage()
    {
      Navigator.Current.NavigateToSuggestedPostponedPostsPage(Math.Abs(this._ownerId), this._ownerId < 0, 1);
    }

    public void Handle(WallPostPostponed message)
    {
      if (message.OwnerId != this._ownerId)
        return;
      this.PostponedPostsCount = this.PostponedPostsCount + 1;
    }

    public void Handle(WallPostSuggested message)
    {
      if (message.to_id != this._ownerId)
        return;
      this.SuggestedPostsCount = this.SuggestedPostsCount + 1;
    }

    public void Handle(WallPostPublished message)
    {
      if (message.WallPost.to_id != this._ownerId)
        return;
      if (message.IsSuggested)
        this.SuggestedPostsCount = this.SuggestedPostsCount - 1;
      if (!message.IsPostponed)
        return;
      this.PostponedPostsCount = this.PostponedPostsCount + 1;
    }

    public void Handle(WallPostPostponedPublished message)
    {
      if (message.WallPost.owner_id != this._ownerId)
        return;
      this.PostponedPostsCount = this.PostponedPostsCount - 1;
    }

    public void Handle(WallPostDeleted message)
    {
      if (message.WallPost.owner_id != this._ownerId)
        return;
      if (message.WallPost.post_type == "suggest")
      {
        this.SuggestedPostsCount = this.SuggestedPostsCount - 1;
      }
      else
      {
        if (!message.WallPost.IsPostponed)
          return;
        this.PostponedPostsCount = this.PostponedPostsCount - 1;
      }
    }
  }
}
