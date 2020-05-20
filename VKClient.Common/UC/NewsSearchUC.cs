using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class NewsSearchUC : UserControl
    {
        private readonly DelayedExecutor _de;
        private bool _focusOnLoad;
        private ISearchWallPostsViewModel _viewModel;
        internal TextBox textBoxSearch;
        internal TextBlock textBlockWatermarkText;
        internal ViewportControl scrollNews;
        internal MyVirtualizingStackPanel stackPanel;
        internal MyVirtualizingPanel2 panelNews;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        public TextBox TextBoxSearch
        {
            get
            {
                return this.textBoxSearch;
            }
        }

        public NewsSearchUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            this.textBlockWatermarkText.Text = (CommonResources.NewsSearch.ToLowerInvariant());
        }

        public void Init(long ownerId = 0, string query = "", bool focusOnLoad = true)
        {
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.NewsSearchUC_Loaded));
            if (ownerId == 0L)
            {
                string tag;
                string domain;
                NewsSearchUC.TryGetProfileDomain(query, out tag, out domain);
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(domain))
                {
                    query = tag;
                    this._viewModel = (ISearchWallPostsViewModel)new PostsSearchViewModel(domain);
                }
                else
                {
                    this._viewModel = (ISearchWallPostsViewModel)new NewsSearchViewModel();
                    EventAggregator.Current.Publish(new DiscoverActionEvent()
                    {
                        ActionType = DiscoverActionType.view
                    });
                }
            }
            else
                this._viewModel = (ISearchWallPostsViewModel)new PostsSearchViewModel(ownerId);
            base.DataContext = this._viewModel;
            this.scrollNews.BindViewportBoundsTo((FrameworkElement)this.stackPanel);
            this.panelNews.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.scrollNews), false);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.panelNews);
            this.panelNews.OnRefresh = new Action(this._viewModel.Refresh);
            this._focusOnLoad = focusOnLoad;
            if (!string.IsNullOrEmpty(query))
            {
                this.TextBoxSearch.Text = query;
                this.TextBoxSearch.SelectionStart = query.Length;
                this.textBoxSearch_TextChanged_1(this, null);
            }
            // ISSUE: method pointer
            this.TextBoxSearch.TextChanged += (new TextChangedEventHandler(this.textBoxSearch_TextChanged_1));
        }

        private static void TryGetProfileDomain(string str, out string tag, out string domain)
        {
            tag = "";
            domain = "";
            foreach (Match match in BrowserNavigationService.Regex_Tag.Matches(str))
            {
                GroupCollection groups = match.Groups;
                if (groups.Count == 6)
                {
                    tag = groups[2].Value;
                    domain = groups[5].Value;
                }
            }
        }

        private void NewsSearchUC_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._viewModel.ItemsCount != 0 || !string.IsNullOrEmpty(this.TextBoxSearch.Text) || !this._focusOnLoad)
                return;
            this._de.AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() => ((Control)this.textBoxSearch).Focus()))));
        }

        private void textBoxSearch_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            this._viewModel.Search(this.textBoxSearch.Text);
            ((UIElement)this.textBlockWatermarkText).Visibility = (string.IsNullOrEmpty(this.textBoxSearch.Text).ToVisiblity());
        }

        private void scrollNews_ManipulationStarted_1(object sender, ManipulationStartedEventArgs e)
        {
            ((Control)this.scrollNews).Focus();
        }

        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !(this.textBoxSearch.Text != string.Empty))
                return;
            ((Control)this.scrollNews).Focus();
            this._viewModel.Refresh();
        }

        private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxSearch.SelectAll();
        }

        private void Trend_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            Trend trend = (frameworkElement != null ? frameworkElement.DataContext : null) as Trend;
            if (trend == null)
                return;
            string name = trend.name;
            this.TextBoxSearch.Text = name;
            EventAggregator.Current.Publish(new DiscoverActionEvent()
            {
                ActionType = DiscoverActionType.click_trending,
                ActionParam = name
            });
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsSearchUC.xaml", UriKind.Relative));
            this.textBoxSearch = (TextBox)base.FindName("textBoxSearch");
            this.textBlockWatermarkText = (TextBlock)base.FindName("textBlockWatermarkText");
            this.scrollNews = (ViewportControl)base.FindName("scrollNews");
            this.stackPanel = (MyVirtualizingStackPanel)base.FindName("stackPanel");
            this.panelNews = (MyVirtualizingPanel2)base.FindName("panelNews");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }
    }
}
