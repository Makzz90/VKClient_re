using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public sealed class LinkHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<User>
    {
        private string _title = "";
        private string _description = "";
        private Visibility _blockVisibility;
        private long _id;
        private string _photo;
        private Visibility _addToManagersVisibility;
        private Visibility _editVisibility;
        private Visibility _removeFromCommunityVisibility;

        public bool IsActionButtonEnabled { get; private set; }
        public string ActionButtonIcon { get; private set; }
        public double ActionButtonSize { get; private set; }
        public Action<FrameworkElement> ActionButtonAction { get; private set; }

        public long Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
                base.NotifyPropertyChanged<long>(() => this.Id);
            }
        }

        public string Photo
        {
            get
            {
                return this._photo;
            }
            set
            {
                this._photo = value;
                base.NotifyPropertyChanged<string>(() => this.Photo);
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                base.NotifyPropertyChanged<string>(() => this.Title);
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                base.NotifyPropertyChanged<string>(() => this.Description);
                base.NotifyPropertyChanged<Thickness>(() => this.TitleMargin);
            }
        }

        public Thickness TitleMargin
        {
            get
            {
                return new Thickness(92.0, string.IsNullOrWhiteSpace(this.Description) ? 24.0 : 11.0, 16.0, 0.0);
            }
        }

        public User User { get; set; }

        public GroupLink Link { get; set; }

        public string Url { get; private set; }

        public Visibility ActionButtonVisibility { get; private set; }

        public Visibility AddToManagersVisibility
        {
            get
            {
                return this._addToManagersVisibility;
            }
            set
            {
                this._addToManagersVisibility = value;
                base.NotifyPropertyChanged<Visibility>(() => this.AddToManagersVisibility);
            }
        }

        public Visibility EditVisibility
        {
            get
            {
                return this._editVisibility;
            }
            set
            {
                this._editVisibility = value;
                base.NotifyPropertyChanged<Visibility>(() => this.EditVisibility);
            }
        }

        public Visibility RemoveFromCommunityVisibility
        {
            get
            {
                return this._removeFromCommunityVisibility;
            }
            set
            {
                this._removeFromCommunityVisibility = value;
                base.NotifyPropertyChanged<Visibility>(() => this.RemoveFromCommunityVisibility);
            }
        }

        public Visibility BlockVisibility
        {
            get
            {
                return this._blockVisibility;
            }
            set
            {
                this._blockVisibility = value;
                base.NotifyPropertyChanged<Visibility>(() => this.BlockVisibility);
            }
        }

        public bool IsLocalItem
        {
            get
            {
                return false;
            }
        }

        public LinkHeader(GroupLink link)
        {
            this.Id = link.id;
            this.Photo = link.photo_100;
            this.Title = link.name.ForUI();
            this.Description = link.desc.ForUI();
            this.Link = link;
            this.Url = link.url;
        }

        public LinkHeader(User user)
        {
            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
        }

        public LinkHeader(User user, CommunityManagementRole currentUserRole, bool isManagement)
        {
            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
            this.IsActionButtonEnabled = isManagement && (user.Role == CommunityManagementRole.Unknown || currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator) && (user.id != AppGlobalStateManager.Current.LoggedInUserId || currentUserRole == CommunityManagementRole.Administrator) && user.Role != CommunityManagementRole.Creator;
            this.ActionButtonVisibility = this.IsActionButtonEnabled.ToVisiblity();
            base.NotifyPropertyChanged<bool>(() => this.IsActionButtonEnabled);
            base.NotifyPropertyChanged<Visibility>(() => this.ActionButtonVisibility);
            this.AddToManagersVisibility = (user.Role == CommunityManagementRole.Unknown && (currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator)).ToVisiblity();
            this.EditVisibility = (user.Role != CommunityManagementRole.Unknown && (currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator)).ToVisiblity();
            this.RemoveFromCommunityVisibility = (user.Role == CommunityManagementRole.Unknown || (currentUserRole == CommunityManagementRole.Administrator && user.id != AppGlobalStateManager.Current.LoggedInUserId) || currentUserRole == CommunityManagementRole.Creator).ToVisiblity();
            this.BlockVisibility = (this.EditVisibility == Visibility.Collapsed).ToVisiblity();
            this.UpdateRole(user.Role);
        }

        public LinkHeader(User user, Action<FrameworkElement> actionButtonAction, string icon)
        {
            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
            this.ActionButtonSize = 32.0;
            this.ActionButtonIcon = icon;
            this.ActionButtonAction = actionButtonAction;
            base.NotifyPropertyChanged<double>(() => this.ActionButtonSize);
            base.NotifyPropertyChanged<string>(() => this.ActionButtonIcon);
            base.NotifyPropertyChanged<Action<FrameworkElement>>(() => this.ActionButtonAction);
            this.IsActionButtonEnabled = false;
            base.NotifyPropertyChanged<bool>(() => this.IsActionButtonEnabled);
            this.UpdateRole(user.Role);
            if (user.Role == CommunityManagementRole.Creator)
            {
                this.ActionButtonVisibility = Visibility.Collapsed;
                base.NotifyPropertyChanged<Visibility>(() => this.ActionButtonVisibility);
            }
            if (string.IsNullOrEmpty(this.Description) && user.occupation != null)
            {
                this.Description = user.occupation.name.ForUI();
            }
        }

        public string GetKey()
        {
            return this.Id.ToString();
        }

        public bool Matches(string searchString)
        {
            MatchStrings matchStrings = TransliterationHelper.GetMatchStrings(searchString);
            return this.MatchesAny(matchStrings.SearchStrings, matchStrings.LatinStrings, matchStrings.CyrillicStrings);
        }

        public bool MatchesAny(List<string> searchStrings, List<string> searchStringsLatin, List<string> searchStringsCyrillic)
        {
            if (!this.Matches(searchStrings) && !this.Matches(searchStringsLatin))
                return this.Matches(searchStringsCyrillic);
            return true;
        }

        private bool Matches(List<string> searchStrings)
        {
            // ISSUE: method pointer
            if (searchStrings.Count == 0 || Enumerable.All<string>(searchStrings, (Func<string, bool>)new Func<string, bool>(string.IsNullOrWhiteSpace)))
                return false;
            List<string>.Enumerator enumerator = searchStrings.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    bool flag = false;
                    string title = this.Title;
                    char[] chArray = new char[1] { ' ' };
                    foreach (string str in ((string)title).Split((char[])chArray))
                    {
                        flag = str.StartsWith(current, (StringComparison)3);
                        if (flag)
                            break;
                    }
                    if (!flag)
                        return false;
                }
            }
            finally
            {
                enumerator.Dispose();
            }
            return true;
        }

        public void UpdateRole(CommunityManagementRole role)
        {
            switch (role)
            {
                case CommunityManagementRole.Moderator:
                    this.Description = CommonResources.CommunityManager_Moderator;
                    break;
                case CommunityManagementRole.Editor:
                    this.Description = CommonResources.CommunityManager_Editor;
                    break;
                case CommunityManagementRole.Administrator:
                    this.Description = CommonResources.CommunityManager_Administrator;
                    break;
                case CommunityManagementRole.Creator:
                    this.Description = CommonResources.CommunityManager_Creator;
                    break;
                default:
                    this.Description = "";
                    break;
            }
        }

        public LinkHeader()
        {
            this.IsActionButtonEnabled = true;
            this._blockVisibility = Visibility.Collapsed;
            this.ActionButtonIcon = "/Resources/WallPost/CardActions.png";
            this.ActionButtonSize = 56.0;
            this.ActionButtonAction = (Action<FrameworkElement>)(e => ContextMenuService.GetContextMenu((DependencyObject)e).IsOpen = true);
        }
    }
}
