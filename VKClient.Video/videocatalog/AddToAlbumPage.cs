using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
  public class AddToAlbumPage : PageBase
  {
    private bool _isInitialized;
    private long _videoId;
    private ApplicationBarIconButton _appBarButtonCommit;
    private readonly ApplicationBarIconButton _addAlbumButton;
    private ApplicationBar _appBar;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    private bool _contentLoaded;

    private AddToAlbumViewModel VM
    {
      get
      {
        return base.DataContext as AddToAlbumViewModel;
      }
    }

    public AddToAlbumPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarMenuSave = CommonResources.AppBarMenu_Save;
      applicationBarIconButton1.Text = appBarMenuSave;
      this._appBarButtonCommit = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarAdd = CommonResources.AppBar_Add;
      applicationBarIconButton2.Text = appBarAdd;
      this._addAlbumButton = applicationBarIconButton2;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCommit.Click+=(new EventHandler(this._appBarButtonCommit_Click));
      this._addAlbumButton.Click+=(new EventHandler(this._addAlbumButton_Click));
      this._appBar.Buttons.Add(this._addAlbumButton);
      this._appBar.Buttons.Add(this._appBarButtonCommit);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _addAlbumButton_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCreateEditVideoAlbum(0, this.VM.TargetId < 0L ? -this.VM.TargetId : 0, "",  null);
    }

    private void _appBarButtonCommit_Click(object sender, EventArgs e)
    {
        this.VM.Save((Action<bool>)(res =>
        {
            if (!res)
                return;
            Execute.ExecuteOnUIThread((Action)(() => Navigator.Current.GoBack()));
        }));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._videoId = long.Parse(((Page) this).NavigationContext.QueryString["VideoId"]);
      AddToAlbumViewModel toAlbumViewModel = new AddToAlbumViewModel(this.CommonParameters.OwnerId, this._videoId);
      base.DataContext = toAlbumViewModel;
      toAlbumViewModel.AlbumsVM.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/AddToAlbumPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
    }
  }
}
