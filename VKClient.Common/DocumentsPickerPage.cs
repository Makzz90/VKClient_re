using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.Storage.Pickers;

namespace VKClient.Common
{
  public class DocumentsPickerPage : PageBase
  {
    private int _maxAllowedToSelect;
    private bool _isInitialized;
    private ObservableCollection<DocumentsSectionViewModel> _sections;
    private ListPickerUC2 _picker;
    internal Grid gridRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid gridContent;
    internal ExtendedLongListSelector list;
    internal Rectangle rectSeparator;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    private DocumentsViewModel ViewModel
    {
      get
      {
        return base.DataContext as DocumentsViewModel;
      }
    }

    public DocumentsPickerPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
      this.ucHeader.OnHeaderTap += (Action) (() => this.list.ScrollToTop());
      this.list.OnRefresh = (Action) (() => this.ViewModel.CurrentSection.Items.LoadData(true, false,  null, false));
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.list);
      this.BuilAppBar();
    }

    private void BuilAppBar()
    {
      this.ApplicationBar = ((IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri;
      string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
      applicationBarIconButton1.Text = pageAppBarSearch;
      ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
      applicationBarIconButton2.Click+=(new EventHandler(this.AppBarSearchButton_OnClicked));
      this.ApplicationBar.Buttons.Add(applicationBarIconButton2);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._maxAllowedToSelect = int.Parse(((Page) this).NavigationContext.QueryString["MaxAllowedToSelect"]);
        long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
        DocumentsViewModel parentPageViewModel = new DocumentsViewModel(loggedInUserId);
        DocumentsSectionViewModel sectionViewModel = new DocumentsSectionViewModel(parentPageViewModel, loggedInUserId, 0, CommonResources.AllDocuments, false, true)
        {
          IsSelected = true
        };
        parentPageViewModel.Sections.Add(sectionViewModel);
        parentPageViewModel.LoadSection(0);
        base.DataContext = parentPageViewModel;
        this._isInitialized = true;
      }
      if (e.NavigationMode != NavigationMode.Back || !ParametersRepository.Contains("FilePicked") && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      this.SkipNextNavigationParametersRepositoryClearing = true;
      Navigator.Current.GoBack();
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      DocumentHeader selectedItem = ((LongListSelector) sender).SelectedItem as DocumentHeader;
      if (selectedItem == null)
        return;
      this.list.SelectedItem = null;
      ParametersRepository.SetParameterForId("PickedDocument", selectedItem.Document);
      Navigator.Current.GoBack();
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.CurrentSection.Items.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void AppBarSearchButton_OnClicked(object sender, EventArgs e)
    {
      DocumentsSearchDataProvider searchDataProvider = new DocumentsSearchDataProvider((IEnumerable<DocumentHeader>) this.ViewModel.CurrentSection.Items.Collection);
      DataTemplate dataTemplate = (DataTemplate) base.Resources["ItemTemplate"];
      Action<object, object> selectedItemCallback = (Action<object, object>) ((p, f) => this.List_OnSelectionChanged(p,  null));
      Action<string> textChangedCallback = (Action<string>) (searchString => ((UIElement) this.list).Visibility = (searchString != "" ? Visibility.Collapsed : Visibility.Visible));
      DataTemplate itemTemplate = dataTemplate;
      // ISSUE: variable of the null type
      
      Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
      DialogService popup = GenericSearchUC.CreatePopup<Doc, DocumentHeader>((ISearchDataProvider<Doc, DocumentHeader>) searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, null, margin);
      EventHandler eventHandler = (EventHandler) ((o, args) => ((UIElement) this.list).Visibility = Visibility.Visible);
      popup.Closing += eventHandler;
      Grid gridContent = this.gridContent;
      popup.Show((UIElement) gridContent);
    }

    private void FirstButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(this._maxAllowedToSelect, false, true);
    }

    private void SecondButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["FilePickedType"] = 10;
      foreach (string supportedDocExtension in VKConstants.SupportedDocExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedDocExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = "DocumentFromPhone";
      fileOpenPicker.PickSingleFileAndContinue();
    }

    private void DocumentsSectionFilter_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.OpenSectionsPicker();
    }

    private void OpenSectionsPicker()
    {
      this._sections = this.ViewModel.Sections;
      this._picker = new ListPickerUC2()
      {
        ItemsSource = (IList) this._sections,
        PickerMaxWidth = 408.0,
        PickerMaxHeight = 768.0,
        BackgroundColor = (Brush) Application.Current.Resources["PhoneCardOverlayBrush"],
        PickerMargin = new Thickness(0.0, 0.0, 0.0, 64.0),
        ItemTemplate = (DataTemplate) base.Resources["FilterItemTemplate"]
      };
      this._picker.ItemTapped += (EventHandler<object>) ((sender, item) =>
      {
        DocumentsSectionViewModel section = item as DocumentsSectionViewModel;
        if (section == null)
          return;
        this.SelectSection(section);
        this.ViewModel.CurrentSection = section;
      });
      Point point = ((UIElement) this.rectSeparator).TransformToVisual((UIElement) this.gridRoot).Transform(new Point(0.0, 0.0));
      int num1 = this._sections.IndexOf((DocumentsSectionViewModel)Enumerable.FirstOrDefault<DocumentsSectionViewModel>(this._sections, (Func<DocumentsSectionViewModel, bool>)(section => section.IsSelected)));
      double num2 = 0.0;
      if (num1 > -1)
        num2 = (double) (num1 * 64);
      // ISSUE: explicit reference operation
      this._picker.Show(new Point(8.0, Math.Max(32.0, ((Point) @point).Y - num2)), (FrameworkElement) FramePageUtils.CurrentPage);
    }

    private void SelectSection(DocumentsSectionViewModel section)
    {
      if (this._sections == null || section == null)
        return;
      using (IEnumerator<DocumentsSectionViewModel> enumerator = this._sections.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DocumentsSectionViewModel current = enumerator.Current;
          int num = current.SectionId == section.SectionId ? 1 : 0;
          current.IsSelected = num != 0;
        }
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/DocumentsPickerPage.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.gridContent = (Grid) base.FindName("gridContent");
      this.list = (ExtendedLongListSelector) base.FindName("list");
      this.rectSeparator = (Rectangle) base.FindName("rectSeparator");
      this.pullToRefresh = (PullToRefreshUC) base.FindName("pullToRefresh");
    }
  }
}
