using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ChatEditPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC Header;
    internal ExtendedLongListSelector ContentList;
    internal TextBox TitleBox;
    internal TextBoxPanelControl TextBoxPanel;
    private bool _contentLoaded;

    private ChatEditViewModel ChatEditVM
    {
      get
      {
        return base.DataContext as ChatEditViewModel;
      }
    }

    public ChatEditPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap += (Action) (() => this.ContentList.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        base.DataContext = (new ChatEditViewModel(long.Parse(((Page) this).NavigationContext.QueryString["ChatId"]), ((Page) this).NavigationService));
        this.ChatEditVM.Reload(true);
        this._isInitialized = true;
      }
      else
      {
        List<User> parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
        if (parameterForIdAndReset1 != null && Enumerable.Any<User>(parameterForIdAndReset1))
          this.ChatEditVM.AddMember((User) Enumerable.First<User>(parameterForIdAndReset1));
        List<Stream> parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
        if (parameterForIdAndReset2 == null || !Enumerable.Any<Stream>(parameterForIdAndReset2))
          return;
        Rect crop =  new Rect();
        if (ParametersRepository.Contains("UserPicSquare"))
          crop = (Rect) ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
        this.ChatEditVM.UpdatePhoto(parameterForIdAndReset2[0], crop);
      }
    }

    private void TitleBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
      if (!(this.TitleBox.Text != this.ChatEditVM.Title))
        return;
      this.ChatEditVM.ChangeTitle(this.TitleBox.Text, (Action) (() => this.TitleBox.Text = this.ChatEditVM.Title));
    }

    private void Photo_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ChatEditVM.IsPhotoMenuEnabled || this.ChatEditVM.IsPhotoChanging)
        return;
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    private void ChangePhoto_OnClicked(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    private void DeletePhoto_OnClicked(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.DeleteOnePhoto, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ChatEditVM.DeletePhoto();
    }

    private void NotificationsSound_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ChatEditVM.IsNotificationsSoundModeSwitching)
        return;
      this.ChatEditVM.SwitchNotificationsSoundMode();
    }

    private void ConversationMaterials_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToConversationMaterials(this.ChatEditVM.PeerId);
    }

    private void AddMember_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ChatEditVM.IsMemberAdding)
        return;
      Navigator.Current.NavigateToPickUser(false, 0, true, 0, PickUserMode.PickWithSearch, "", 0, true);
    }

    private void Member_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ChatMember dataContext = ((FrameworkElement) sender).DataContext as ChatMember;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToUserProfile(dataContext.Id, "", "", false);
    }

    private void ExcludeButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      ChatMember dataContext = ((FrameworkElement) sender).DataContext as ChatMember;
      if (dataContext == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.ChatMemberExcluding, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ChatEditVM.ExcludeMember(dataContext);
    }

    private void LeaveButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ChatEditVM.IsChatLeaving || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.ChatMemberExcluding, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ChatEditVM.LeaveChat();
    }

    private void TitleBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      Point relativePosition = ((UIElement) sender).GetRelativePosition((UIElement) this.ContentList);
      // ISSUE: explicit reference operation
      this.ContentList.ScrollToPosition(((Point) @relativePosition).Y - 38.0);
    }

    private void TitleBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
      if (!(this.TitleBox.Text != this.ChatEditVM.Title))
        return;
      this.ChatEditVM.ChangeTitle(this.TitleBox.Text, (Action) (() => this.TitleBox.Text = this.ChatEditVM.Title));
    }

    private void ExcludeButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ChatEditPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ContentList = (ExtendedLongListSelector) base.FindName("ContentList");
      this.TitleBox = (TextBox) base.FindName("TitleBox");
      this.TextBoxPanel = (TextBoxPanelControl) base.FindName("TextBoxPanel");
    }
  }
}
