using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKMessenger.Library
{
    public sealed class UsersTypingHelper
    {
        private readonly List<long> _typingNowUserIds = new List<long>();
        private readonly Dictionary<long, DispatcherTimer> _typingNowTimers = new Dictionary<long, DispatcherTimer>();
        private string _typingStringValue = "";
        private Visibility _typingVisibilityValue = Visibility.Collapsed;
        private string _uiSubtitleDefaultValue = "";
        private DispatcherTimer _dotsAnimationTimer;
        private int _currentDotsNumber;
        private string _typingString;
        private ConversationHeader _conversationHeader;
        private ConversationViewModel _conversationViewModel;

        public bool IsChat
        {
            get
            {
                if (this._conversationHeader != null)
                    return this._conversationHeader.IsChat;
                return this._conversationViewModel.IsChat;
            }
        }

        public string TypingString
        {
            get
            {
                if (this._conversationHeader != null)
                    return this._conversationHeader.TypingStr;
                return this._typingStringValue;
            }
            set
            {
                if (this._conversationHeader != null)
                {
                    this._conversationHeader.TypingStr = value;
                }
                else
                {
                    this._typingStringValue = value;
                    if (this.TypingVisibility != Visibility.Visible)
                        return;
                    this._conversationViewModel.UISubtitle = value;
                }
            }
        }

        public Visibility TypingVisibility
        {
            get
            {
                if (this._conversationHeader != null)
                    return this._conversationHeader.TypingVisibility;
                return this._typingVisibilityValue;
            }
            set
            {
                if (this._conversationHeader != null)
                {
                    this._conversationHeader.TypingVisibility = value;
                }
                else
                {
                    if (this._typingVisibilityValue == value)
                        return;
                    this._typingVisibilityValue = value;
                    if (value == Visibility.Visible)
                    {
                        this._uiSubtitleDefaultValue = this._conversationViewModel.UISubtitle;
                        this._conversationViewModel.UISubtitle = this.TypingString;
                    }
                    else
                        this._conversationViewModel.UISubtitle = this._uiSubtitleDefaultValue;
                }
            }
        }

        public bool AnyTypingNow
        {
            get
            {
                return this._typingNowUserIds.Any<long>();
            }
        }

        public UsersTypingHelper(ConversationHeader conversationHeader)
        {
            this._conversationHeader = conversationHeader;
        }

        public UsersTypingHelper(ConversationViewModel conversationViewModel)
        {
            this._conversationViewModel = conversationViewModel;
        }

        public void Reinitialize(ConversationHeader conversationHeader)
        {
            string typingString = this.TypingString;
            Visibility typingVisibility = this.TypingVisibility;
            this._conversationHeader = conversationHeader;
            this.TypingString = typingString;
            this.TypingVisibility = typingVisibility;
        }

        public void SetUserIsTypingWithDelayedReset(long userId)
        {
            ConversationViewModel conversationViewModel = this._conversationViewModel;
            if ((conversationViewModel != null ? (!conversationViewModel.IsOnScreen ? 1 : 0) : 0) != 0)
                return;
            if (this._typingNowUserIds.Contains(userId))
            {
                this.UpdateTypingState(userId, false);
            }
            else
            {
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                TimeSpan timeSpan = TimeSpan.FromSeconds(5.0);
                dispatcherTimer.Interval=(timeSpan);
                DispatcherTimer timer = dispatcherTimer;
                timer.Tick+=((EventHandler)((o, e) =>
                {
                    timer.Stop();
                    this.UpdateTypingState(userId, true);
                }));
                timer.Start();
                this._typingNowUserIds.Insert(0, userId);
                this._typingNowTimers.Add(userId, timer);
                this.UpdateTypingView();
            }
        }

        private void UpdateTypingState(long userId, bool typingIsOver)
        {
            if (!typingIsOver)
            {
                DispatcherTimer typingNowTimer = this._typingNowTimers[userId];
                typingNowTimer.Stop();
                typingNowTimer.Start();
            }
            else
            {
                this._typingNowUserIds.Remove(userId);
                this._typingNowTimers.Remove(userId);
                this.UpdateTypingView();
            }
        }

        private void UpdateTypingView()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!this._typingNowUserIds.Any<long>())
                {
                    this.TypingVisibility = Visibility.Collapsed;
                    DispatcherTimer dotsAnimationTimer = this._dotsAnimationTimer;
                    if (dotsAnimationTimer == null)
                        return;
                    dotsAnimationTimer.Stop();
                }
                else
                {
                    this._typingString = CommonResources.Conversation_IsTyping.Replace("...", "");
                    if (this.IsChat)
                    {
                        User associatedUser = this.GetAssociatedUser(this._typingNowUserIds.First<long>());
                        string str = associatedUser != null ? associatedUser.FirstNameLastNameShort : (string)null;
                        this._typingString = this._typingNowUserIds.Count != 1 ? string.Format(CommonResources.Conversation_FewUsersAreTypingFrm.Replace("...", ""), (object)str, (object)(this._typingNowUserIds.Count - 1)) : string.Format(CommonResources.Conversation_UserIsTypingFrm.Replace("...", ""), (object)str);
                    }
                    if (this.TypingVisibility == Visibility.Collapsed)
                    {
                        this.TypingVisibility = Visibility.Visible;
                        DispatcherTimer dispatcherTimer = new DispatcherTimer();
                        TimeSpan timeSpan = TimeSpan.FromSeconds(0.25);
                        dispatcherTimer.Interval=(timeSpan);
                        this._dotsAnimationTimer = dispatcherTimer;
                        this._currentDotsNumber = 1;
                        this._dotsAnimationTimer.Tick+=((EventHandler)((o, e) =>
                        {
                            this._currentDotsNumber = this._currentDotsNumber + 1;
                            if (this._currentDotsNumber > 3)
                            {
                                this._currentDotsNumber = 0;
                            }
                            else
                            {
                                string typingString = this._typingString;
                                for (int index = 0; index < this._currentDotsNumber; ++index)
                                    typingString += ".";
                                this.TypingString = typingString;
                            }
                        }));
                        this.TypingString = this._typingString + ".";
                        this._dotsAnimationTimer.Start();
                    }
                    else
                    {
                        string typingString = this._typingString;
                        for (int index = 0; index < this._currentDotsNumber; ++index)
                            typingString += ".";
                        this.TypingString = typingString;
                    }
                }
            }));
        }

        private User GetAssociatedUser(long id)
        {
            ConversationHeader conversationHeader = this._conversationHeader;
            User user;
            if (conversationHeader == null)
            {
                user = (User)null;
            }
            else
            {
                List<User> associatedUsers = conversationHeader._associatedUsers;
                if (associatedUsers == null)
                {
                    user = (User)null;
                }
                else
                {
                    Func<User, bool> predicate = (Func<User, bool>)(u => u.id == id);
                    user = associatedUsers.FirstOrDefault<User>(predicate);
                }
            }
            return user ?? UsersService.Instance.GetCachedUser(id);
        }

        public void SetUserIsNotTyping(long userId)
        {
            if (!this._typingNowUserIds.Contains(userId))
                return;
            this._typingNowTimers[userId].Stop();
            this.UpdateTypingState(userId, true);
        }
    }
}
