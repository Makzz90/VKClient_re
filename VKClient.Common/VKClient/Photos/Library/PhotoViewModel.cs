using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Photos.Library
{
  public class PhotoViewModel : ViewModelBase, ISupportCommentsAndLikes, ILikeable, IHandle<PhotoViewerOrientationLockedModeChanged>, IHandle
  {
    private int _knownCommentsCount = -1;
    public readonly int CountToLoad = 5;
    private readonly string _accessKey = "";
    private Photo _photo;
    private PhotoWithFullInfo _photoWithFullInfo;
    private bool _isLoading;
    private readonly long _ownerId;
    private readonly long _pid;
    private bool _isGifAdded;
    private bool _isSavingInSavedPhotos;
    private readonly Doc _doc;
    private bool _isAddingDoc;
    private bool _adding;

    public int KnownCommentsCount
    {
      get
      {
        return this._knownCommentsCount;
      }
    }

    public SolidColorBrush LikeBackgroundBrush
    {
      get
      {
        if (!this.UserLiked)
          return new SolidColorBrush("#66ffffff".ToColor());
        return new SolidColorBrush("#ff73a8e6".ToColor());
      }
    }

    public SolidColorBrush LikeTextForegroundBrush
    {
      get
      {
        if (!this.UserLiked)
          return new SolidColorBrush("#ccffffff".ToColor());
        return new SolidColorBrush("#ff73a8e6".ToColor());
      }
    }

    public int RealOffset
    {
      get
      {
        return this._photo.real_offset;
      }
    }

    public double LikeOpacity
    {
      get
      {
        return !this.UserLiked ? 0.6 : 1.0;
      }
    }

    public string LikesCountStr
    {
      get
      {
        Photo photo = this._photo;
        if ((photo != null ? photo.likes :  null) == null || this._photo.likes.count <= 0)
          return "";
        return UIStringFormatterHelper.FormatForUIShort((long) this._photo.likes.count);
      }
    }

    public Visibility IsFullInfoLoadedVisibility
    {
      get
      {
        return this.IsLoadedFullInfo.ToVisiblity();
      }
    }

    public double IsFullInfoLoadedOpacity
    {
      get
      {
        return 1.0;
      }
    }

    public Visibility CommentVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public string CommentsCountStr
    {
      get
      {
        Photo photo = this._photo;
        if ((photo != null ? photo.comments : (VKClient.Common.Backend.DataObjects.Comments) null) == null || this._photo.comments.count <= 0)
          return "";
        return UIStringFormatterHelper.FormatForUIShort((long) this._photo.comments.count);
      }
    }

    public bool IsUserVisible
    {
      get
      {
        return this.PhotoTags.Count > 0;
      }
    }

    public Visibility UserVisibility
    {
      get
      {
        return this.IsUserVisible.ToVisiblity();
      }
    }

    public string UserCountStr
    {
      get
      {
        return UIStringFormatterHelper.FormatForUIShort((long) this.PhotoTags.Count);
      }
    }

    public Photo Photo
    {
      get
      {
        return this._photo;
      }
      set
      {
        this._photo = value;
      }
    }

    public bool IsLoadedFullInfo
    {
      get
      {
        return this._knownCommentsCount != -1;
      }
    }

    public string ImageSrc
    {
      get
      {
        if (this._photo == null)
          return  null;
        if (!string.IsNullOrEmpty(this._photo.src_xbig))
          return this._photo.src_xbig;
        return this._photo.src_big;
      }
    }

    public string Text
    {
      get
      {
        Photo photo = this._photo;
        switch (photo != null ? photo.text :  null)
        {
          case null:
            return "";
          default:
            return Extensions.ForUI(this._photo.text);
        }
      }
    }

    public Visibility TextSeparatorVisibility
    {
      get
      {
        return (!string.IsNullOrWhiteSpace(this.Text)).ToVisiblity();
      }
    }

    public PhotoWithFullInfo PhotoWithInfo
    {
      get
      {
        return this._photoWithFullInfo;
      }
    }

    public List<PhotoVideoTag> PhotoTags
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<PhotoVideoTag>();
        return this._photoWithFullInfo.PhotoTags;
      }
    }

    public string OwnerImageUri
    {
      get
      {
        string str = "";
        if (this._photoWithFullInfo != null)
        {
          if (this.AuthorId < 0L)
          {
            Group group = (Group) Enumerable.FirstOrDefault<Group>(this._photoWithFullInfo.Groups, (Func<Group, bool>) (g => g.id == -this.AuthorId));
            if (group != null)
              str = group.photo_200;
          }
          else
          {
            User user = (User) Enumerable.FirstOrDefault<User>(this._photoWithFullInfo.Users, (Func<User, bool>) (u => u.id == this.AuthorId));
            if (user != null)
              str = user.photo_max;
          }
        }
        return str;
      }
    }

    public int TotalCommentsCount
    {
      get
      {
        return this.KnownCommentsCount;
      }
    }

    public long OwnerId
    {
      get
      {
        Photo photo = this._photo;
        if (photo == null)
          return 0;
        return photo.owner_id;
      }
    }

    public long UserOwnerId
    {
      get
      {
        Photo photo = this._photo;
        if (photo == null)
          return 0;
        return photo.user_id;
      }
    }

    public long AuthorId
    {
      get
      {
        if (this.UserOwnerId > 0L && this.UserOwnerId != User.ADMIN_ID)
          return this.UserOwnerId;
        return this.OwnerId;
      }
    }

    public string OwnerName
    {
      get
      {
        string str = "";
        if (this._photoWithFullInfo != null)
        {
          if (this.AuthorId < 0L)
          {
            Group group = (Group) Enumerable.FirstOrDefault<Group>(this._photoWithFullInfo.Groups, (Func<Group, bool>) (g => g.id == -this.AuthorId));
            if (group != null)
              str = group.name;
          }
          else
          {
            User user = (User) Enumerable.FirstOrDefault<User>(this._photoWithFullInfo.Users, (Func<User, bool>) (u => u.uid == this.AuthorId));
            if (user != null)
              str = user.Name;
          }
        }
        return str;
      }
    }

    public CommentType CommentType
    {
      get
      {
        return CommentType.Photo;
      }
    }

    public bool UserLiked
    {
      get
      {
        Photo photo = this._photo;
        if (photo == null)
          return false;
        Likes likes = photo.likes;
        int? nullable = likes != null ? new int?(likes.user_likes) : new int?();
        int num = 1;
        if (nullable.GetValueOrDefault() != num)
          return false;
        return nullable.HasValue;
      }
    }

    public bool CanRepost
    {
      get
      {
        return true;
      }
    }

    public string AccessKey
    {
      get
      {
        return this._accessKey;
      }
    }

    public long Pid
    {
      get
      {
        return this._pid;
      }
    }

    public bool IsGif { get;set; }

    public Visibility CanAddVisibility
    {
      get
      {
        return (!this._isGifAdded).ToVisiblity();
      }
    }

    public Visibility AddedVisibility
    {
      get
      {
        return this._isGifAdded.ToVisiblity();
      }
    }

    public SolidColorBrush OrientationLockFill
    {
      get
      {
        if (!AppGlobalStateManager.Current.GlobalState.IsPhotoViewerOrientationLocked)
          return new SolidColorBrush("#66ffffff".ToColor());
        return new SolidColorBrush("#ff73a8e6".ToColor());
      }
    }

    public Doc Document
    {
      get
      {
        return this._doc;
      }
    }

    public List<Comment> Comments
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<Comment>();
        return this._photoWithFullInfo.Comments;
      }
    }

    public List<User> Users
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<User>();
        return this._photoWithFullInfo.Users;
      }
    }

    public List<Group> Groups
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<Group>();
        return this._photoWithFullInfo.Groups;
      }
    }

    public List<User> Users2
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<User>();
        return this._photoWithFullInfo.Users2;
      }
    }

    public List<long> LikesAllIds
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<long>();
        return this._photoWithFullInfo.LikesAllIds;
      }
    }

    public int LikesCount
    {
      get
      {
        PhotoWithFullInfo photoWithFullInfo = this._photoWithFullInfo;
        if (photoWithFullInfo == null)
          return 0;
        return photoWithFullInfo.Photo.likes.count;
      }
    }

    public int RepostsCount
    {
      get
      {
        PhotoWithFullInfo photoWithFullInfo = this._photoWithFullInfo;
        if (photoWithFullInfo == null)
          return 0;
        return photoWithFullInfo.RepostsCount;
      }
    }

    public long ItemId
    {
      get
      {
        PhotoWithFullInfo photoWithFullInfo = this._photoWithFullInfo;
        if (photoWithFullInfo == null)
          return 0;
        return photoWithFullInfo.Photo.pid;
      }
    }

    public LikeObjectType LikeObjectType
    {
      get
      {
        return LikeObjectType.photo;
      }
    }

    public bool CanComment
    {
      get
      {
        if (this._photoWithFullInfo != null)
          return this._photoWithFullInfo.Photo.can_comment == 1;
        return false;
      }
    }

    public bool CanReport
    {
      get
      {
        return this._ownerId != AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public PhotoViewModel(Photo photo, PhotoWithFullInfo photoWithFullInfo = null)
      : this()
    {
      this._photo = photo;
      this._ownerId = photo.owner_id;
      this._pid = photo.pid;
      this._accessKey = photo.access_key;
      this._photoWithFullInfo = photoWithFullInfo;
      PhotoWithFullInfo photoWithFullInfo1 = this._photoWithFullInfo;
      VKClient.Common.Backend.DataObjects.Comments comments;
      if (photoWithFullInfo1 == null)
      {
        comments = (VKClient.Common.Backend.DataObjects.Comments) null;
      }
      else
      {
        Photo photo1 = photoWithFullInfo1.Photo;
        comments = photo1 != null ? photo1.comments : (VKClient.Common.Backend.DataObjects.Comments) null;
      }
      if (comments == null)
        return;
      this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
    }

    public PhotoViewModel(long ownerId, long pid, string accessKey)
      : this()
    {
      this._accessKey = accessKey;
      this._ownerId = ownerId;
      this._pid = pid;
    }

    public PhotoViewModel(Doc doc)
      : this()
    {
      this._doc = doc;
      this.IsGif = true;
      this.Photo = doc.ConvertToPhotoPreview();
    }

    private PhotoViewModel()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public void AddDocument()
    {
      if (this._doc == null || this._isGifAdded || this._isAddingDoc)
        return;
      this._isAddingDoc = true;
      this.SetInProgressMain(true, "");
      DocumentsService.Current.Add(this._doc.owner_id, this._doc.id, this._doc.access_key, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (res =>
      {
        this._isAddingDoc = false;
        this.SetInProgressMain(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._isGifAdded = true;
          // ISSUE: type reference
          // ISSUE: method reference
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.CanAddVisibility));
                    this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.AddedVisibility));
                    GenericInfoUC.ShowBasedOnResult(0, CommonResources.FileIsSavedInDocuments, (VKRequestsDispatcher.Error) null);
        }
        else if (res.ResultCode == ResultCode.WrongParameter && res.Error.error_msg.Contains("already added"))
          GenericInfoUC.ShowBasedOnResult(0, CommonResources.FileIsAlreadySavedInDocuments, (VKRequestsDispatcher.Error) null);
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", null);
      }));
    }

    public void LoadInfoWithComments(Action<bool, int> callback)
    {
      if (this.IsGif)
        callback.Invoke(true, 0);
      else if (this._photoWithFullInfo != null)
      {
        Group group = (Group) Enumerable.FirstOrDefault<Group>(this._photoWithFullInfo.Groups, (Func<Group, bool>) (g => g.id == -this._ownerId));
        callback.Invoke(true, group != null ? group.admin_level : 0);
      }
      else
      {
        if (this._isLoading)
          return;
        this._isLoading = true;
        this.SetInProgress(true, "");
        PhotosService.Current.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, -1, 0, this.CountToLoad, (Action<BackendResult<PhotoWithFullInfo, ResultCode>>) (res =>
        {
          int num1 = 0;
          if (res.ResultCode == ResultCode.Succeeded)
          {
            int num2 = string.IsNullOrEmpty(this.ImageSrc) ? 1 : 0;
            this._photoWithFullInfo = res.ResultData;
            this._photo = res.ResultData.Photo;
            if (this._photo != null && string.IsNullOrEmpty(this._photo.access_key) && !string.IsNullOrEmpty(this._accessKey))
              this._photo.access_key = this._accessKey;
            this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
            PhotoWithFullInfo resultData = res.ResultData;
            Group group = resultData != null ?  Enumerable.FirstOrDefault<Group>(resultData.Groups, (Func<Group, bool>) (g => g.id == -this._ownerId)) :  null;
            if (group != null)
              num1 = group.admin_level;
            if (num2 != 0)
                            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ImageSrc));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.CommentsCountStr));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Text));
                        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsLoadedFullInfo));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UserCountStr));
                        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UserVisibility));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.LikesCountStr));
                        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.UserLiked));
                        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.LikeOpacity));
                        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeBackgroundBrush));
                        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeTextForegroundBrush));
                        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsFullInfoLoadedVisibility));
                        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.IsFullInfoLoadedOpacity));
                    }
          this.SetInProgress(false, "");
          this._isLoading = false;
          callback.Invoke(res.ResultCode == ResultCode.Succeeded, num1);
        }));
      }
    }

    public void LikeUnlike()
    {
      Photo photo = this._photo;
      if ((photo != null ? photo.likes :  null) == null)
        return;
      if (this._photo.likes.user_likes == 0)
      {
        LikesService.Current.AddRemoveLike(true, this._photo.owner_id, this._photo.pid, LikeObjectType.photo, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}), this._accessKey);
        ++this._photo.likes.count;
        this._photo.likes.user_likes = 1;
      }
      else
      {
        LikesService.Current.AddRemoveLike(false, this._photo.owner_id, this._photo.pid, LikeObjectType.photo, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}), this._accessKey);
        --this._photo.likes.count;
        this._photo.likes.user_likes = 0;
      }
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.LikesCountStr));
            this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.LikeOpacity));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeBackgroundBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeTextForegroundBrush));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.UserLiked));
        }

    public void LoadMoreComments(int countToLoad, Action<bool> callback)
    {
      if (!this.IsLoadedFullInfo || this._isLoading)
        return;
      this._isLoading = true;
      PhotosService.Current.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, this.KnownCommentsCount, this._photoWithFullInfo.Comments.Count, countToLoad, (Action<BackendResult<PhotoWithFullInfo, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          List<Comment> comments = this._photoWithFullInfo.Comments;
          this._photoWithFullInfo.Comments = res.ResultData.Comments;
          this._photoWithFullInfo.Comments.AddRange((IEnumerable<Comment>) comments);
          this._photoWithFullInfo.Users.AddRange((IEnumerable<User>) res.ResultData.Users);
          this._photoWithFullInfo.Groups.AddRange((IEnumerable<Group>) res.ResultData.Groups);
          this._photoWithFullInfo.Users2.AddRange((IEnumerable<User>) res.ResultData.Users2);
        }
        this._isLoading = false;
        callback(res.ResultCode == ResultCode.Succeeded);
      }));
    }

    public void AddComment(Comment comment, List<string> attachmentIds, bool fromGroup, Action<bool, Comment> callback, string stickerReferrer = "")
    {
      if (this._adding)
      {
        callback.Invoke(false,  null);
      }
      else
      {
        this._adding = true;
                PhotosService.Current.CreateComment(this.OwnerId, this._photo.pid, comment.reply_to_cid, comment.text, fromGroup, attachmentIds, (Action<BackendResult<Comment, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        ++this.Photo.comments.count;
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            if (this.PhotoWithInfo == null)
                                return;
                            this.PhotoWithInfo.Comments.Add(res.ResultData);
                        }));
                        callback(true, res.ResultData);
                    }
                    else
                        callback(false, (Comment)null);
                    this._adding = false;
                }), this._accessKey, comment.sticker_id, stickerReferrer);
            }
        }

    public void DeleteComment(long cid)
    {
        --this.Photo.comments.count;
        PhotosService.Current.DeleteComment(this.OwnerId, this._photo.pid, cid, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if ((this.Comments) == null)
                return;
            Comment comment = (this.Comments).FirstOrDefault<Comment>((Func<Comment, bool>)(c => c.cid == cid));
            if (comment == null)
                return;
            (this.Comments).Remove(comment);
        }))));
    }

    public void Share(string text, long gid = 0, string groupName = "")
    {
        if (!this.IsGif)
        {
            WallService.Current.Repost(this._ownerId, this._pid, text, RepostObject.photo, gid, (Action<BackendResult<RepostResult, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                    GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Photo, gid, groupName);
                else
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
            }))));
        }
        else
        {
            WallService current = WallService.Current;
            WallPostRequestData postData = new WallPostRequestData();
            postData.owner_id = gid > 0L ? -gid : AppGlobalStateManager.Current.LoggedInUserId;
            postData.message = text;
            postData.AttachmentIds = new List<string>()
        {
          this._doc.UniqueIdForAttachment
        };
            Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                    GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Doc, gid, groupName);
                else
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
            })));
            current.Post(postData, callback);
        }
    }

    public void SaveInSavedPhotosAlbum()
    {
      if (this._isSavingInSavedPhotos)
        return;
      this._isSavingInSavedPhotos = true;
      PhotosService.Current.CopyPhotos(this._ownerId, this._pid, this._accessKey, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
      {
        this._isSavingInSavedPhotos = false;
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        // ISSUE: method pointer
        Execute.ExecuteOnUIThread(new Action(GenericInfoUC.ShowPhotoIsSavedInSavedPhotos));
      }));
    }

    public void LikeUnlike(bool like)
    {
      this.LikeUnlike();
    }

    public void ToggleOrientationLockMode()
    {
      AppGlobalStateData globalState = AppGlobalStateManager.Current.GlobalState;
      int num = !globalState.IsPhotoViewerOrientationLocked ? 1 : 0;
      globalState.IsPhotoViewerOrientationLocked = num != 0;
    }

    public void Handle(PhotoViewerOrientationLockedModeChanged message)
    {
        base.NotifyPropertyChanged<SolidColorBrush>(() => this.OrientationLockFill);
    }
  }
}
