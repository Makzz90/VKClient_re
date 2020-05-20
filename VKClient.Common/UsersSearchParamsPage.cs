using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class UsersSearchParamsPage : PageBase
  {
    private bool _isIntialized;
    private UsersSearchParamsViewModel _viewModel;
    private readonly ApplicationBarIconButton _appBarButtonSave;
    private readonly ApplicationBarIconButton _appBarButtonReset;
    internal Storyboard ShowCustomAgeAnimation;
    internal Storyboard HideCustomAgeAnimation;
    internal GenericHeaderUC ucHeader;
    internal Border customAgeContainer;
    private bool _contentLoaded;

    public UsersSearchParamsPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("./Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarMenuSave = CommonResources.AppBarMenu_Save;
      applicationBarIconButton1.Text = appBarMenuSave;
      this._appBarButtonSave = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonReset = applicationBarIconButton2;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_SearchParameters;
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      applicationBar.Buttons.Add(this._appBarButtonSave);
      this._appBarButtonSave.Click+=(new EventHandler(this.AppBarButtonSave_OnClick));
      applicationBar.Buttons.Add(this._appBarButtonReset);
      this._appBarButtonReset.Click+=(new EventHandler(this.AppBarButtonReset_OnClick));
      this.ApplicationBar = ((IApplicationBar) applicationBar);
    }

    private void AppBarButtonSave_OnClick(object sender, EventArgs eventArgs)
    {
      this._viewModel.Save();
      Navigator.Current.GoBack();
    }

    private void AppBarButtonReset_OnClick(object sender, EventArgs eventArgs)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isIntialized)
        return;
      this._viewModel = new UsersSearchParamsViewModel(ParametersRepository.GetParameterForIdAndReset("UsersSearchParams") as SearchParams);
      base.DataContext = this._viewModel;
      this._isIntialized = true;
    }

    private void CountryPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      CountryPickerUC.Show(this._viewModel.Country, true, (Action<Country>) (country => this._viewModel.Country = country),  null);
    }

    private void CityPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._viewModel.Country == null)
        return;
      CityPickerUC.Show(this._viewModel.Country.id, this._viewModel.City, true, (Action<City>) (city => this._viewModel.City = city),  null);
    }

    private void AnyAgeCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
    {
      this.ShowCustomAgeAnimation.Begin();
    }

    private void AnyAgeCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
      this.HideCustomAgeAnimation.Begin();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UsersSearchParamsPage.xaml", UriKind.Relative));
      this.ShowCustomAgeAnimation = (Storyboard) base.FindName("ShowCustomAgeAnimation");
      this.HideCustomAgeAnimation = (Storyboard) base.FindName("HideCustomAgeAnimation");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.customAgeContainer = (Border) base.FindName("customAgeContainer");
    }
  }
}
