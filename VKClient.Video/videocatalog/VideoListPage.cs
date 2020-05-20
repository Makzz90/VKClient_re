using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
  public class VideoListPage : PageBase
  {
    private bool _isInitialized;
    private string _section_id;
    private string _next;
    private string _name;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    private VideoListViewModel VM
    {
      get
      {
        return base.DataContext as VideoListViewModel;
      }
    }

    public VideoListPage()
    {
        this.InitializeComponent();
        this.ucHeader.OnHeaderTap = delegate
        {
            this.listBox.ScrollToTop();
        };
        this.ucPullToRefresh.TrackListBox(this.listBox);
        this.listBox.OnRefresh = delegate
        {
            this.VM.VideosGenCol.LoadData(true, false, null, false);
        };
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._section_id = ((Page) this).NavigationContext.QueryString["SectionId"];
      this._next = ((Page) this).NavigationContext.QueryString["Next"];
      this._name = ((Page) this).NavigationContext.QueryString["Name"];
      StatisticsActionSource source = (StatisticsActionSource) int.Parse(((Page) this).NavigationContext.QueryString["Source"]);
      string context = ((Page) this).NavigationContext.QueryString["Context"];
      VKList<VideoCatalogItem> vkList = ParametersRepository.GetParameterForIdAndReset("CatalogItemsToShow") as VKList<VideoCatalogItem>;
      int num1 = vkList != null ? 1 : 0;
      if (num1 == 0)
      {
        VideoListPage.VideoListData videoListData = new VideoListPage.VideoListData();
        CacheManager.TryDeserialize((IBinarySerializable) videoListData, this._section_id + "cached", CacheManager.DataType.CachedData);
        vkList = new VKList<VideoCatalogItem>()
        {
          count = videoListData.count,
          groups = videoListData.groups,
          profiles = videoListData.profiles,
          items = videoListData.items
        };
      }
      VideoListViewModel videoListViewModel = new VideoListViewModel(vkList ?? new VKList<VideoCatalogItem>(), this._section_id, this._next, this._name, source, context);
      base.DataContext = videoListViewModel;
      videoListViewModel.VideosGenCol.LoadData(true, false,  null, false);
      if (num1 != 0)
      {
        VideoListPage.VideoListData videoListData = new VideoListPage.VideoListData();
        videoListData.count = vkList.count;
        videoListData.groups = vkList.groups;
        videoListData.profiles = vkList.profiles;
        videoListData.items = vkList.items;
        string fileId = this._section_id + "cached";
        int num2 = 0;
        CacheManager.TrySerializeAsync((IBinarySerializable) videoListData, fileId, num2 != 0, (CacheManager.DataType) 0);
      }
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.VideosGenCol.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/VideoListPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }

    public class VideoListData : IBinarySerializable
    {
      public List<User> profiles { get; set; }

      public List<Group> groups { get; set; }

      public List<VideoCatalogItem> items { get; set; }

      public int count { get; set; }

      public void Write(BinaryWriter writer)
      {
        writer.Write(1);
        writer.WriteList<User>((IList<User>) this.profiles, 10000);
        writer.WriteList<Group>((IList<Group>) this.groups, 10000);
        writer.WriteList<VideoCatalogItem>((IList<VideoCatalogItem>) this.items, 10000);
        writer.Write(this.count);
      }

      public void Read(BinaryReader reader)
      {
        reader.ReadInt32();
        this.profiles = reader.ReadList<User>();
        this.groups = reader.ReadList<Group>();
        this.items = reader.ReadList<VideoCatalogItem>();
        this.count = reader.ReadInt32();
      }
    }
  }
}
