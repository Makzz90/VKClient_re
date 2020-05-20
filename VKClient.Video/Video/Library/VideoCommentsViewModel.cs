using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;
using VKClient.Video.Localization;

namespace VKClient.Video.Library
{
    public class VideoCommentsViewModel : ViewModelStatefulBase, ISupportCommentsAndLikes, ISupportOtherVideos, IHandle<VideoEdited>, IHandle, ILikeable
    {
        private OwnerFullHeaderWithSubscribeViewModel _ownerHeaderViewModel = new OwnerFullHeaderWithSubscribeViewModel();
        private int _knownCommentsCount = -1;
        private DateTime _lastTimeCompletedPlayRequest = DateTime.MinValue;
        private string _description;
        private VideoResolution _resolution;
        private readonly string _accessKey;
        private readonly StatisticsActionSource _actionSource;
        private readonly string _videoContext;
        private CatalogItemViewModel _catItemVM;
        private const int MAX_PREVIEW_SYMBOLS_COUNT = 300;
        private VideoLikesCommentsData _likesCommentsData;
        private bool _isLoadingComments;
        private bool _isAdding;
        private bool _inPlayRequest;
        private bool _isAddingRemoving;
        private bool _isDescriptionExpanded;

        public List<VideoResolution> Resolutions { get; private set; }

        public long OwnerId { get; private set; }

        public long VideoId { get; private set; }

        public VKClient.Common.Backend.DataObjects.Video Video { get; private set; }

        public Visibility ShowDurationVisibility
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(this.UIDuration)).ToVisiblity();
            }
        }

        public Visibility IsLiveVisibility
        {
            get
            {
                CatalogItemViewModel catItemVm = this._catItemVM;
                if (catItemVm == null)
                    return (Visibility)1;
                return catItemVm.IsLiveVisibility;
            }
        }

        public Visibility ShowPlaySmallIconVisibility
        {
            get
            {
                return (Visibility)1;
            }
        }

        public string UIDuration
        {
            get
            {
                if (this._catItemVM == null)
                    return "";
                return this._catItemVM.UIDuration;
            }
        }

        public string Title
        {
            get
            {
                return VideoResources.Video_Title;
            }
        }

        public OwnerFullHeaderWithSubscribeViewModel OwnerHeaderViewModel
        {
            get
            {
                return this._ownerHeaderViewModel;
            }
            private set
            {
                this._ownerHeaderViewModel = value;
                this.NotifyPropertyChanged("OwnerHeaderViewModel");
            }
        }

        public string VideoTitle
        {
            get
            {
                VKClient.Common.Backend.DataObjects.Video video = this.Video;
                return (video != null ? video.title : null) ?? "";
            }
        }

        public bool CanPlay
        {
            get
            {
                return VideoPlayerHelper.CanPlayVideo(this.Video);
            }
        }

        public Visibility CannotPlayVisibility
        {
            get
            {
                return (!this.CanPlay).ToVisiblity();
            }
        }

        public Visibility CanPlayVisibility
        {
            get
            {
                return this.CanPlay.ToVisiblity();
            }
        }

        public string ImageSrc
        {
            get
            {
                if (this.Video == null)
                    return "";
                return this.Video.image_big ?? this.Video.image_medium ?? this.Video.image;
            }
        }

        public string MediaDuration
        {
            get
            {
                if (this.Video == null)
                    return "";
                return UIStringFormatterHelper.FormatDuration(this.Video.duration);
            }
        }

        public string VideoDescription
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                this.NotifyPropertyChanged("VideoDescription");
            }
        }

        public Visibility ExpandDescriptionVisibility
        {
            get
            {
                return (!this._isDescriptionExpanded).ToVisiblity();
            }
        }

        public string MetaDataStr
        {
            get
            {
                string str = "";
                if (this.Video == null)
                    return str;
                if (this.Video.views > 0)
                    str += UIStringFormatterHelper.FormatNumberOfSomething(this.Video.views, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true, null, false);
                if (this.Video.date > 0)
                {
                    if (!string.IsNullOrEmpty(str))
                        str += " · ";
                    str += UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double)this.Video.date, true));
                }
                return str;
            }
        }

        public VideoResolution Resolution
        {
            get
            {
                return this._resolution;
            }
            set
            {
                this._resolution = value;
                this.NotifyPropertyChanged("Resolution");
                if (this._resolution == null)
                    return;
                AppGlobalStateManager.Current.GlobalState.DefaultVideoResolution = this._resolution.Resolution.ToString();
            }
        }

        public Visibility HaveManyResolutionsVisibility
        {
            get
            {
                return (this.Resolutions.Count > 0).ToVisiblity();
            }
        }

        public Visibility DescriptionVisibility
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(this.VideoDescription)).ToVisiblity();
            }
        }

        public int DesiredResolution
        {
            get
            {
                int result;
                int.TryParse(AppGlobalStateManager.Current.GlobalState.DefaultVideoResolution, out result);
                return result;
            }
        }

        public int TotalCommentsCount
        {
            get
            {
                return this._knownCommentsCount;
            }
        }

        public long ItemId
        {
            get
            {
                return this.VideoId;
            }
        }

        public CommentType CommentType
        {
            get
            {
                return CommentType.Video;
            }
        }

        public LikeObjectType LikeObjectType
        {
            get
            {
                return LikeObjectType.video;
            }
        }

        public List<Comment> Comments
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<Comment>();
                return this._likesCommentsData.Comments;
            }
        }

        public List<User> Users
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<User>();
                return this._likesCommentsData.Users;
            }
        }

        public List<Group> Groups
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<Group>();
                return this._likesCommentsData.Groups;
            }
        }

        public List<User> Users2
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<User>();
                return this._likesCommentsData.Users2;
            }
        }

        public List<long> LikesAllIds
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<long>();
                return this._likesCommentsData.LikesAllIds;
            }
        }

        public int LikesCount
        {
            get
            {
                if (this._likesCommentsData == null)
                    return 0;
                return this._likesCommentsData.LikesAllCount ?? 0;
            }
        }

        public int RepostsCount
        {
            get
            {
                if (this._likesCommentsData == null)
                    return 0;
                return this._likesCommentsData.RepostsCount ?? 0;
            }
        }

        public bool UserLiked
        {
            get
            {
                if (this._likesCommentsData != null)
                    return this._likesCommentsData.UserLiked == 1;
                return false;
            }
        }

        public bool CanRepost
        {
            get
            {
                return this.Video.can_repost == 1;
            }
        }

        public List<PhotoVideoTag> Tags
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<PhotoVideoTag>();
                return this._likesCommentsData.Tags;
            }
        }

        public VKList<VKClient.Common.Backend.DataObjects.Video> OtherVideos
        {
            get
            {
                if (this._likesCommentsData == null)
                    return (VKList<VKClient.Common.Backend.DataObjects.Video>)null;
                return this._likesCommentsData.VideoRecommendations;
            }
        }

        public bool CanComment
        {
            get
            {
                if (this.Video != null)
                    return this.Video.can_comment == 1;
                return false;
            }
        }

        public bool CanReport
        {
            get
            {
                return this.OwnerId != AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public bool CanEdit
        {
            get
            {
                if (this.Video != null)
                    return this.Video.can_edit == 1;
                return false;
            }
        }

        public bool CanDelete
        {
            get
            {
                if (this.Video != null)
                    return this.Video.can_edit == 1;
                return false;
            }
        }

        public bool CanAddToMyVideos
        {
            get
            {
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                if ((likesCommentsData != null ? likesCommentsData.Albums : null) != null && !this._likesCommentsData.Albums.Contains(VideoAlbum.ADDED_ALBUM_ID))
                    return this.Video.can_add == 1;
                return false;
            }
        }

        public bool CanRemoveFromMyVideos
        {
            get
            {
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                if ((likesCommentsData != null ? likesCommentsData.Albums : null) != null)
                    return this._likesCommentsData.Albums.Contains(VideoAlbum.ADDED_ALBUM_ID);
                return false;
            }
        }

        public string VideoUri
        {
            get
            {
                return string.Format("https://vk.com/video{0}_{1}", this.OwnerId, this.VideoId);
            }
        }

        public VideoCommentsViewModel(long ownerId, long videoId, string accessKey, VKClient.Common.Backend.DataObjects.Video video = null, StatisticsActionSource actionSource = StatisticsActionSource.news, string videoContext = "")
        {
            this.Resolutions = new List<VideoResolution>();
            this._knownCommentsCount = -1;
            this._lastTimeCompletedPlayRequest = DateTime.MinValue;

            this.OwnerId = ownerId;
            this.VideoId = videoId;
            this._accessKey = accessKey;
            this.Video = video;
            this._actionSource = actionSource;
            this._videoContext = videoContext;
            if (this.Video != null)
            {
                this._catItemVM = new CatalogItemViewModel(new VideoCatalogItem(this.Video), new List<User>(), new List<Group>(), false);
                this.PublishOpenVideoEvent();
            }
            this.UpdateOwnerInfo();
            this.UpdateDescription();
            this.InitResolutionsCollection();
            EventAggregator.Current.Subscribe(this);
        }

        private void UpdateOwnerInfo()
        {
            if (this._likesCommentsData != null)
            {
                if (this.OwnerId < 0L)
                {
                    Group group = Enumerable.FirstOrDefault<Group>(this._likesCommentsData.Groups, (Group g) => g.id == -this.OwnerId);
                    if (group != null)
                    {
                        this.OwnerHeaderViewModel = new OwnerFullHeaderWithSubscribeViewModel(group);
                        return;
                    }
                }
                else
                {
                    User user = Enumerable.FirstOrDefault<User>(this._likesCommentsData.Users, (User u) => u.uid == this.OwnerId);
                    if (user != null)
                    {
                        this.OwnerHeaderViewModel = new OwnerFullHeaderWithSubscribeViewModel(user);
                    }
                }
            }
        }

        public override void Load(Action<ResultCode> callback)
        {
            VKClient.Common.Backend.DataObjects.Video video = this.Video;
            if ((video != null ? video.files : null) != null && this.Video.files.Count > 0)
                callback(ResultCode.Succeeded);
            else
                VideoService.Instance.GetVideoById(this.OwnerId, this.VideoId, this._accessKey, (Action<BackendResult<List<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)(res =>
                {
                    ResultCode resultCode = res.ResultCode;
                    if (resultCode == ResultCode.Succeeded)
                    {
                        if (res.ResultData.Any<VKClient.Common.Backend.DataObjects.Video>())
                        {
                            int num = this.Video == null ? 1 : 0;
                            this.Video = res.ResultData[0];
                            this._catItemVM = new CatalogItemViewModel(new VideoCatalogItem(this.Video), new List<User>(), new List<Group>(), false);
                            this.NotifyVideoDurationPropertiesChanged();
                            if (num != 0)
                                this.PublishOpenVideoEvent();
                            this.UpdateOwnerInfo();
                            this.UpdateDescription();
                            this.InitResolutionsCollection();
                            this.NotifyPropertiesChanged();
                        }
                        else
                            resultCode = ResultCode.VideoNotFound;
                    }
                    Action<ResultCode> action = callback;
                    if (action == null)
                        return;
                    int num1 = (int)resultCode;
                    action((ResultCode)num1);
                }));
        }

        private void PublishOpenVideoEvent()
        {
            EventAggregator.Current.Publish(new OpenVideoEvent()
            {
                id = this.Video.GloballyUniqueId,
                Source = this._actionSource,
                context = this._videoContext
            });
        }

        private void NotifyVideoDurationPropertiesChanged()
        {
            this.NotifyPropertyChanged<Visibility>((() => this.IsLiveVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.ShowDurationVisibility));
            this.NotifyPropertyChanged<string>((() => this.UIDuration));
        }

        private void NotifyPropertiesChanged()
        {
            Execute.ExecuteOnUIThread(delegate
            {
                base.NotifyPropertyChanged<List<VideoResolution>>(() => this.Resolutions);
                base.NotifyPropertyChanged<VideoResolution>(() => this.Resolution);
                base.NotifyPropertyChanged<Visibility>(() => this.HaveManyResolutionsVisibility);
                base.NotifyPropertyChanged<string>(() => this.VideoTitle);
                base.NotifyPropertyChanged<string>(() => this.MediaDuration);
                base.NotifyPropertyChanged<string>(() => this.ImageSrc);
                base.NotifyPropertyChanged<string>(() => this.MetaDataStr);
                base.NotifyPropertyChanged<Visibility>(() => this.DescriptionVisibility);
                base.NotifyPropertyChanged<bool>(() => this.CanPlay);
                base.NotifyPropertyChanged<Visibility>(() => this.CanPlayVisibility);
                base.NotifyPropertyChanged<Visibility>(() => this.CannotPlayVisibility);
            });
        }

        private void UpdateDescription()
        {
            if (this.Video == null)
                return;
            string str = this.Video.description ?? "";
            if (str.Length <= 300)
            {
                this.VideoDescription = str;
                this._isDescriptionExpanded = true;
            }
            else
            {
                this.VideoDescription = str.Substring(0, 300) + "...";
                this._isDescriptionExpanded = false;
            }
            this.NotifyPropertyChanged<Visibility>((() => this.ExpandDescriptionVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.DescriptionVisibility));
        }

        private void InitResolutionsCollection()
        {
            Execute.ExecuteOnUIThread(delegate
            {
                VKClient.Common.Backend.DataObjects.Video expr_06 = this.Video;
                if (((expr_06 != null) ? expr_06.files : null) != null)
                {
                    using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = this.Video.files.Keys.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            this.AddIfNeeded(current, 240);
                            this.AddIfNeeded(current, 360);
                            this.AddIfNeeded(current, 480);
                            this.AddIfNeeded(current, 720);
                            this.AddIfNeeded(current, 1080);
                        }
                    }
                    if (this.Resolutions.Count > 0)
                    {
                        IEnumerable<VideoResolution> arg_C0_0 = this.Resolutions;
                        Func<VideoResolution, int> arg_C0_1 = new Func<VideoResolution, int>((r) => { return r.Resolution; });

                        this._resolution = (Enumerable.FirstOrDefault<VideoResolution>(Enumerable.OrderByDescending<VideoResolution, int>(arg_C0_0, arg_C0_1), (VideoResolution r) => r.Resolution <= this.DesiredResolution) ?? this.Resolutions[0]);
                    }
                }
            });
        }


        private void AddIfNeeded(string resolutionRaw, int resolution)
        {
            if (resolutionRaw.Contains(resolution.ToString()) && Enumerable.All<VideoResolution>(this.Resolutions, (VideoResolution r) => r.Resolution != resolution))
            {
                this.Resolutions.Add(new VideoResolution
                {
                    Resolution = resolution
                });
            }
        }


        public void LoadMoreComments(int countToLoad, Action<bool> callback)
        {
            if (this._isLoadingComments)
                return;
            this._isLoadingComments = true;
            VideoService.Instance.GetComments(this.OwnerId, this.VideoId, this._knownCommentsCount, this._likesCommentsData == null || this._likesCommentsData.Comments == null ? 0 : this._likesCommentsData.Comments.Count, countToLoad, this._actionSource, this._videoContext, (Action<BackendResult<VideoLikesCommentsData, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    if (this._likesCommentsData == null)
                    {
                        this._likesCommentsData = res.ResultData;
                        this._knownCommentsCount = this._likesCommentsData.TotalCommentsCount;
                        this.UpdateOwnerInfo();
                        this.NotifyPropertiesChanged();
                    }
                    else
                    {
                        List<Comment> comments = this._likesCommentsData.Comments;
                        this._likesCommentsData.Comments = res.ResultData.Comments;
                        this._likesCommentsData.Comments.AddRange((IEnumerable<Comment>)comments);
                        this._likesCommentsData.Users.AddRange((IEnumerable<User>)res.ResultData.Users);
                        this._likesCommentsData.Groups.AddRange((IEnumerable<Group>)res.ResultData.Groups);
                    }
                    if (this._likesCommentsData != null)
                        EventAggregator.Current.Publish(new VideoCommentsLikesUpdated()
                        {
                            CommentsCount = this._likesCommentsData.TotalCommentsCount,
                            LikesCount = (this._likesCommentsData.LikesAllCount ?? 0),
                            OwnerId = this.OwnerId,
                            VideoId = this.VideoId
                        });
                    callback(true);
                }
                else
                    callback(false);
                this._isLoadingComments = false;
            }));
        }

        public void AddComment(Comment comment, List<string> attachmentIds, bool fromGroup, Action<bool, Comment> callback, string stickerReferrer = "")
        {
            if (this._isAdding)
            {
                callback.Invoke(false, null);
                return;
            }
            this._isAdding = true;
            VideoService.Instance.CreateComment(this.OwnerId, this.VideoId, comment.text, comment.reply_to_cid, attachmentIds, delegate(BackendResult<Comment, ResultCode> res)
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    Execute.ExecuteOnUIThread(delegate
                    {
                        if (this.Comments != null)
                        {
                            this.Comments.Add(res.ResultData);
                        }
                    });
                    EventAggregator.Current.Publish(new VideoCommentIsAddedDeleted
                    {
                        IsAdded = true,
                        OwnerId = this.OwnerId,
                        VideoId = this.VideoId
                    });
                    callback.Invoke(true, res.ResultData);
                }
                else
                {
                    callback.Invoke(false, null);
                }
                this._isAdding = false;
            }, comment.sticker_id, stickerReferrer);
        }


        public void DeleteComment(long cid)
        {
            Func<Comment, bool> _9__2 = null;
            Action _9__1 = null;
            VideoService.Instance.DeleteComment(this.OwnerId, this.VideoId, cid, delegate(BackendResult<ResponseWithId, ResultCode> res)
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    EventAggregator.Current.Publish(new VideoCommentIsAddedDeleted
                    {
                        IsAdded = false,
                        OwnerId = this.OwnerId,
                        VideoId = this.VideoId
                    });
                    Action arg_5F_0;
                    if ((arg_5F_0 = _9__1) == null)
                    {
                        arg_5F_0 = (_9__1 = delegate
                        {
                            if (this.Comments != null)
                            {
                                IEnumerable<Comment> arg_37_0 = this.Comments;
                                Func<Comment, bool> arg_37_1;
                                if ((arg_37_1 = _9__2) == null)
                                {
                                    arg_37_1 = (_9__2 = ((Comment c) => c.cid == cid));
                                }
                                Comment comment = Enumerable.FirstOrDefault<Comment>(arg_37_0, arg_37_1);
                                if (comment != null)
                                {
                                    this.Comments.Remove(comment);
                                }
                            }
                        });
                    }
                    Execute.ExecuteOnUIThread(arg_5F_0);
                }
            });
        }


        internal void LikeUnlike()
        {
            if (this._likesCommentsData == null)
                return;
            if (this._likesCommentsData.UserLiked == 0)
            {
                LikesService.Current.AddRemoveLike(true, this.OwnerId, this.VideoId, LikeObjectType.video, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }), this._accessKey);
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                int? likesAllCount = likesCommentsData.LikesAllCount;
                int? nullable = likesAllCount.HasValue ? new int?(likesAllCount.GetValueOrDefault() + 1) : new int?();
                likesCommentsData.LikesAllCount = nullable;
                this._likesCommentsData.UserLiked = 1;
            }
            else
            {
                LikesService.Current.AddRemoveLike(false, this.OwnerId, this.VideoId, LikeObjectType.video, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }), this._accessKey);
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                int? likesAllCount = likesCommentsData.LikesAllCount;
                int? nullable = likesAllCount.HasValue ? new int?(likesAllCount.GetValueOrDefault() - 1) : new int?();
                likesCommentsData.LikesAllCount = nullable;
                this._likesCommentsData.UserLiked = 0;
            }
        }

        internal void PlayVideo()
        {
            if (this._inPlayRequest || (DateTime.Now - this._lastTimeCompletedPlayRequest).TotalSeconds < 3.5)
            {
                return;
            }
            this._inPlayRequest = true;
            int selectedResolution = this.GetSelectedResolution();
            base.SetInProgress(true, CommonResources.Loading);
            VideoPlayerHelper.PlayVideo(this.Video, delegate
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    base.SetInProgress(false, "");
                    this._inPlayRequest = false;
                    this._lastTimeCompletedPlayRequest = DateTime.Now;
                });
            }, selectedResolution, this._actionSource, this._videoContext);
        }


        private int GetSelectedResolution()
        {
            if (this._resolution != null)
                return this._resolution.Resolution;
            return this.DesiredResolution;
        }

        public void Delete(Action<ResultCode> resultCallback = null)
        {
            VideoService.Instance.Delete(this.OwnerId, this.VideoId, (Action<BackendResult<long, ResultCode>>)(res =>
            {
                Action<ResultCode> action = resultCallback;
                if (action == null)
                    return;
                int resultCode = (int)res.ResultCode;
                action((ResultCode)resultCode);
            }));
        }

        internal void Share(string text, long gid = 0, string groupName = "")
        {
            WallService.Current.Repost(this.Video.owner_id, this.Video.vid, text, RepostObject.video, gid, delegate(BackendResult<RepostResult, ResultCode> res)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Video, gid, groupName);
                        return;
                    }
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
                });
            });
        }


        public void Handle(VideoEdited message)
        {
            if (message.Video.owner_id != this.OwnerId || message.Video.id != this.VideoId || this.Video == null)
                return;
            this.Video.title = message.Video.title;
            this.Video.description = message.Video.description;
            this.Video.privacy_view = message.Video.privacy_view;
            this.Video.privacy_comment = message.Video.privacy_comment;
            this.NotifyPropertiesChanged();
        }

        public void LikeUnlike(bool like)
        {
            this.LikeUnlike();
        }

        internal void AddRemoveToMyVideos()
        {
            if (this._isAddingRemoving)
                return;
            this._isAddingRemoving = false;
            bool add = this.CanAddToMyVideos;
            VideoService.Instance.AddRemovedToFromAlbum(add, AppGlobalStateManager.Current.LoggedInUserId, VideoAlbum.ADDED_ALBUM_ID, this.OwnerId, this.VideoId, (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
            {
                this._isAddingRemoving = false;
                string successString = add ? CommonResources.VideoNew_VideoHasBeenAddedToMyVideos : CommonResources.VideoNew_VideoHasBeenRemovedFromMyVideos;
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, successString, null);
                if (res.ResultCode != ResultCode.Succeeded || this._likesCommentsData == null || this._likesCommentsData.Albums == null)
                    return;
                if (add)
                    this._likesCommentsData.Albums.Add(VideoAlbum.ADDED_ALBUM_ID);
                else
                    this._likesCommentsData.Albums.Remove(VideoAlbum.ADDED_ALBUM_ID);
            }));
        }

        public void ExpandDescription()
        {
            if (this._isDescriptionExpanded)
                return;
            this._isDescriptionExpanded = true;
            this.VideoDescription = this.Video.description;
            this.NotifyPropertyChanged<Visibility>((() => this.ExpandDescriptionVisibility));
        }
    }
}
