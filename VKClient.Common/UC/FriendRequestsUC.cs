using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class FriendRequestsUC : UserControl
  {
      public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(FriendRequests), typeof(FriendRequestsUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestsUC.OnModelChanged)));
    private FriendRequests _model;
    internal TextBlock TitleBlock;
    internal Border ShowAllBlock;
    internal ContentControl RequestView;
    private bool _contentLoaded;

    public FriendRequests Model
    {
      get
      {
        return (FriendRequests) base.GetValue(FriendRequestsUC.ModelProperty);
      }
      set
      {
        base.SetValue(FriendRequestsUC.ModelProperty, value);
      }
    }

    public FriendRequestsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((FriendRequestsUC) d).UpdateDataView((FriendRequests) e.NewValue);
    }

    public void UpdateDataView(FriendRequests model)
    {
      this._model = model;
      if (this._model == null || model.count == 0)
        return;
      if (!model.are_suggested_friends)
        this.TitleBlock.Text = (UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.OneFriendRequestFrm, CommonResources.TwoFourFriendRequestsFrm, CommonResources.FiveFriendRequestsFrm, true,  null, false));
      else
        this.TitleBlock.Text = (UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.SuggestedFriendOneFrm, CommonResources.SuggestedFriendTwoFrm, CommonResources.SuggestedFriendFiveFrm, true,  null, false));
      ((UIElement) this.ShowAllBlock).Visibility = (model.count > 1 ? Visibility.Visible : Visibility.Collapsed);
      if (model.requests[0].RequestHandledAction == null)
        model.requests[0].RequestHandledAction = (Action<FriendRequests>) (requests => ((FriendsViewModel) base.DataContext).RequestsViewModel = requests);
      this.RequestView.Content=(new FriendRequestUC()
      {
        Model = model.requests[0],
        Profiles = model.profiles.ToArray(),
        IsSuggestedFriend = new bool?(model.are_suggested_friends)
      });
    }

    private void ShowAllBlock_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._model == null)
        return;
      ParametersRepository.SetParameterForId("FriendRequestsUC", this);
      Navigator.Current.NavigateToFriendRequests(this._model.are_suggested_friends);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/FriendRequestsUC.xaml", UriKind.Relative));
      this.TitleBlock = (TextBlock) base.FindName("TitleBlock");
      this.ShowAllBlock = (Border) base.FindName("ShowAllBlock");
      this.RequestView = (ContentControl) base.FindName("RequestView");
    }
  }
}
