using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class AllProfilePostsToggleViewModel : ViewModelBase
    {
        private readonly bool _hidePostsUnderline;
        private readonly IProfileData _profileData;
        private bool _isAllPosts;

        public string AllPostsText { get; private set; }

        public Brush AllPostsForeground
        {
            get
            {
                return (Brush)Application.Current.Resources[this._isAllPosts ? "PhoneProfilePostsToggleActiveBrush" : "PhoneProfilePostsToggleInactiveBrush"];
            }
        }

        public Visibility AllPostsUnderlineVisibility
        {
            get
            {
                return (this._isAllPosts && !this._hidePostsUnderline).ToVisiblity();
            }
        }

        public Brush ProfilePostsForeground
        {
            get
            {
                return (Brush)Application.Current.Resources[(!this._isAllPosts ? "PhoneProfilePostsToggleActiveBrush" : "PhoneProfilePostsToggleInactiveBrush")];
            }
        }

        public Visibility ProfilePostsUnderlineVisibility
        {
            get
            {
                return (!this._isAllPosts).ToVisiblity();
            }
        }

        public string ProfilePostsText { get; set; }

        public bool IsAllPosts
        {
            get
            {
                return this._isAllPosts;
            }
            set
            {
                if (this._isAllPosts == value || this.IsLocked)
                    return;
                this._isAllPosts = value;
                this.NotifyPropertyChanged<Brush>((System.Linq.Expressions.Expression<Func<Brush>>)(() => this.AllPostsForeground));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.AllPostsUnderlineVisibility));
                this.NotifyPropertyChanged<Brush>((System.Linq.Expressions.Expression<Func<Brush>>)(() => this.ProfilePostsForeground));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ProfilePostsUnderlineVisibility));
                Action<bool> stateChangedCallback = this.StateChangedCallback;
                if (stateChangedCallback == null)
                    return;
                int num = value ? 1 : 0;
                stateChangedCallback(num != 0);
            }
        }

        public Visibility PostsToggleVisibility { get; private set; }

        public Visibility PostsCountVisibility { get; private set; }

        public bool IsLocked { get; set; }

        public Action<bool> StateChangedCallback { get; set; }

        public AllProfilePostsToggleViewModel(IProfileData profileData, bool isAllPosts = true)
        {
            this.AllPostsText = CommonResources.Group_AllPosts;
            //
            this._profileData = profileData;
            this._isAllPosts = isAllPosts;
            this.PostsCountVisibility = Visibility.Collapsed;
            GroupData profileData1 = this._profileData as GroupData;
            if (profileData1 != null)
            {
                if (profileData1.group != null && (profileData1.group.GroupType == GroupType.PublicPage || !profileData1.group.CanSeeAllPosts))
                {
                    this._hidePostsUnderline = true;
                    this.AllPostsText = UIStringFormatterHelper.FormatNumberOfSomething(profileData1.wallPostsCount, CommonResources.OneWallPostFrm, CommonResources.TwoWallPostsFrm, CommonResources.FiveWallPostsFrm, true, null, false);
                    this.PostsCountVisibility = Visibility.Visible;
                    this.PostsToggleVisibility = Visibility.Collapsed;
                }
                else
                    this.ProfilePostsText = CommonResources.Group_CommunityPosts;
            }
            else
                this.ProfilePostsText = AppGlobalStateManager.Current.LoggedInUserId == this._profileData.Id ? CommonResources.User_MyPosts : string.Format(CommonResources.User_ProfilePostsFrm, this._profileData.NameGen);
        }

        public void NavigateToSearch()
        {
            Navigator.Current.NavigateToPostsSearch(this._profileData is GroupData ? -this._profileData.Id : this._profileData.Id, this._profileData.NameGen);
        }
    }
}
