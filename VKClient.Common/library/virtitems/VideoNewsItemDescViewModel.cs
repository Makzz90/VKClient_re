using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;

namespace VKClient.Common.Library.VirtItems
{
  public class VideoNewsItemDescViewModel
  {
    private bool _isBig;
    private VKClient.Common.Backend.DataObjects.Video _video;
    private CatalogItemViewModel _catItemVM;

    public Visibility BigPreviewVisibility
    {
      get
      {
        if (!this._isBig)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SmallPreviewVisibility
    {
      get
      {
        if (this._isBig)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility ShowPlaySmallIconVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public Visibility ShowDurationVisibility
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.UIDuration))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility IsLiveVisibility
    {
      get
      {
        return this._catItemVM.IsLiveVisibility;
      }
    }

    public string Title
    {
      get
      {
        return this._video.title;
      }
    }

    public string Subtitle
    {
      get
      {
        int views = this._video.views;
        if (views <= 0)
          return "";
        return UIStringFormatterHelper.FormatNumberOfSomething(views, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true,  null, false);
      }
    }

    public string UIDuration
    {
      get
      {
        return this._catItemVM.UIDuration;
      }
    }

    public VideoNewsItemDescViewModel(VKClient.Common.Backend.DataObjects.Video video, bool isBig)
    {
      this._video = video;
      this._catItemVM = new CatalogItemViewModel(new VideoCatalogItem(video), new List<User>(), new List<Group>(), false);
      this._isBig = isBig;
    }
  }
}
