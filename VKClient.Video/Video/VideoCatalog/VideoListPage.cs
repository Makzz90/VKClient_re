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
    public partial class VideoListPage : PageBase
  {
    private bool _isInitialized;
    private string _section_id;
    private string _next;
    private string _name;

    private VideoListViewModel VM
    {
      get
      {
        return this.DataContext as VideoListViewModel;
      }
    }

    public VideoListPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() => this.VM.VideosGenCol.LoadData(true, false, (Action<BackendResult<GetCatalogSectionResponse, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._section_id = this.NavigationContext.QueryString["SectionId"];
      this._next = this.NavigationContext.QueryString["Next"];
      this._name = this.NavigationContext.QueryString["Name"];
      StatisticsActionSource source = (StatisticsActionSource) int.Parse(this.NavigationContext.QueryString["Source"]);
      string context = this.NavigationContext.QueryString["Context"];
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
      this.DataContext = (object) videoListViewModel;
      videoListViewModel.VideosGenCol.LoadData(true, false, (Action<BackendResult<GetCatalogSectionResponse, ResultCode>>) null, false);
      if (num1 != 0)
      {
        VideoListPage.VideoListData videoListData = new VideoListPage.VideoListData();
        videoListData.count = vkList.count;
        videoListData.groups = vkList.groups;
        videoListData.profiles = vkList.profiles;
        videoListData.items = vkList.items;
        string fileId = this._section_id + "cached";
        int num2 = 0;
        int num3 = 0;
        CacheManager.TrySerializeAsync((IBinarySerializable) videoListData, fileId, num2 != 0, (CacheManager.DataType) num3);
      }
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.VideosGenCol.LoadMoreIfNeeded(e.ContentPresenter.Content);
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
