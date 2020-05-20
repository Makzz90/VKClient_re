using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.Views
{
  public class GiftSendPage : PageBase
  {
    private const string PAGE_STATE_KEY = "GiftSendPageState";
    private bool _isInitialized;
    private GiftSendViewModel _viewModel;
    private bool _isFocused;
    private const int MESSAGE_MIN_UNSCROLLABLE_HEIGHT = 134;
    private double _savedHeight;
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    internal ScrollViewer scrollViewerMessage;
    internal TextBox textBoxMessage;
    internal TextBlock textBlockWatermark;
    internal TextBoxPanelControl textBoxPanel;
    private bool _contentLoaded;

    public GiftSendPage()
    {
      this.InitializeComponent();
      this.ucHeader.Title = CommonResources.Gift.ToUpperInvariant();
      this.ucHeader.OnHeaderTap = (Action) (() => this.scrollViewer.ScrollToTopWithAnimation());
      this.textBoxPanel.BindTextBox(this.textBoxMessage);
      this.textBoxPanel.IsFocusedChanged += new EventHandler<bool>(this.IsFocusedChanged);
    }

    private void IsFocusedChanged(object sender, bool e)
    {
      this._isFocused = this.textBoxPanel.IsTextBoxTargetFocused;
    }

    private void ForceFocusIfNeeded()
    {
      if (!this._isFocused)
        return;
      ((Control) this.textBoxMessage).Focus();
      this.textBoxPanel.IgnoreNextLostGotFocus();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
      {
        if (!ParametersRepository.Contains("SelectedUsers"))
          return;
        List<User> parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
        User user = parameterForIdAndReset != null ?  Enumerable.FirstOrDefault<User>(parameterForIdAndReset) :  null;
        if (user == null)
          return;
        this._viewModel.AddRecipient(user);
      }
      else
      {
        long result1 = 0;
        string categoryName = "";
        bool isProduct = false;
        string description1 = "";
        string imageUrl1 = "";
        int result2 = 0;
        int result3 = 0;
        List<long> userIds1 =  null;
        IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
        if (queryString.ContainsKey("GiftId"))
          long.TryParse(queryString["GiftId"], out result1);
        if (queryString.ContainsKey("CategoryName"))
          categoryName = queryString["CategoryName"];
        if (queryString.ContainsKey("IsProduct") && queryString["IsProduct"] == bool.TrueString)
          isProduct = true;
        if (ParametersRepository.Contains("Description"))
          description1 = ParametersRepository.GetParameterForIdAndReset("Description") as string;
        if (ParametersRepository.Contains("ImageUrl"))
          imageUrl1 = ParametersRepository.GetParameterForIdAndReset("ImageUrl") as string;
        if (ParametersRepository.Contains("Price"))
          int.TryParse(ParametersRepository.GetParameterForIdAndReset("Price").ToString(), out result2);
        if (ParametersRepository.Contains("GiftsLeft"))
          int.TryParse(ParametersRepository.GetParameterForIdAndReset("GiftsLeft").ToString(), out result3);
        if (ParametersRepository.Contains("UserIds"))
        {
          userIds1 = ParametersRepository.GetParameterForIdAndReset("UserIds") as List<long>;
          long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
          if (userIds1 != null && userIds1.Contains(loggedInUserId))
            userIds1.Remove(loggedInUserId);
        }
        GiftSendPageState giftSendPageState =  null;
        if (this.State.ContainsKey("GiftSendPageState"))
          giftSendPageState = this.State["GiftSendPageState"] as GiftSendPageState;
        if (giftSendPageState != null)
        {
          string message = giftSendPageState.Message ?? "";
          bool nameAndTextPublic = giftSendPageState.AreNameAndTextPublic;
          string description2 = giftSendPageState.Description ?? "";
          string imageUrl2 = giftSendPageState.ImageUrl ?? "";
          int price = giftSendPageState.Price;
          int giftsLeft = giftSendPageState.GiftsLeft;
          List<long> userIds2 = giftSendPageState.UserIds;
          this._viewModel = new GiftSendViewModel(result1, categoryName, isProduct, description2, imageUrl2, price, giftsLeft, ((Page) this).NavigationService, userIds2, message, nameAndTextPublic);
          this.textBoxMessage.Text = message;
          this.UpdateMessageTextBox();
        }
        else
          this._viewModel = new GiftSendViewModel(result1, categoryName, isProduct, description1, imageUrl1, result2, result3, ((Page) this).NavigationService, userIds1);
        base.DataContext = this._viewModel;
        this._viewModel.Reload(true);
        this._isInitialized = true;
      }
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      this.SaveState();
      GiftSendViewModel viewModel = this._viewModel;
      if (viewModel == null)
        return;
      int num = 0;
      string inProgressText = "";
      viewModel.SetInProgress(num != 0, inProgressText);
    }

    private void SaveState()
    {
      this.State["GiftSendPageState"] = this._viewModel.GetState();
    }

    private async void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.scrollViewerMessage.VerticalScrollBarVisibility=((ScrollBarVisibility) 1);
      await Task.Delay(1);
      this.scrollViewer.ScrollToBottomWithAnimation();
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      if (((FrameworkElement) this.textBoxMessage).ActualHeight > 134.0)
        return;
      this.scrollViewerMessage.VerticalScrollBarVisibility=((ScrollBarVisibility) 0);
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateMessageTextBox();
    }

    private void UpdateMessageTextBox()
    {
      ((UIElement) this.textBlockWatermark).Opacity = (this.textBoxMessage.Text == "" ? 1.0 : 0.0);
      this._viewModel.Message = this.textBoxMessage.Text;
      this.UpdateMessageScroll();
    }

    private void UpdateMessageScroll()
    {
      double actualHeight = ((FrameworkElement) this.textBoxMessage).ActualHeight;
      Thickness padding = ((Control) this.textBoxMessage).Padding;
      // ISSUE: explicit reference operation
      double bottom = ((Thickness) @padding).Bottom;
      double num = actualHeight - bottom;
      if (this._savedHeight > 0.0)
      {
        bool flag = false;
        if (num < this._savedHeight && this.scrollViewerMessage.ExtentHeight == this.scrollViewerMessage.VerticalOffset + this.scrollViewerMessage.ViewportHeight)
          flag = true;
        if (!flag)
          this.scrollViewerMessage.ScrollToOffsetWithAnimation(this.scrollViewerMessage.VerticalOffset + num - this._savedHeight, 0.15, false);
      }
      this._savedHeight = num;
    }

    private void ToggleControl_OnCheckedUnchecked(object sender, bool e)
    {
      this.ForceFocusIfNeeded();
    }

    private void AddRecipient_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._viewModel.NavigateToAddRecipient();
    }

    private void Recipient_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      User user = (frameworkElement != null ? frameworkElement.DataContext : null) as User;
      if (user == null)
        return;
      Navigator.Current.NavigateToUserProfile(user.id, user.Name, "", false);
    }

    private void RemoveRecipient_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      User user = (frameworkElement != null ? frameworkElement.DataContext : null) as User;
      if (user == null)
        return;
      this._viewModel.RemoveRecipient(user.id);
    }

    private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
    {
      this._viewModel.Send();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftSendPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.scrollViewerMessage = (ScrollViewer) base.FindName("scrollViewerMessage");
      this.textBoxMessage = (TextBox) base.FindName("textBoxMessage");
      this.textBlockWatermark = (TextBlock) base.FindName("textBlockWatermark");
      this.textBoxPanel = (TextBoxPanelControl) base.FindName("textBoxPanel");
    }
  }
}
