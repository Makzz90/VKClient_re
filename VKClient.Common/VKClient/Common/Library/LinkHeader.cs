using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Visibility _blockVisibility = Visibility.Collapsed;
        private long _id;
        private string _photo;
        private Visibility _addToManagersVisibility;
        private Visibility _editVisibility;
        private Visibility _removeFromCommunityVisibility;

        public bool IsActionButtonEnabled { get; set; }

        public string ActionButtonIcon { get; set; }

        public double ActionButtonSize { get; set; }

        public Action<FrameworkElement> ActionButtonAction { get; set; }

        public long Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
                this.NotifyPropertyChanged<long>((System.Linq.Expressions.Expression<Func<long>>)(() => this.Id));
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
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Photo));
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
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Title));
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
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Description));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.TitleMargin));
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

        public string Url { get; set; }//

        public Visibility ActionButtonVisibility { get; set; }//

        public Visibility AddToManagersVisibility
        {
            get
            {
                return this._addToManagersVisibility;
            }
            set
            {
                this._addToManagersVisibility = value;
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.AddToManagersVisibility));
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
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.EditVisibility));
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
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.RemoveFromCommunityVisibility));
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
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.BlockVisibility));
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
            this.IsActionButtonEnabled = true;
            this._blockVisibility = Visibility.Visible;
            this.ActionButtonIcon = "/Resources/WallPost/CardActions.png";
            this.ActionButtonSize = 56.0;
            Action<FrameworkElement> arg_5E_1 = new Action<FrameworkElement>(e => { ContextMenuService.GetContextMenu(e).IsOpen = true; });
            this.ActionButtonAction = arg_5E_1;

            this.Id = link.id;
            this.Photo = link.photo_100;
            this.Title = link.name.ForUI();
            this.Description = link.desc.ForUI();
            this.Link = link;
            this.Url = link.url;
        }

        public LinkHeader(User user)
        {
            this.IsActionButtonEnabled = true;
            this._blockVisibility = Visibility.Visible;
            this.ActionButtonIcon = "/Resources/WallPost/CardActions.png";
            this.ActionButtonSize = 56.0;
            Action<FrameworkElement> arg_5E_1 = new Action<FrameworkElement>(e => { ContextMenuService.GetContextMenu(e).IsOpen = true; });
            this.ActionButtonAction = arg_5E_1;

            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
        }

        public LinkHeader(User user, CommunityManagementRole currentUserRole, bool isManagement)
        {
            //this.IsActionButtonEnabled = true;
            this._blockVisibility = Visibility.Visible;
            this.ActionButtonIcon = "/Resources/WallPost/CardActions.png";
            this.ActionButtonSize = 56.0;
            Action<FrameworkElement> arg_5E_1 = new Action<FrameworkElement>(e => { ContextMenuService.GetContextMenu(e).IsOpen = true; });
            this.ActionButtonAction = arg_5E_1;

            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
            this.IsActionButtonEnabled = isManagement && (user.Role == CommunityManagementRole.Unknown || currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator) && user.id != AppGlobalStateManager.Current.LoggedInUserId && user.Role != CommunityManagementRole.Creator;
            this.ActionButtonVisibility = this.IsActionButtonEnabled.ToVisiblity();
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsActionButtonEnabled));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ActionButtonVisibility));
            this.AddToManagersVisibility = (user.Role == CommunityManagementRole.Unknown && (currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator)).ToVisiblity();
            this.EditVisibility = (user.Role != CommunityManagementRole.Unknown && (currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator)).ToVisiblity();
            this.RemoveFromCommunityVisibility = (user.Role == CommunityManagementRole.Unknown || currentUserRole == CommunityManagementRole.Administrator || currentUserRole == CommunityManagementRole.Creator).ToVisiblity();
            this.BlockVisibility = (this.EditVisibility == Visibility.Collapsed).ToVisiblity();
            this.UpdateRole(user.Role);
        }

        public LinkHeader(User user, Action<FrameworkElement> actionButtonAction, string icon)
        {
            //this.IsActionButtonEnabled = true;
            this._blockVisibility = Visibility.Visible;
            //this.ActionButtonIcon = "/Resources/WallPost/CardActions.png";
            //this.ActionButtonSize = 56.0;
            Action<FrameworkElement> arg_5E_1 = new Action<FrameworkElement>(e => { ContextMenuService.GetContextMenu(e).IsOpen = true; });
            this.ActionButtonAction = arg_5E_1;

            this.Id = user.id;
            this.Photo = user.photo_max;
            this.Title = user.Name;
            this.User = user;
            this.ActionButtonSize = 32.0;
            this.ActionButtonIcon = icon;
            this.ActionButtonAction = actionButtonAction;
            this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.ActionButtonSize));
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ActionButtonIcon));
            this.NotifyPropertyChanged<Action<FrameworkElement>>((System.Linq.Expressions.Expression<Func<Action<FrameworkElement>>>)(() => this.ActionButtonAction));
            this.IsActionButtonEnabled = false;
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsActionButtonEnabled));
            this.UpdateRole(user.Role);
            if (user.Role == CommunityManagementRole.Creator || user.id == AppGlobalStateManager.Current.LoggedInUserId && user.Role == CommunityManagementRole.Administrator)
            {
                this.ActionButtonVisibility = Visibility.Collapsed;
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ActionButtonVisibility));
            }
            if (!string.IsNullOrEmpty(this.Description) || user.occupation == null)
                return;
            this.Description = user.occupation.name.ForUI();
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
            if (searchStrings.Count == 0 || searchStrings.All<string>(new Func<string, bool>(string.IsNullOrWhiteSpace)))
                return false;
            foreach (string searchString in searchStrings)
            {
                bool flag = false;
                string title = this.Title;
                char[] chArray = new char[1] { ' ' };
                foreach (string str in title.Split(chArray))
                {
                    flag = str.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase);
                    if (flag)
                        break;
                }
                if (!flag)
                    return false;
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
    }
}
