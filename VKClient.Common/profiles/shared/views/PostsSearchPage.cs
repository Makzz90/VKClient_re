using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class PostsSearchPage : PageBase
  {
    private bool _isInitialized;
    internal NewsSearchUC ucNewsSearch;
    private bool _contentLoaded;

    public PostsSearchPage()
    {
      this.InitializeComponent();
      this.SuppressOpenMenuTapArea = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        base.DataContext = (new ViewModelBase());
        string str1 = "";
        long result;
        if (long.TryParse(((Page) this).NavigationContext.QueryString["OwnerId"], out result))
          this.ucNewsSearch.Init(result, "", true);
        if (((Page) this).NavigationContext.QueryString.ContainsKey("NameGen"))
          str1 = ((Page) this).NavigationContext.QueryString["NameGen"];
        string str2 = "";
        if (result > 0L)
        {
          if (!string.IsNullOrEmpty(str1))
            str2 = string.Format(CommonResources.SearchByUserPosts, str1);
        }
        else
          str2 = CommonResources.SearchByCommunityPosts;
        if (!string.IsNullOrEmpty(str2))
          this.ucNewsSearch.textBlockWatermarkText.Text = str2;
        this._isInitialized = true;
      }
      CurrentMediaSource.VideoSource = StatisticsActionSource.search;
      CurrentMediaSource.AudioSource = StatisticsActionSource.search;
      CurrentMediaSource.GifPlaySource = StatisticsActionSource.search;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/PostsSearchPage.xaml", UriKind.Relative));
      this.ucNewsSearch = (NewsSearchUC) base.FindName("ucNewsSearch");
    }
  }
}
