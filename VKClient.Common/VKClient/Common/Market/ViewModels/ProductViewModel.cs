using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.ViewModels
{
    public class ProductViewModel : ViewModelStatefulBase, ISupportCommentsAndLikes, ILikeable
    {
        private Visibility _wikiPageVisibility = Visibility.Collapsed;
        private Visibility _contactSellerButtonVisibility = Visibility.Collapsed;
        private Visibility _productUnavailableVisibility = Visibility.Collapsed;
        private Visibility _expandDescriptionVisibility = Visibility.Collapsed;
        private const double PHOTO_VIEWER_WIDTH_HEIGHT = 480.0;
        private Product _product;
        private Group _group;
        private List<PhotoHeader> _photos;
        private bool _isSlideViewCycled;
        private Visibility _navDostVisibility;
        private string _productTitle;
        private string _price;
        private string _description;
        private bool _isDescriptionExpanded;
        private string _groupImage;
        private string _groupName;
        private string _category;
        private string _metaData;
        private string _wikiPageName;
        private const int MAX_PREVIEW_SYMBOLS_COUNT = 300;
        private ProductLikesCommentsData _likesCommentsData;
        private bool _isLoadingComments;
        private bool _isAdding;

        public int TotalCommentsCount { get; private set; }

        public List<PhotoHeader> Photos
        {
            get
            {
                return this._photos;
            }
            private set
            {
                this._photos = value;
                this.NotifyPropertyChanged("Photos");
            }
        }

        public bool IsSlideViewCycled
        {
            get
            {
                return this._isSlideViewCycled;
            }
            private set
            {
                this._isSlideViewCycled = value;
                this.NotifyPropertyChanged("IsSlideViewCycled");
            }
        }

        public Visibility NavDostVisibility
        {
            get
            {
                return this._navDostVisibility;
            }
            private set
            {
                this._navDostVisibility = value;
                this.NotifyPropertyChanged("NavDostVisibility");
            }
        }

        public string ProductTitle
        {
            get
            {
                return this._productTitle;
            }
            private set
            {
                this._productTitle = value;
                this.NotifyPropertyChanged("ProductTitle");
            }
        }

        public string Price
        {
            get
            {
                return this._price;
            }
            private set
            {
                this._price = value;
                this.NotifyPropertyChanged("Price");
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            private set
            {
                this._description = value;
                this.NotifyPropertyChanged("Description");
            }
        }

        public Visibility ExpandDescriptionVisibility
        {
            get
            {
                return this._expandDescriptionVisibility;
            }
            private set
            {
                this._expandDescriptionVisibility = value;
                this.NotifyPropertyChanged("ExpandDescriptionVisibility");
            }
        }

        public string GroupImage
        {
            get
            {
                return this._groupImage;
            }
            private set
            {
                this._groupImage = value;
                this.NotifyPropertyChanged("GroupImage");
            }
        }

        public string GroupName
        {
            get
            {
                return this._groupName;
            }
            private set
            {
                this._groupName = value;
                this.NotifyPropertyChanged("GroupName");
            }
        }

        public string Category
        {
            get
            {
                return this._category;
            }
            private set
            {
                this._category = value;
                this.NotifyPropertyChanged("Category");
            }
        }

        public string MetaData
        {
            get
            {
                return this._metaData;
            }
            private set
            {
                this._metaData = value;
                this.NotifyPropertyChanged("MetaData");
            }
        }

        public Visibility WikiPageVisibility
        {
            get
            {
                return this._wikiPageVisibility;
            }
            private set
            {
                this._wikiPageVisibility = value;
                this.NotifyPropertyChanged("WikiPageVisibility");
            }
        }

        public string WikiPageName
        {
            get
            {
                return this._wikiPageName;
            }
            private set
            {
                this._wikiPageName = value;
                this.NotifyPropertyChanged("WikiPageName");
            }
        }

        public Visibility ContactSellerButtonVisibility
        {
            get
            {
                return this._contactSellerButtonVisibility;
            }
            private set
            {
                this._contactSellerButtonVisibility = value;
                this.NotifyPropertyChanged("ContactSellerButtonVisibility");
            }
        }

        public Visibility ProductUnavailableVisibility
        {
            get
            {
                return this._productUnavailableVisibility;
            }
            private set
            {
                this._productUnavailableVisibility = value;
                this.NotifyPropertyChanged("ProductUnavailableVisibility");
            }
        }

        public bool IsLiked
        {
            get
            {
                Product product = this._product;
                if ((product != null ? product.likes : (Likes)null) != null)
                    return this._product.likes.user_likes == 1;
                return false;
            }
        }

        public Visibility IsLikedVisibility
        {
            get
            {
                return this.IsLiked.ToVisiblity();
            }
        }

        public Visibility IsNotLikedVisibility
        {
            get
            {
                return (!this.IsLiked).ToVisiblity();
            }
        }

        public long OwnerId { get; set; }//

        public long ItemId { get; set; }//

        public CommentType CommentType
        {
            get
            {
                return CommentType.Market;
            }
        }

        public LikeObjectType LikeObjectType
        {
            get
            {
                return LikeObjectType.market;
            }
        }

        public bool CanComment
        {
            get
            {
                if (this._product != null)
                    return this._product.CanComment;
                return false;
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
                return this._likesCommentsData.users;
            }
        }

        public List<Group> Groups
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<Group>();
                return this._likesCommentsData.groups;
            }
        }

        public List<User> Users2
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<User>();
                return this._likesCommentsData.users2;
            }
        }

        public List<long> LikesAllIds
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<long>();
                return this._likesCommentsData.likesAllIds;
            }
        }

        public int LikesCount
        {
            get
            {
                ProductLikesCommentsData likesCommentsData = this._likesCommentsData;
                if (likesCommentsData == null)
                    return 0;
                return likesCommentsData.likesAllCount;
            }
        }

        public int RepostsCount
        {
            get
            {
                ProductLikesCommentsData likesCommentsData = this._likesCommentsData;
                if (likesCommentsData == null)
                    return 0;
                return likesCommentsData.repostsCount;
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
                return this._product.CanRepost;
            }
        }

        public List<PhotoVideoTag> Tags
        {
            get
            {
                if (this._likesCommentsData == null)
                    return new List<PhotoVideoTag>();
                return this._likesCommentsData.tags;
            }
        }

        public ProductViewModel(long ownerId, long productId)
        {
            this.TotalCommentsCount = -1;

            this.OwnerId = ownerId;
            this.ItemId = productId;
            EventAggregator.Current.Publish((object)new MarketItemActionEvent()
            {
                itemId = string.Format("{0}_{1}", (object)ownerId, (object)productId),
                source = CurrentMarketItemSource.Source
            });
        }

        public ProductViewModel(Product product)
            : this(product.owner_id, product.id)
        {
            this._product = product;
        }

        public override void Load(Action<bool> callback)
        {
            MarketService.Instance.GetProductData(this.OwnerId, this.ItemId, (Action<BackendResult<ProductData, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    ProductData resultData = result.ResultData;
                    if (resultData != null)
                    {
                        this._product = resultData.product;
                        this._group = resultData.group;
                        this.UpdateUIProperties();
                    }
                    callback(true);
                }
                else
                    callback(false);
            }))));
        }

        private void UpdateUIProperties()
        {
            if (this._product == null || this._group == null)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.UpdatePhotos();
                this.ProductTitle = this._product.title;
                this.Price = this._product.price != null ? this._product.price.text : "";
                this.UpdateDescription();
                this.UpdateMetaData();
                this.UpdateWikiPage();
                this.UpdateContactSellerButtonVisibility();
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsLikedVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsNotLikedVisibility));
            }));
        }

        private void UpdatePhotos()
        {
            List<PhotoHeader> photoHeaderList = new List<PhotoHeader>();
            if (!this._product.photos.IsNullOrEmpty())
                photoHeaderList.AddRange(this._product.photos.Select<Photo, PhotoHeader>((Func<Photo, PhotoHeader>)(photo => new PhotoHeader(photo, 480.0))));
            this.Photos = photoHeaderList;
            this.IsSlideViewCycled = photoHeaderList.Count > 1;
            this.NavDostVisibility = photoHeaderList.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateDescription()
        {
            string str = this._product.description ?? "";
            if (str.Length <= 300)
            {
                this.Description = str;
                this._isDescriptionExpanded = true;
            }
            else
            {
                this.Description = str.Substring(0, 300) + "...";
                this._isDescriptionExpanded = false;
            }
            this.ExpandDescriptionVisibility = !this._isDescriptionExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateMetaData()
        {
            this.GroupImage = this._group.photo_200;
            this.GroupName = this._group.name;
            this.Category = this._product.category != null ? this._product.category.name : "";
            string str = UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double)this._product.date, true));
            int viewsCount = this._product.views_count;
            if (viewsCount > 0)
                str = str + " · " + UIStringFormatterHelper.FormatNumberOfSomething(viewsCount, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true, null, false);
            this.MetaData = str;
        }

        private void UpdateWikiPage()
        {
            VKClient.Common.Backend.DataObjects.Market market = this._group.market;
            string str1;
            if (market == null)
            {
                str1 = null;
            }
            else
            {
                Wiki wiki = market.wiki;
                str1 = wiki != null ? wiki.title : null;
            }
            string str2 = str1;
            if (string.IsNullOrEmpty(str2))
                return;
            this.WikiPageVisibility = Visibility.Visible;
            this.WikiPageName = str2;
        }

        private void UpdateContactSellerButtonVisibility()
        {
            if (this._product.IsAvailable)
            {
                this.ContactSellerButtonVisibility = Visibility.Visible;
                this.ProductUnavailableVisibility = Visibility.Collapsed;
            }
            else
            {
                this.ContactSellerButtonVisibility = Visibility.Collapsed;
                this.ProductUnavailableVisibility = Visibility.Visible;
            }
        }

        public void ExpandDescription()
        {
            if (this._isDescriptionExpanded)
                return;
            this._isDescriptionExpanded = true;
            this.Description = this._product.description;
            this.ExpandDescriptionVisibility = Visibility.Collapsed;
        }

        public void NavigateToGroup()
        {
            Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false);
        }

        public void NavigateToMarketWiki()
        {
            VKClient.Common.Backend.DataObjects.Market market = this._group.market;
            string str;
            if (market == null)
            {
                str = null;
            }
            else
            {
                Wiki wiki = market.wiki;
                str = wiki != null ? wiki.view_url : null;
            }
            if (string.IsNullOrEmpty(str))
                return;
            Navigator.Current.NavigateToWebUri(this._group.market.wiki.view_url, true, false);
        }

        public void OpenPhotoViewer(int index = 0)
        {
            List<Photo> list = this.Photos.Select<PhotoHeader, Photo>((Func<PhotoHeader, Photo>)(p => p.Photo)).ToList<Photo>();
            if (list.Count == 0)
                return;
            Navigator.Current.NavigateToImageViewer(list.Count, 0, index, list.Select<Photo, long>((Func<Photo, long>)(p => p.pid)).ToList<long>(), list.Select<Photo, long>((Func<Photo, long>)(p => p.owner_id)).ToList<long>(), list.Select<Photo, string>((Func<Photo, string>)(p => p.access_key)).ToList<string>(), list, "PhotosByIdsForProduct", false, false, (Func<int, Image>)(i => (Image)null), (PageBase)null, true);
        }

        public void CopyLink()
        {
            Clipboard.SetText(string.Format("http://vk.com/market{0}?w=product{1}_{2}", (object)this.OwnerId, (object)this.OwnerId, (object)this.ItemId));
        }

        public void ShareToGroup(string text, long groupId, string groupName)
        {
            WallPostRequestData postData = new WallPostRequestData()
            {
                message = text,
                AttachmentIds = new List<string>()
        {
          this._product.ToString()
        }
            };
            if (groupId != 0L)
            {
                postData.owner_id = -groupId;
                postData.OnBehalfOfGroup = true;
            }
            WallService.Current.Post(postData, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                    GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, groupId, groupName);
                else
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement)null);
            }))));
        }

        public void Share(string message)
        {
            this.PrepareProductForSharing(message);
            Navigator.Current.NavigateToPickConversation();
        }

        public void ContactSeller()
        {
            VKClient.Common.Backend.DataObjects.Market market = this._group.market;
            if (market == null || market.contact_id == 0L)
                return;
            EventAggregator.Current.Publish((object)new MarketContactEvent(string.Format("{0}_{1}", (object)this._product.owner_id, (object)this._product.id), MarketContactAction.start));
            this.PrepareProductForSharing(CommonResources.ContactSellerMessage);
            Navigator.Current.NavigateToConversation(market.contact_id, false, false, "", 0L, true);
        }

        private void PrepareProductForSharing(string message)
        {
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.Message = message;
            Product product = this._product;
            contentDataProvider.Product = product;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
        }

        public void LoadMoreComments(int countToLoad, Action<bool> callback)
        {
            if (this._isLoadingComments)
                return;
            this._isLoadingComments = true;
            MarketService instance = MarketService.Instance;
            long ownerId = this.OwnerId;
            long itemId = this.ItemId;
            int totalCommentsCount = this.TotalCommentsCount;
            ProductLikesCommentsData likesCommentsData = this._likesCommentsData;
            int? nullable;
            if (likesCommentsData == null)
            {
                nullable = new int?();
            }
            else
            {
                List<Comment> comments = likesCommentsData.Comments;
                nullable = comments != null ? new int?((comments.Count)) : new int?();
            }
            int offset = nullable ?? 0;
            int count = countToLoad;
            Action<BackendResult<ProductLikesCommentsData, ResultCode>> callback1 = (Action<BackendResult<ProductLikesCommentsData, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    if (this._likesCommentsData == null)
                    {
                        this._likesCommentsData = res.ResultData;
                        this.TotalCommentsCount = this._likesCommentsData.TotalCommentsCount;
                    }
                    else
                    {
                        List<Comment> comments = this._likesCommentsData.Comments;
                        this._likesCommentsData.Comments = res.ResultData.Comments;
                        this._likesCommentsData.Comments.AddRange((IEnumerable<Comment>)comments);
                        this._likesCommentsData.users.AddRange((IEnumerable<User>)res.ResultData.users);
                        this._likesCommentsData.groups.AddRange((IEnumerable<Group>)res.ResultData.groups);
                    }
                    callback(true);
                }
                else
                    callback(false);
                this._isLoadingComments = false;
            });
            instance.GetComments(ownerId, itemId, totalCommentsCount, offset, count, callback1);
        }

        public void AddComment(Comment comment, List<string> attachmentIds, bool fromGroup, Action<bool, Comment> callback, string stickerReferrer = "")
        {
            if (this._isAdding)
            {
                callback(false, null);
            }
            else
            {
                this._isAdding = true;
                MarketService.Instance.CreateComment(this.OwnerId, this.ItemId, comment.text, attachmentIds, fromGroup, comment.reply_to_cid, (Action<BackendResult<Comment, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            List<Comment> comments = this.Comments;
                            if (comments == null)
                                return;
                            Comment resultData = res.ResultData;
                            comments.Add(resultData);
                        }));
                        callback(true, res.ResultData);
                    }
                    else
                        callback(false, (Comment)null);
                    this._isAdding = false;
                }), comment.sticker_id, stickerReferrer);
            }
        }

        public void DeleteComment(long commentId)
        {
            MarketService.Instance.DeleteComment(this.OwnerId, commentId, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                List<Comment> comments = this.Comments;
                Comment comment = comments != null ? comments.FirstOrDefault<Comment>((Func<Comment, bool>)(c => c.cid == commentId)) : null;
                if (comment == null)
                    return;
                this.Comments.Remove(comment);
            }))));
        }

        public void LikeUnlike(bool like)
        {
            this.LikeUnlike();
        }

        private void LikeUnlike()
        {
            if (this._likesCommentsData == null)
                return;
            if (this._likesCommentsData.UserLiked == 0)
                this.Like();
            else
                this.Unlike();
            Product product = this._product;
            if ((product != null ? product.likes : (Likes)null) == null)
                return;
            this._product.likes.user_likes = this._likesCommentsData.UserLiked;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsLikedVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsNotLikedVisibility));
            }));
        }

        private void Like()
        {
            LikesService.Current.AddRemoveLike(true, this.OwnerId, this.ItemId, LikeObjectType.market, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
            {
                EventAggregator current = EventAggregator.Current;
                ProductFavedUnfavedEvent favedUnfavedEvent = new ProductFavedUnfavedEvent();
                favedUnfavedEvent.Product = this._product;
                int num = 1;
                favedUnfavedEvent.IsFaved = num != 0;
                current.Publish((object)favedUnfavedEvent);
            }), "");
            ++this._likesCommentsData.likesAllCount;
            this._likesCommentsData.UserLiked = 1;
        }

        private void Unlike()
        {
            LikesService.Current.AddRemoveLike(false, this.OwnerId, this.ItemId, LikeObjectType.market, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
            {
                EventAggregator current = EventAggregator.Current;
                ProductFavedUnfavedEvent favedUnfavedEvent = new ProductFavedUnfavedEvent();
                favedUnfavedEvent.Product = this._product;
                int num = 0;
                favedUnfavedEvent.IsFaved = num != 0;
                current.Publish((object)favedUnfavedEvent);
            }), "");
            --this._likesCommentsData.likesAllCount;
            this._likesCommentsData.UserLiked = 0;
        }
    }
}
