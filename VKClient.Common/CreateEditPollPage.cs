using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class CreateEditPollPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBarIconButton _appBarButtonCheck;
    private ApplicationBarIconButton _appBarButtonCancel;
    private ApplicationBar _mainAppBar;
    private readonly DelayedExecutor _de;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    internal StackPanel stackPanel;
    internal TextBox textBoxQuestion;
    internal InlineAddButtonUC ucAddOption;
    internal TextBoxPanelControl textBoxPanel;
    private bool _contentLoaded;

    private CreateEditPollViewModel VM
    {
      get
      {
        return base.DataContext as CreateEditPollViewModel;
      }
    }

    public CreateEditPollPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarMenuSave = CommonResources.AppBarMenu_Save;
      applicationBarIconButton1.Text = appBarMenuSave;
      this._appBarButtonCheck = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonCancel = applicationBarIconButton2;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._mainAppBar = applicationBar;
      this._de = new DelayedExecutor(100);
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucAddOption.Text = CommonResources.Poll_AddAnOption;
      this.ucAddOption.OnAdd = new Action(this.AddOption);
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.CreateEditPollPage_Loaded));
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click+=(new EventHandler(this._appBarButtonCheck_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
      this._mainAppBar.Buttons.Add(this._appBarButtonCheck);
      this._mainAppBar.Buttons.Add(this._appBarButtonCancel);
      this.ApplicationBar = ((IApplicationBar) this._mainAppBar);
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      this.VM.SavePoll((Action<Poll>) (poll =>
      {
        ParametersRepository.SetParameterForId("UpdatedPoll", poll);
        Navigator.Current.GoBack();
      }));
    }

    private void CreateEditPollPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.VM != null && this.VM.CurrentMode == CreateEditPollViewModel.Mode.Create)
        ((Control) this.textBoxQuestion).Focus();
      // ISSUE: method pointer
      base.Loaded-=(new RoutedEventHandler( this.CreateEditPollPage_Loaded));
    }

    private void AddOption()
    {
      this.VM.AddPollOption();
      this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
      {
        List<TextBox> textBoxList = FramePageUtils.AllTextBoxes((DependencyObject) this.LayoutRoot);
        if (!Enumerable.Any<TextBox>(textBoxList))
          return;
        ((Control)Enumerable.Last<TextBox>(textBoxList, (Func<TextBox, bool>)(t =>
        {
          if (((FrameworkElement) t).Tag != null)
            return ((FrameworkElement) t).Tag.ToString() == "RemovableTextBox";
          return false;
        }))).Focus();
      }))));
      ((DependencyObject) Deployment.Current).Dispatcher.BeginInvoke((Action) (() => {}));
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) this.textBoxQuestion).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        long ownerId = long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]);
        long pollId = long.Parse(((Page) this).NavigationContext.QueryString["PollId"]);
        Poll parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("Poll") as Poll;
        if (pollId != 0L)
          base.DataContext = (CreateEditPollViewModel.CreateForEditPoll(ownerId, pollId, parameterForIdAndReset));
        else
          base.DataContext = (CreateEditPollViewModel.CreateForNewPoll(ownerId));
        this.VM.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.BuildAppBar();
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanSave"))
        return;
      this.UpdateAppBar();
    }

    private void textBox_KeyUp(object sender, KeyEventArgs e)
    {
      TextBox textbox = sender as TextBox;
      if (textbox == null || string.IsNullOrWhiteSpace(textbox.Text) || e.Key != Key.Enter)
        return;
      TextBox nextTextBox = FramePageUtils.FindNextTextBox((DependencyObject) this.LayoutRoot, textbox);
      if (nextTextBox == null)
        return;
      ((Control) nextTextBox).Focus();
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = true;
      FrameworkElement frameworkElement = (FrameworkElement) sender;
      if (frameworkElement.Name == ((FrameworkElement) this.textBoxQuestion).Name)
      {
        this.scrollViewer.ScrollToVerticalOffset(0.0);
        base.UpdateLayout();
      }
      StackPanel stackPanel = this.stackPanel;
      Point relativePosition = ((UIElement) frameworkElement).GetRelativePosition((UIElement) stackPanel);
      // ISSUE: explicit reference operation
      this.scrollViewer.ScrollToOffsetWithAnimation(((Point) @relativePosition).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = false;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/CreateEditPollPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.textBoxQuestion = (TextBox) base.FindName("textBoxQuestion");
      this.ucAddOption = (InlineAddButtonUC) base.FindName("ucAddOption");
      this.textBoxPanel = (TextBoxPanelControl) base.FindName("textBoxPanel");
    }
  }
}
