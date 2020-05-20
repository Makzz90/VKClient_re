using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;
using VKClient.Video.Localization;

namespace VKClient.Video.VideoCatalog
{
  public class VideoListViewModel : ViewModelBase, ICollectionDataProvider<GetCatalogSectionResponse, CatalogItemViewModel>
  {
    private bool _isVideos = true;
    private GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> _videosGenCol;
    private string _sectionId;
    private VKList<VideoCatalogItem> _list;
    private string _next;
    private bool _shouldLoadMore;
    private string _name;
    private StatisticsActionSource _source;
    private string _context;

    public GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> VideosGenCol
    {
      get
      {
        return this._videosGenCol;
      }
    }

    public string Title
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this._name))
          return this._name.ToUpperInvariant();
        return CommonResources.Videos.ToUpperInvariant();
      }
    }

    public Func<GetCatalogSectionResponse, ListWithCount<CatalogItemViewModel>> ConverterFunc
    {
      get
      {
        return (Func<GetCatalogSectionResponse, ListWithCount<CatalogItemViewModel>>) (vList =>
        {
          ListWithCount<CatalogItemViewModel> listWithCount = new ListWithCount<CatalogItemViewModel>();
          foreach (VideoCatalogItem videoCatalogItem in vList.items)
            listWithCount.List.Add(new CatalogItemViewModel(videoCatalogItem, vList.profiles, vList.groups, false)
            {
              ActionSource = new StatisticsActionSource?(this._source),
              VideoContext = this._context
            });
          return listWithCount;
        });
      }
    }

    public VideoListViewModel(VKList<VideoCatalogItem> list, string sectionId, string next, string name, StatisticsActionSource source, string context)
    {
      this._list = list;
      this._sectionId = sectionId;
      this._next = next;
      this._name = name;
      this._shouldLoadMore = !string.IsNullOrEmpty(this._next);
      this._source = source;
      this._context = context;
      this._videosGenCol = new GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel>((ICollectionDataProvider<GetCatalogSectionResponse, CatalogItemViewModel>) this);
    }

    public void GetData(GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> caller, int offset, int count, Action<BackendResult<GetCatalogSectionResponse, ResultCode>> callback)
    {
      if (offset == 0)
        callback(new BackendResult<GetCatalogSectionResponse, ResultCode>(ResultCode.Succeeded, new GetCatalogSectionResponse()
        {
          groups = this._list.groups,
          items = this._list.items,
          profiles = this._list.profiles
        }));
      else if (this._shouldLoadMore)
        VideoService.Instance.GetVideoCatalogSection(this._sectionId, offset > 0 ? this._next : "", (Action<BackendResult<GetCatalogSectionResponse, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            this._next = res.ResultData.next;
            if (res.ResultData.items.Any<VideoCatalogItem>())
              this._isVideos = res.ResultData.items.First<VideoCatalogItem>().type == "video";
          }
          callback(res);
        }));
      else
        callback(new BackendResult<GetCatalogSectionResponse, ResultCode>(ResultCode.Succeeded, new GetCatalogSectionResponse()
        {
          groups = new List<Group>(),
          items = new List<VideoCatalogItem>(),
          profiles = new List<User>()
        }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GetCatalogSectionResponse, CatalogItemViewModel> caller, int count)
    {
      if (count > 0)
        return UIStringFormatterHelper.FormatNumberOfSomething(count, this._isVideos ? CommonResources.OneVideoFrm : VideoResources.OneAlbumFrm, this._isVideos ? CommonResources.TwoFourVideosFrm : VideoResources.TwoFourAlbumsFrm, this._isVideos ? CommonResources.FiveVideosFrm : VideoResources.FiveAlbumsFrm, true, null, false);
      if (!this._isVideos)
        return VideoResources.NoAlbums;
      return CommonResources.NoVideos;
    }
  }
}
