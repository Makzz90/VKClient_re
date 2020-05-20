using System;
using System.Collections.Generic;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class SubscriptionsMediaListItemViewModel : MediaListItemViewModelBase
  {
    private const int MAX_URI_COUNT = 6;
    private readonly List<string> _photos;

    public List<string> ImageUris { get; private set; }

    public override string Id
    {
      get
      {
        if (this._photos == null || this._photos.Count <= 0)
          return "";
        return string.Join(",", (IEnumerable<string>) this._photos);
      }
    }

    public SubscriptionsMediaListItemViewModel(List<string> photos)
      : base(ProfileMediaListItemType.Subscriptions)
    {
      this._photos = photos;
      this.ImageUris = new List<string>()
      {
        "",
        "",
        "",
        "",
        "",
        ""
      };
      if (photos == null)
        return;
      int num = Math.Min(photos.Count, 6);
      for (int index = 0; index < num; ++index)
        this.ImageUris[index] = photos[index];
    }
  }
}
