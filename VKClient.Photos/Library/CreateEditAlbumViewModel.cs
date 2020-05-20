using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
  public class CreateEditAlbumViewModel : ViewModelBase
  {
    private bool _isNewMode;
    private AccessType _accessType;
    private AccessType _accessTypeComments;
    private string _name;
    private string _description;
    private Album _album;
    private Action<Album> _notifyOnCompletion;
    private long _gid;
    private EditPrivacyViewModel _privacyViewVM;
    private bool _isBusy;

    public string Caption
    {
      get
      {
        if (!this._isNewMode)
          return PhotoResources.CreateAlbumUC_EditAlbum;
        return PhotoResources.CreateAlbumUC_CreateAlbum;
      }
    }

    public string ButtonText
    {
      get
      {
        if (!this._isNewMode)
          return PhotoResources.CreateAlbumUC_Save;
        return PhotoResources.CreateAlbumUC_Create;
      }
    }

    public string Name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = value;
        this.NotifyPropertyChanged<string>((() => this.Name));
      }
    }

    public string Description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = value;
        this.NotifyPropertyChanged<string>((() => this.Description));
      }
    }

    public List<AccessType> AccessTypes
    {
      get
      {
        if (this._gid != 0L)
          return AccessTypesList.AccessTypesGroupAlbums;
        return AccessTypesList.AccessTypes;
      }
    }

    public AccessType AccessType
    {
      get
      {
        return this._accessType;
      }
      set
      {
        this._accessType = value;
        this.NotifyPropertyChanged<AccessType>((Expression<Func<AccessType>>) (() => this.AccessType));
      }
    }

    public AccessType AccessTypeComments
    {
      get
      {
        return this._accessTypeComments;
      }
      set
      {
        this._accessTypeComments = value;
        this.NotifyPropertyChanged<AccessType>((Expression<Func<AccessType>>) (() => this.AccessTypeComments));
      }
    }

    public Visibility IsUserAlbumVisibility
    {
      get
      {
        if (this._gid != 0L)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility IsGroupAlbumVisibility
    {
      get
      {
        if (this._gid == 0L)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public EditPrivacyViewModel PrivacyViewVM
    {
      get
      {
        return this._privacyViewVM;
      }
      set
      {
        this._privacyViewVM = value;
        this.NotifyPropertyChanged<EditPrivacyViewModel>((Expression<Func<EditPrivacyViewModel>>) (() => this.PrivacyViewVM));
      }
    }

    public CreateEditAlbumViewModel(Album album, Action<Album> notifyOnCompletion, long gid = 0)
    {
        this._album = album;
        this._isNewMode = string.IsNullOrEmpty(album.aid);
        this.Name = this._album.title ?? "";
        this.Description = this._album.description ?? "";
        this.AccessTypeComments = AccessTypesList.AccessTypes.FirstOrDefault<AccessType>((Func<AccessType, bool>)(a => a.Id == this._album.comment_privacy));
        this._notifyOnCompletion = notifyOnCompletion;
        this._gid = gid;
        this._privacyViewVM = new EditPrivacyViewModel(PhotoResources.CreateAlbumUC_Access, this._album.PrivacyViewInfo, "", (List<string>)null);
    }

    public void Save()
    {
      if (this._isBusy)
        return;
      this._isBusy = true;
      this.SetInProgressMain(true, "");
      Album album = this._album.Copy();
      album.title = this.Name;
      album.description = UIStringFormatterHelper.CorrectNewLineCharacters(this.Description);
      album.privacy_view = this.PrivacyViewVM.GetAsPrivacyInfo().ToStringList();
      album.comment_privacy = this.AccessTypeComments != null ? this.AccessTypeComments.Id : 0;
      PhotosService photosService = new PhotosService();
      if (this._isNewMode)
        photosService.CreateAlbum(album, (Action<BackendResult<Album, ResultCode>>) (res =>
        {
          this._isBusy = false;
          this.SetInProgressMain(false, "");
          if (res.ResultCode == ResultCode.Succeeded)
            this._notifyOnCompletion(res.ResultData);
          else
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }), this._gid);
      else
        photosService.EditAlbum(album, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          this._isBusy = false;
          this.SetInProgressMain(false, "");
          if (res.ResultCode == ResultCode.Succeeded)
            this._notifyOnCompletion(album);
          else
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }), this._gid);
    }
  }
}
