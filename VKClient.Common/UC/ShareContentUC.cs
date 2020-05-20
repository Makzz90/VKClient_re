using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class ShareContentUC : UserControl
  {
    private ConversationsUCBase _conversationsUC;
    internal Grid gridRoot;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public IShareContentDataProvider ShareContentDataProvider { get; set; }

    public ShareContentUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.ShareWallPost_Share.ToUpperInvariant());
      this.ucHeader.HideSandwitchButton = true;
      this.InitPageControls();
    }

    private void InitPageControls()
    {
      this._conversationsUC = ServiceLocator.Resolve<IConversationsUCFactory>().Create();
      this._conversationsUC.IsShareContentMode = true;
      Grid.SetRow((FrameworkElement) this._conversationsUC, 1);
      // ISSUE: method pointer
      this._conversationsUC.Loaded+=(delegate(object sender, RoutedEventArgs args)
      {
          this._conversationsUC.PrepareForViewIfNeeded();
      });
      this._conversationsUC.ConversationTap += new EventHandler<Action>(this.ConversationsUCOnConversationTap);
      ShareContentActionsUC contentActionsUc = new ShareContentActionsUC();
      contentActionsUc.ShareWallPostItemSelected += new EventHandler(this.ListHeader_OnShareWallPostItemSelected);
      contentActionsUc.ShareCommunityItemSelected += new EventHandler(this.ListHeader_OnShareCommunityItemSelected);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.gridRoot).Children).Add((UIElement) this._conversationsUC);
      this._conversationsUC.SetListHeader((FrameworkElement) contentActionsUc);
    }

    private void ListHeader_OnShareWallPostItemSelected(object sender, EventArgs eventArgs)
    {
      this.ShareContentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider(this.ShareContentDataProvider);
      Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
    }

    private void ListHeader_OnShareCommunityItemSelected(object sender, EventArgs eventArgs)
    {
      this.ShareContentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider(this.ShareContentDataProvider);
      Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0, 0, "", false, "", 0L);
    }

    private void ConversationsUCOnConversationTap(object sender, Action callback)
    {
      this.ShareContentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider(this.ShareContentDataProvider);
      if (callback == null)
        return;
      callback();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ShareContentUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
