using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
    public class InvitationItemHeader : ISubscriptionItemHeader, INotifyPropertyChanged
    {
        private bool _isInvited;

        public SubscriptionItemType SubscriptionItemType
        {
            get
            {
                return SubscriptionItemType.Invitation;
            }
        }

        public string Title { get; private set; }

        public string Subtitle { get; private set; }

        public string ImageUrl { get; set; }

        public Stream ImageStream { get; set; }

        public Action InviteTapFunc { get; set; }

        public Visibility InviteVisibility
        {
            get
            {
                return !this._isInvited ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility InvitedVisibility
        {
            get
            {
                return !this._isInvited ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private InvitationItemHeader(string title, string subtitle, Action<Action<bool>> inviteTapFunc)
        {
            InvitationItemHeader invitationItemHeader = this;
            this.Title = title;
            this.Subtitle = subtitle;
            this.InviteTapFunc = (Action)(() =>
            {
                if (inviteTapFunc == null)
                    return;

                inviteTapFunc(new Action<bool>(res =>
                {
                    this._isInvited = res;
                    this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.InviteVisibility));
                    this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.InvitedVisibility));
                }));
            });
        }

        public InvitationItemHeader(string title, string subtitle, string imageUrl, Action<Action<bool>> inviteTapFunc)
            : this(title, subtitle, inviteTapFunc)
        {
            this.ImageUrl = imageUrl;
        }

        public InvitationItemHeader(string title, string subtitle, Stream imageStream, Action<Action<bool>> inviteTapFunc)
            : this(title, subtitle, inviteTapFunc)
        {
            this.ImageStream = imageStream;
        }

        private void NotifyPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                return;
            this.RaisePropertyChanged((propertyExpression.Body as MemberExpression).Member.Name);
        }

        private void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.PropertyChanged == null)
                    return;
                this.PropertyChanged((object)this, new PropertyChangedEventArgs(property));
            }));
        }
    }
}
