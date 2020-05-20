using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Groups.Management.Library
{
  public sealed class ServicesViewModel : ViewModelStatefulBase
  {
    private bool _isFormEnabled = true;
    private string _keyWords = "";
    private readonly long _communityId;
    private CommunitySettings _information;
    private CommunityServiceState _wallOrComments;
    private CommunityServiceState _photos;
    private CommunityServiceState _videos;
    private CommunityServiceState _audios;
    private CommunityServiceState _documents;
    private CommunityServiceState _discussions;
    private bool _links;
    private bool _events;
    private bool _contacts;
    private bool _isStrongLanguageFilterEnabled;
    private bool _isKeyWordsFilterEnabled;

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
      }
    }

    public CommunityServiceState WallOrComments
    {
      get
      {
        return this._wallOrComments;
      }
      set
      {
        this._wallOrComments = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.WallOrComments));
        this.NotifyPropertyChanged<bool>((() => this.IsCommentsChecked));
        this.NotifyPropertyChanged<string>((() => this.WallStateString));
      }
    }

    public CommunityServiceState Photos
    {
      get
      {
        return this._photos;
      }
      set
      {
        this._photos = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.Photos));
        this.NotifyPropertyChanged<bool>((() => this.IsPhotosChecked));
        this.NotifyPropertyChanged<string>((() => this.PhotosStateString));
      }
    }

    public CommunityServiceState Videos
    {
      get
      {
        return this._videos;
      }
      set
      {
        this._videos = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.Videos));
        this.NotifyPropertyChanged<bool>((() => this.IsVideosChecked));
        this.NotifyPropertyChanged<string>((() => this.VideosStateString));
      }
    }

    public CommunityServiceState Audios
    {
      get
      {
        return this._audios;
      }
      set
      {
        this._audios = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.Audios));
        this.NotifyPropertyChanged<bool>((() => this.IsAudiosChecked));
        this.NotifyPropertyChanged<string>((() => this.AudiosStateString));
      }
    }

    public CommunityServiceState Documents
    {
      get
      {
        return this._documents;
      }
      set
      {
        this._documents = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.Documents));
        this.NotifyPropertyChanged<string>((() => this.DocumentsStateString));
      }
    }

    public CommunityServiceState Discussions
    {
      get
      {
        return this._discussions;
      }
      set
      {
        this._discussions = value;
        this.NotifyPropertyChanged<CommunityServiceState>((() => this.Discussions));
        this.NotifyPropertyChanged<bool>((() => this.IsDiscussionsChecked));
        this.NotifyPropertyChanged<string>((() => this.DiscussionsStateString));
      }
    }

    public bool Links
    {
      get
      {
        return this._links;
      }
      set
      {
        this._links = value;
        this.NotifyPropertyChanged<bool>((() => this.Links));
      }
    }

    public bool Events
    {
      get
      {
        return this._events;
      }
      set
      {
        this._events = value;
        this.NotifyPropertyChanged<bool>((() => this.Events));
      }
    }

    public bool Contacts
    {
      get
      {
        return this._contacts;
      }
      set
      {
        this._contacts = value;
        this.NotifyPropertyChanged<bool>((() => this.Contacts));
      }
    }

    public Visibility DetailedFormVisibility
    {
      get
      {
        CommunitySettings information = this._information;
        return (information == null || information.Type != GroupType.PublicPage).ToVisiblity();
      }
    }

    public Visibility SimpleFormVisibility
    {
      get
      {
        CommunitySettings information = this._information;
        return (information != null && information.Type == GroupType.PublicPage).ToVisiblity();
      }
    }

    public bool IsCommentsChecked
    {
      get
      {
        return this.WallOrComments == CommunityServiceState.Opened;
      }
      set
      {
        this.WallOrComments = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
      }
    }

    public bool IsPhotosChecked
    {
      get
      {
        return this.Photos == CommunityServiceState.Opened;
      }
      set
      {
        this.Photos = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
      }
    }

    public bool IsVideosChecked
    {
      get
      {
        return this.Videos == CommunityServiceState.Opened;
      }
      set
      {
        this.Videos = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
      }
    }

    public bool IsAudiosChecked
    {
      get
      {
        return this.Audios == CommunityServiceState.Opened;
      }
      set
      {
        this.Audios = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
      }
    }

    public bool IsDiscussionsChecked
    {
      get
      {
        return this.Discussions == CommunityServiceState.Opened;
      }
      set
      {
        this.Discussions = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
      }
    }

    public string WallStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.WallOrComments, true);
      }
    }

    public string PhotosStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.Photos, false);
      }
    }

    public string VideosStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.Videos, false);
      }
    }

    public string AudiosStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.Audios, false);
      }
    }

    public string DocumentsStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.Documents, false);
      }
    }

    public string DiscussionsStateString
    {
      get
      {
        return ServicesViewModel.GetStateString(this.Discussions, false);
      }
    }

    public bool IsStrongLanguageFilterEnabled
    {
      get
      {
        return this._isStrongLanguageFilterEnabled;
      }
      set
      {
        this._isStrongLanguageFilterEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsStrongLanguageFilterEnabled));
      }
    }

    public bool IsKeyWordsFilterEnabled
    {
      get
      {
        return this._isKeyWordsFilterEnabled;
      }
      set
      {
        this._isKeyWordsFilterEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsKeyWordsFilterEnabled));
        this.NotifyPropertyChanged<Visibility>((() => this.KeyWordsFieldVisibility));
      }
    }

    public string KeyWords
    {
      get
      {
        return this._keyWords;
      }
      set
      {
        this._keyWords = value;
        this.NotifyPropertyChanged<string>((() => this.KeyWords));
      }
    }

    public Visibility KeyWordsFieldVisibility
    {
      get
      {
        return this.IsKeyWordsFilterEnabled.ToVisiblity();
      }
    }

    public ServicesViewModel(long communityId)
    {
      this._communityId = communityId;
    }

    public override void Load(Action<ResultCode> callback)
    {
        GroupsService.Current.GetCommunitySettings(this._communityId, (Action<BackendResult<CommunitySettings, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            ResultCode resultCode = result.ResultCode;
            if (resultCode == ResultCode.Succeeded)
            {
                this._information = result.ResultData;
                this.WallOrComments = (CommunityServiceState)this._information.wall;
                if (this._information.Type != GroupType.PublicPage)
                {
                    this.Documents = (CommunityServiceState)this._information.docs;
                }
                else
                {
                    this.Links = this._information.links == 1;
                    this.Events = this._information.events == 1;
                    this.Contacts = this._information.contacts == 1;
                }
                this.Photos = (CommunityServiceState)this._information.photos;
                this.Videos = (CommunityServiceState)this._information.video;
                this.Audios = (CommunityServiceState)this._information.audio;
                this.Discussions = (CommunityServiceState)this._information.topics;
                this.NotifyPropertyChanged<Visibility>((() => this.DetailedFormVisibility));
                this.NotifyPropertyChanged<Visibility>((() => this.SimpleFormVisibility));
                this.IsStrongLanguageFilterEnabled = this._information.obscene_filter == 1;
                this.IsKeyWordsFilterEnabled = this._information.obscene_stopwords == 1;
                if (this._information.obscene_words != null)
                {
                    foreach (string obsceneWord in this._information.obscene_words)
                        this.KeyWords = !(this.KeyWords == "") ? this.KeyWords + ", " + obsceneWord : obsceneWord;
                }
            }
            callback(resultCode);
        }))));
    }

    public void SaveChanges()
    {
        this.SetInProgress(true, "");
        this.IsFormEnabled = false;
        GroupsService.Current.SetCommunityServices(this._communityId, (int)this.WallOrComments, (int)this.Photos, (int)this.Videos, (int)this.Audios, (int)this.Documents, (int)this.Discussions, this.Links ? 1 : 0, this.Events ? 1 : 0, this.Contacts ? 1 : 0, this.IsStrongLanguageFilterEnabled ? 1 : 0, this.IsKeyWordsFilterEnabled ? 1 : 0, this.KeyWords, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                EventAggregator.Current.Publish((object)new CommunityServicesChanged()
                {
                    Id = this._communityId
                });
                Navigator.Current.GoBack();
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }
        }))));
    }

    private static string GetStateString(CommunityServiceState state, bool isWall)
    {
      switch (state)
      {
        case CommunityServiceState.Disabled:
          if (!isWall)
            return CommonResources.Disabled_Form2;
          return CommonResources.Disabled_Form1;
        case CommunityServiceState.Opened:
          if (!isWall)
            return CommonResources.Opened_Form2;
          return CommonResources.Opened_Form1;
        case CommunityServiceState.Limited:
          if (!isWall)
            return CommonResources.Limited_Form2;
          return CommonResources.Limited_Form1;
        case CommunityServiceState.Closed:
          return CommonResources.Closed;
        default:
          return "";
      }
    }
  }
}
