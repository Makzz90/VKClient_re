using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common
{
    public class DocumentsPage : PageBase
    {
        private readonly ApplicationBarIconButton _appBarAddButton;
        private bool _isInitialized;
        private bool _isSearchNow;
        private int _loadedListsCount;
        internal GenericHeaderUC header;
        internal Pivot pivot;
        internal PullToRefreshUC pullToRefresh;
        private bool _contentLoaded;

        private DocumentsViewModel ViewModel
        {
            get
            {
                return ((FrameworkElement)this).DataContext as DocumentsViewModel;
            }
        }

        public DocumentsPage()
        {
            this.InitializeComponent();
            this.ApplicationBar = ((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string friendsPageAppBarAdd = CommonResources.FriendsPage_AppBar_Add;
            applicationBarIconButton1.Text = friendsPageAppBarAdd;
            this._appBarAddButton = applicationBarIconButton1;
            this._appBarAddButton.Click += (new EventHandler(DocumentsPage.AppBarAddButton_OnClicked));
            this.ApplicationBar.Buttons.Add(this._appBarAddButton);
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
            applicationBarIconButton2.Text = pageAppBarSearch;
            ApplicationBarIconButton applicationBarIconButton3 = applicationBarIconButton2;
            applicationBarIconButton3.Click += (new EventHandler(this.AppBarSearchButton_OnClicked));
            this.ApplicationBar.Buttons.Add(applicationBarIconButton3);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                long ownerId = 0;
                bool isOwnerCommunityAdmined = false;
                if (((Page)this).NavigationContext.QueryString.ContainsKey("OwnerId"))
                    ownerId = long.Parse(((Page)this).NavigationContext.QueryString["OwnerId"]);
                if (((Page)this).NavigationContext.QueryString.ContainsKey("IsOwnerCommunityAdmined"))
                    isOwnerCommunityAdmined = bool.Parse(((Page)this).NavigationContext.QueryString["IsOwnerCommunityAdmined"]);
                if (ownerId != AppGlobalStateManager.Current.LoggedInUserId && !isOwnerCommunityAdmined)
                    this.ApplicationBar.Buttons.Remove(this._appBarAddButton);
                DocumentsViewModel parentPageViewModel = new DocumentsViewModel(ownerId);
                parentPageViewModel.Sections.Add(new DocumentsSectionViewModel(parentPageViewModel, ownerId, 0, CommonResources.Header_ShowAll, isOwnerCommunityAdmined, false));
                parentPageViewModel.LoadSection(0);
                ((FrameworkElement)this).DataContext = parentPageViewModel;
                this._isInitialized = true;
            }
            else if (ParametersRepository.Contains("FilePicked"))
            {
                FileOpenPickerContinuationEventArgs parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
                StorageFile m0;
                if (parameterForIdAndReset == null)
                {
                    m0 = null;
                }
                else
                {
                    IReadOnlyList<StorageFile> files = parameterForIdAndReset.Files;
                    m0 = files != null ? Enumerable.FirstOrDefault<StorageFile>(files) : null;
                }
                StorageFile storageFile = (StorageFile)m0;
                if (storageFile == null)
                    return;
                this.ViewModel.UploadDocuments(new List<StorageFile>()
        {
          storageFile
        });
            }
            else
            {
                if (!ParametersRepository.Contains("PickedPhotoDocuments"))
                    return;
                List<StorageFile> parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments") as List<StorageFile>;
                if (parameterForIdAndReset == null || !Enumerable.Any<StorageFile>(parameterForIdAndReset))
                    return;
                this.ViewModel.UploadDocuments(parameterForIdAndReset);
            }
        }

        private void list_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExtendedLongListSelector extendedLongListSelector = (ExtendedLongListSelector)sender;
            DocumentHeader documentHeader = extendedLongListSelector.SelectedItem as DocumentHeader;
            if (documentHeader != null)
            {
                extendedLongListSelector.SelectedItem = (null);
                if (!documentHeader.IsGif)
                {
                    Navigator.Current.NavigateToWebUri(documentHeader.Document.url, true, false);
                    return;
                }
                InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
                List<PhotoOrDocument> documents = new List<PhotoOrDocument>();
                int num = 0;
                List<DocumentHeader> list = Enumerable.ToList<DocumentHeader>(this.ViewModel.Sections[this.pivot.SelectedIndex].Items.Collection);
                if (this._isSearchNow)
                {
                    ObservableCollection<Group<DocumentHeader>> groupedCollection = ((GenericCollectionViewModel2<VKList<Doc>, DocumentHeader>)extendedLongListSelector.DataContext).GroupedCollection;
                    list = new List<DocumentHeader>();
                    if (groupedCollection.Count > 0)
                    {
                        list = Enumerable.ToList<DocumentHeader>(groupedCollection[0]);
                    }
                    if (groupedCollection.Count > 1)
                    {
                        list.AddRange(Enumerable.ToList<DocumentHeader>(groupedCollection[1]));
                    }
                }
                IEnumerable<DocumentHeader> arg_103_0 = list;
                Func<DocumentHeader, bool> arg_103_1 = new Func<DocumentHeader, bool>((document) => { return document.IsGif; });

                using (IEnumerator<DocumentHeader> enumerator = Enumerable.Where<DocumentHeader>(arg_103_0, arg_103_1).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        DocumentHeader current = enumerator.Current;
                        if (current == documentHeader)
                        {
                            num = documents.Count;
                        }
                        documents.Add(new PhotoOrDocument
                        {
                            document = current.Document
                        });
                    }
                }
                Action<int> action = delegate(int i)
                {
                    if (documents[i].document != null)
                    {
                        InplaceGifViewerViewModel inplaceGifViewerViewModel = new InplaceGifViewerViewModel(documents[i].document, true, false, false);
                        gifViewer.VM = inplaceGifViewerViewModel;
                        inplaceGifViewerViewModel.Play(GifPlayStartType.manual);
                        gifViewer.Visibility = Visibility.Visible;
                        return;
                    }
                    InplaceGifViewerViewModel expr_58 = gifViewer.VM;
                    if (expr_58 != null)
                    {
                        expr_58.Stop();
                    }
                    gifViewer.Visibility = Visibility.Collapsed;
                };
                INavigator arg_1DA_0 = Navigator.Current;
                int arg_1DA_1 = num;
                List<PhotoOrDocument> arg_1DA_2 = documents;
                bool arg_1DA_3 = false;
                bool arg_1DA_4 = false;

                bool arg_1DA_7 = false;
                FrameworkElement arg_1DA_8 = gifViewer;
                Action<int> arg_1DA_9 = action;
                //Action<int, bool> arg_1DA_10 = new Action<int, bool>(DocumentsPage.<>c.<>9.<list_OnSelectionChanged>b__8_3));

                arg_1DA_0.NavigateToImageViewerPhotosOrGifs(arg_1DA_1, arg_1DA_2, arg_1DA_3, arg_1DA_4, null, this, arg_1DA_7, arg_1DA_8, arg_1DA_9, null, this.ViewModel.OwnerId == AppGlobalStateManager.Current.LoggedInUserId);
            }
        }


        private void list_OnLoaded(object sender, RoutedEventArgs e)
        {
            ExtendedLongListSelector list = (ExtendedLongListSelector)sender;
            int pivotItemIndex = this._loadedListsCount;
            list.Loaded -= (new RoutedEventHandler(this.list_OnLoaded));
            this._loadedListsCount++;
            GenericHeaderUC expr_50 = this.header;
            Action a = new Action(() =>
            {
                if (this.pivot.SelectedIndex == pivotItemIndex)
                {
                    list.ScrollToTop();
                }
            });
            expr_50.OnHeaderTap = (Action)Delegate.Combine(expr_50.OnHeaderTap, a);
            list.OnRefresh = delegate
            {
                this.ViewModel.Sections[this.pivot.SelectedIndex].Items.LoadData(true, false, null, false);
            };
            this.pullToRefresh.TrackListBox(list);
        }

        private void list_OnLinked(object sender, LinkUnlinkEventArgs e)
        {
            this.ViewModel.Sections[this.pivot.SelectedIndex].Items.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.LoadSection(this.pivot.SelectedIndex);
        }

        private static void AppBarAddButton_OnClicked(object sender, EventArgs e)
        {
            DocumentsPickerUC.Show(20);
        }

        private void AppBarSearchButton_OnClicked(object sender, EventArgs e)
        {
            DialogService expr_12 = new DialogService();
            expr_12.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            expr_12.AnimationType = DialogService.AnimationTypes.None;
            expr_12.HideOnNavigation = false;
            DocumentsSearchDataProvider searchDataProvider = new DocumentsSearchDataProvider(this.ViewModel.Sections[this.pivot.SelectedIndex].Items.Collection);
            DataTemplate itemTemplate = (DataTemplate)base.Resources["ItemTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC();
            searchUC.LayoutRootGrid.Margin = (new Thickness(0.0, 77.0, 0.0, 0.0));
            searchUC.Initialize<Doc, DocumentHeader>(searchDataProvider, delegate(object p, object f)
            {
                this.list_OnSelectionChanged(p, null);
            }, itemTemplate);
            searchUC.SearchTextBox.TextChanged += (delegate(object s, TextChangedEventArgs ev)
            {
                bool flag = searchUC.SearchTextBox.Text != "";
                this.pivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
            });
            expr_12.Closed += delegate(object p, EventArgs f)
            {
                this.pivot.Visibility = Visibility.Visible;
                this._isSearchNow = false;
            };
            this._isSearchNow = true;
            expr_12.Child = searchUC;
            expr_12.Show(this.pivot);
            this.InitializeAdornerControls();
        }

        private void item_OnEditButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            DocumentHeader menuItemDataContext = DocumentsPage.GetMenuItemDataContext(sender) as DocumentHeader;
            if (menuItemDataContext == null)
                return;
            Navigator.Current.NavigateToDocumentEditing(menuItemDataContext.Document.owner_id, menuItemDataContext.Document.id, menuItemDataContext.Document.title);
        }

        private void item_OnDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            DocumentHeader menuItemDataContext = DocumentsPage.GetMenuItemDataContext(sender) as DocumentHeader;
            if (menuItemDataContext == null || MessageBox.Show(CommonResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(1, CommonResources.Documents_DeleteOneFrm, CommonResources.Documents_DeleteTwoFourFrm, CommonResources.Documents_DeleteFiveFrm, true, null, false), MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.ViewModel.DeleteDocument(menuItemDataContext);
        }

        private static object GetMenuItemDataContext(object sender)
        {
            if (sender != null)
            {
                ContextMenu parent = ((FrameworkElement)sender).Parent as ContextMenu;
                if (parent != null)
                    return ((FrameworkElement)parent.Owner).DataContext;
            }
            return null;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/DocumentsPage.xaml", UriKind.Relative));
            this.header = (GenericHeaderUC)((FrameworkElement)this).FindName("header");
            this.pivot = (Pivot)((FrameworkElement)this).FindName("pivot");
            this.pullToRefresh = (PullToRefreshUC)((FrameworkElement)this).FindName("pullToRefresh");
        }
    }
}
