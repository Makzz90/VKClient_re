using Microsoft.Phone.Controls;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
    public class RecommendedGroupsPage : PageBase
    {
        private bool _isInitialized;
        //private bool _isCategoryLoaded;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal Pivot pivot;
        internal PivotItem pivotItemRecommendations;
        internal ExtendedLongListSelector recommendationsListBox;
        internal PivotItem pivotItemCatalog;
        internal ExtendedLongListSelector catalogListBox;
        private bool _contentLoaded;

        private RecommendedGroupsViewModel VM
        {
            get
            {
                return base.DataContext as RecommendedGroupsViewModel;
            }
        }

        public RecommendedGroupsPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                RecommendedGroupsViewModel recommendedGroupsViewModel = new RecommendedGroupsViewModel(int.Parse(((Page)this).NavigationContext.QueryString["CategoryId"]), ((Page)this).NavigationContext.QueryString["CategoryName"]);
                base.DataContext = recommendedGroupsViewModel;
                recommendedGroupsViewModel.Recommendations.LoadData(false, false, null, false);
                this.UpdateCatalogVisibility();
                recommendedGroupsViewModel.CatalogCategories.Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Collection_CollectionChanged);
                this._isInitialized = true;
            }
            CurrentCommunitySource.Source = CommunityOpenSource.Recommendations;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateCatalogVisibility();
        }

        private void UpdateCatalogVisibility()
        {
            bool flag = this.VM.CatalogCategories.Collection.Count > 0;
            if (flag && !((PresentationFrameworkCollection<object>)((ItemsControl)this.pivot).Items).Contains(this.pivotItemCatalog))
            {
                ((PresentationFrameworkCollection<object>)((ItemsControl)this.pivot).Items).Add(this.pivotItemCatalog);
            }
            else
            {
                if (flag || !((PresentationFrameworkCollection<object>)((ItemsControl)this.pivot).Items).Contains(this.pivotItemCatalog))
                    return;
                ((PresentationFrameworkCollection<object>)((ItemsControl)this.pivot).Items).Remove(this.pivotItemCatalog);
            }
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CatalogCategoryHeader dataContext = (sender as FrameworkElement).DataContext as CatalogCategoryHeader;
            if (dataContext == null)
                return;
            Navigator.Current.NavigateToGroupRecommendations(dataContext.CategoryId, dataContext.Title);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/RecommendedGroupsPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.pivot = (Pivot)base.FindName("pivot");
            this.pivotItemRecommendations = (PivotItem)base.FindName("pivotItemRecommendations");
            this.recommendationsListBox = (ExtendedLongListSelector)base.FindName("recommendationsListBox");
            this.pivotItemCatalog = (PivotItem)base.FindName("pivotItemCatalog");
            this.catalogListBox = (ExtendedLongListSelector)base.FindName("catalogListBox");
        }
    }
}
