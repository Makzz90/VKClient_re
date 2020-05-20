using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileAudiosViewModel : ViewModelBase, IMediaVerticalItemsViewModel
  {
    private IProfileData _profileData;
    private bool _isGroup;
    private List<MediaListItemViewModelBase> _items;

    public string Title
    {
      get
      {
        return CommonResources.Profile_Audios;
      }
    }

    public int Count
    {
      get
      {
        if (this._profileData == null || this._profileData.counters == null)
          return 0;
        return this._profileData.counters.audios;
      }
    }

    public bool CanDisplay
    {
      get
      {
        return this.Count > 0;
      }
    }

    public List<MediaListItemViewModelBase> Items
    {
      get
      {
        return this._items;
      }
      private set
      {
        this._items = value;
        this.NotifyPropertyChanged("Items");
      }
    }

    public bool IsAllItemsVisible
    {
      get
      {
        return true;
      }
    }

    public Action HeaderTapAction
    {
      get
      {
        return (Action) (() => Navigator.Current.NavigateToAudio(0, this._profileData.Id, this._isGroup, 0, 0, ""));
      }
    }

    public Action<MediaListItemViewModelBase> ItemTapAction
    {
      get
      {
        return  null;
      }
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this._isGroup = profileData is GroupData;
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if (this._profileData.audios != null && this._profileData.audios.Count > 0)
          itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>)Enumerable.Select<AudioObj, AudioMediaListItemViewModel>(this._profileData.audios, (Func<AudioObj, AudioMediaListItemViewModel>)(audio => new AudioMediaListItemViewModel(audio, this._profileData.audios))));
      this.Items = new List<MediaListItemViewModelBase>((IEnumerable<MediaListItemViewModelBase>) itemViewModelBaseList);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.Count);
    }
  }
}
