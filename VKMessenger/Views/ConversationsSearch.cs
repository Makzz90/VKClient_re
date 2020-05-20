using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using VKClient.Common.Framework;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationsSearch : PageBase
  {
    private DispatcherTimer _searchTimer;
    private string previousSearchStringMessages;
    private string previousSearchStringConversations;
    internal Grid LayoutRoot;
    internal TextBox searchTextBox;
    internal TextBlock textBlockWatermarkText;
    internal Pivot pivotControl;
    internal ExtendedLongListSelector listBoxSearchDialog;
    internal ExtendedLongListSelector listBoxSearchMessages;
    private bool _contentLoaded;

    public ConversationSearchViewModel VM
    {
      get
      {
        return base.DataContext as ConversationSearchViewModel;
      }
    }

    public ConversationsSearch()
    {
      this.InitializeComponent();
      this._searchTimer = new DispatcherTimer();
      this._searchTimer.Interval=(TimeSpan.FromSeconds(1.0));
      this._searchTimer.Tick+=(new EventHandler(this.searchTimer_Tick));
    }

    private void searchTimer_Tick(object sender, EventArgs e)
    {
      this.CheckAndStartSearch(this.searchTextBox.Text);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (e.NavigationMode != NavigationMode.New)
        return;
      base.DataContext = (new ConversationSearchViewModel());
      (base.DataContext as ConversationSearchViewModel).Conversations.Clear();
      (base.DataContext as ConversationSearchViewModel).Messages.Clear();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      this._searchTimer.Stop();
    }

    private void PageBase_Loaded(object sender, RoutedEventArgs e)
    {
      ConversationSearchViewModel dataContext = base.DataContext as ConversationSearchViewModel;
      if ((dataContext.Messages.Count != 0 || this.pivotControl.SelectedIndex != 1) && (dataContext.Conversations.Count != 0 || this.pivotControl.SelectedIndex != 0))
        return;
      ((Control) this.searchTextBox).Focus();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Visibility = (this.searchTextBox.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed);
      this._searchTimer.Stop();
      this._searchTimer.Start();
    }

    private void CheckAndStartSearch(string text)
    {
      if (this.pivotControl.SelectedIndex == 0 && string.Compare(text, this.previousSearchStringConversations) != 0)
      {
        this.previousSearchStringConversations = text;
        (base.DataContext as ConversationSearchViewModel).SearchConversations(text);
      }
      if (this.pivotControl.SelectedIndex != 1 || string.Compare(text, this.previousSearchStringMessages) == 0)
        return;
      this.previousSearchStringMessages = text;
      (base.DataContext as ConversationSearchViewModel).SearchMessages(text);
    }

    private void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.CheckAndStartSearch(this.searchTextBox.Text);
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ConversationHeader selectedItem = (sender as ExtendedLongListSelector).SelectedItem as ConversationHeader;
      if (selectedItem == null)
        return;
      this.NavigateToConversation(selectedItem, true);
      (sender as ExtendedLongListSelector).SelectedItem = null;
    }

    private void ListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ConversationHeader selectedItem = (sender as ExtendedLongListSelector).SelectedItem as ConversationHeader;
      if (selectedItem == null)
        return;
      this.NavigateToConversation(selectedItem, false);
      (sender as ExtendedLongListSelector).SelectedItem = null;
    }

    private void NavigateToConversation(ConversationHeader conversationInfo, bool clearPreviousPage)
    {
      bool isChat = false;
      int num;
      if (conversationInfo._message.chat_id != 0)
      {
        isChat = true;
        num = conversationInfo._message.chat_id;
      }
      else
        num = conversationInfo._message.uid;
      Navigator.Current.NavigateToConversation((long) num, isChat, clearPreviousPage, "", (long) conversationInfo._message.id, false);
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      int count = this.VM.Messages.Count;
      ConversationHeader content = e.ContentPresenter.Content as ConversationHeader;
      if (count < 10 || content == null || this.VM.Messages[count - 10] != content)
        return;
      this.VM.LoadMoreMessages(this.previousSearchStringMessages, (Action) (() => {}));
    }

    private void searchDialogManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.listBoxSearchDialog).Focus();
    }

    private void searchMessagesManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.listBoxSearchMessages).Focus();
    }

    private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this.listBoxSearchMessages).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationsSearch.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.searchTextBox = (TextBox) base.FindName("searchTextBox");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.pivotControl = (Pivot) base.FindName("pivotControl");
      this.listBoxSearchDialog = (ExtendedLongListSelector) base.FindName("listBoxSearchDialog");
      this.listBoxSearchMessages = (ExtendedLongListSelector) base.FindName("listBoxSearchMessages");
    }
  }
}
