using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.MoneyTransfers.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.MoneyTransfers
{
  public class SendMoneyPage : PageBase
  {
    private bool _isInitialized;
    private SendMoneyViewModel _viewModel;
    internal ScrollViewer Viewer;
    internal Grid ViewerContent;
    internal Path bubbleTail;
    internal StackPanel AmountPanel;
    internal TextBox AmountBox;
    internal TextBlock AmountPlaceholder;
    internal TextBox CommentBox;
    internal TextBlock CommentPlaceholder;
    internal TextBoxPanelControl TextBoxPanel;
    private bool _contentLoaded;

    public SendMoneyPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
      this.bubbleTail.Data=(PathHelper.CreateTriangleGeometry(new Point(0.0, 0.0), new Point(12.0, 0.0), new Point(0.0, 16.0)));
    }

    protected override async void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      User parameterForIdAndReset = (User) ParametersRepository.GetParameterForIdAndReset("MoneyTransferTargetUser");
      long targetId = long.Parse(((Page) this).NavigationContext.QueryString["TargetId"]);
      int amount = int.Parse(((Page) this).NavigationContext.QueryString["Amount"]);
      string comment = Extensions.ForUI(((Page) this).NavigationContext.QueryString["Comment"]);
      this._viewModel = new SendMoneyViewModel(targetId, parameterForIdAndReset, amount, comment);
      await Task.Delay(1);
      base.DataContext = this._viewModel;
      this._viewModel.Load();
      this._isInitialized = true;
    }

    protected override void HandleMoneyTransferSentResponse(MoneyTransferSentResponse response)
    {
      Navigator.Current.GoBack();
    }

    private void HelpButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      string uri = "https://m.vk.com/attachments?act=attach_money_about&from_client=1";
      string lang = LangHelper.GetLang();
      if (!string.IsNullOrEmpty(lang))
        uri += string.Format("&lang={0}", lang);
      Navigator.Current.NavigateToWebViewPage(uri, false);
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      TextBox textBox = (TextBox) sender;
      if (textBox == this.AmountBox)
      {
        string text = textBox.Text;
        ((UIElement) this.AmountPlaceholder).Opacity = (text == "" ? 1.0 : 0.0);
        ((FrameworkElement) this.AmountBox).Margin=(new Thickness(-22.0, 0.0, text == "" ? -10.0 : 0.0, 0.0));
        ((FrameworkElement) this.AmountPanel).Margin=(new Thickness(text == "" ? 32.0 : 0.0, 0.0, 0.0, 6.0));
        this._viewModel.Amount = text;
      }
      if (textBox != this.CommentBox)
        return;
      ((UIElement) this.CommentPlaceholder).Opacity = (this.CommentBox.Text == "" ? 1.0 : 0.0);
      this._viewModel.Comment = textBox.Text;
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      Point relativePosition = ((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent);
      // ISSUE: explicit reference operation
      this.Viewer.ScrollToOffsetWithAnimation(((Point) @relativePosition).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
    }

    private void AmountBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      char platformKeyCode = (char) e.PlatformKeyCode;
      if (char.IsDigit(platformKeyCode) && ((int) platformKeyCode != 48 || !(this.AmountBox.Text == "")))
        return;
      e.Handled = true;
    }

    private async void AmountBox_OnLoaded(object sender, RoutedEventArgs e)
    {
      await Task.Delay(500);
      ((Control) this.AmountBox).Focus();
      this.AmountBox.SelectionStart = this.AmountBox.Text.Length;
    }

    private void AmountPanel_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((Control) this.AmountBox).Focus();
    }

    private void SendButton_OnClicked(object sender, RoutedEventArgs e)
    {
      this.Send();
    }

    private void CommentBox_OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.Send();
    }

    private void Send()
    {
      if (!this._viewModel.IsReadyForSending)
        return;
      this._viewModel.Send();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/MoneyTransfers/SendMoneyPage.xaml", UriKind.Relative));
      this.Viewer = (ScrollViewer) base.FindName("Viewer");
      this.ViewerContent = (Grid) base.FindName("ViewerContent");
      this.bubbleTail = (Path) base.FindName("bubbleTail");
      this.AmountPanel = (StackPanel) base.FindName("AmountPanel");
      this.AmountBox = (TextBox) base.FindName("AmountBox");
      this.AmountPlaceholder = (TextBlock) base.FindName("AmountPlaceholder");
      this.CommentBox = (TextBox) base.FindName("CommentBox");
      this.CommentPlaceholder = (TextBlock) base.FindName("CommentPlaceholder");
      this.TextBoxPanel = (TextBoxPanelControl) base.FindName("TextBoxPanel");
    }
  }
}
