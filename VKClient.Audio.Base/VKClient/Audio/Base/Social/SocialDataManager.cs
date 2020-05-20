using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Localization;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using Windows.Phone.PersonalInformation;
using Windows.Phone.SocialInformation;

namespace VKClient.Audio.Base.Social
{
    public class SocialDataManager
    {
        private static SocialDataManager _instance;

        public static SocialDataManager Instance
        {
            get
            {
                if (SocialDataManager._instance == null)
                    SocialDataManager._instance = new SocialDataManager();
                return SocialDataManager._instance;
            }
        }

        public async Task UpdateCommentsCount(long toId, long postId, int currentCount)
        {
            string uniqueRemoteId = RemoteIdHelper.GenerateUniqueRemoteId(new WallPost()
            {
                to_id = toId,
                id = postId
            }.GloballyUniqueId, RemoteIdHelper.RemoteIdItemType.WallPost);
            try
            {
                await SocialManager.UpdateReactionDisplayCountAsync(uniqueRemoteId, currentCount);
            }
            catch
            {
            }
        }

        public async Task MarkFeedAsStale(long ownerId)
        {
            if (ownerId == 0L)
                ownerId = AppGlobalStateManager.Current.LoggedInUserId;
            string remoteId = RemoteIdHelper.GenerateUniqueRemoteId(ownerId.ToString(), RemoteIdHelper.RemoteIdItemType.UserOrGroup);
            try
            {
                await SocialManager.MarkFeedAsStaleAsync(remoteId, FeedType.Contact);
            }
            catch
            {
            }
            try
            {
                await SocialManager.MarkFeedAsStaleAsync(remoteId, FeedType.Dashboard);
            }
            catch
            {
            }
            try
            {
                await SocialManager.MarkFeedAsStaleAsync(remoteId, FeedType.Home);
            }
            catch
            {
            }
        }

        public async Task ProcessSocialOperationsQueue()
        {
            OperationQueue queue = await SocialManager.GetOperationQueueAsync();
            while (true)
            {
                ISocialOperation operation = await queue.GetNextOperationAsync();
                if (operation != null)
                {
                    try
                    {
                        switch (operation.Type)
                        {
                            case SocialOperationType.DownloadHomeFeed:
                                /*int num1 = */
                                await this.ProcessHomeFeed(operation as DownloadHomeFeedOperation);// ? 1 : 0;
                                break;
                            case SocialOperationType.DownloadContactFeed:
                                await this.ProcessContactFeed(operation as DownloadFeedOperation);
                                break;
                            case SocialOperationType.DownloadDashboard:
                                this.ProcessDashboard(operation as DownloadDashboardFeedOperation);
                                break;
                            case SocialOperationType.DownloadRichConnectData:
                                /*int num2 = */
                                await this.ProcessConnectData(operation as DownloadRichConnectDataOperation);// ? 1 : 0;
                                break;
                        }
                        operation.NotifyCompletion();
                    }
                    catch
                    {
                        operation.NotifyCompletion(false);
                    }
                    operation = null;
                }
                else
                    break;
            }
        }

        private async Task<bool> ProcessConnectData(DownloadRichConnectDataOperation downloadRichConnectDataOperation)
        {
            ContactBindingManager store = await ContactBindings.GetAppContactBindingManagerAsync();
            ContactStore contactStore = await ContactStore.CreateOrOpenAsync();
            foreach (string id in (IEnumerable<string>)downloadRichConnectDataOperation.Ids)
            {
                string remoteId = id;
                try
                {
                    string title = (await contactStore.FindContactByRemoteIdAsync(remoteId)).DisplayName;
                    ContactBinding contactBinding = await store.GetContactBindingByRemoteIdAsync(remoteId);
                    ConnectTileData connectTileData = new ConnectTileData();
                    connectTileData.Title = title;
                    BackendResult<PhotosListWithCount, ResultCode> profilePhotos = await PhotosService.Current.GetProfilePhotos(RemoteIdHelper.GetItemIdByRemoteId(remoteId), 0, 3);
                    if (profilePhotos.ResultCode == ResultCode.Succeeded)
                    {
                        for (int index = 0; index < Math.Min(3, profilePhotos.ResultData.response.Count); ++index)
                        {
                            Photo photo = profilePhotos.ResultData.response[index];
                            ConnectTileImage connectTileImage = new ConnectTileImage();
                            connectTileImage.ImageUrl = photo.src_big;
                            ((ICollection<ConnectTileImage>)connectTileData.Images).Add(connectTileImage);
                        }
                    }
                    contactBinding.TileData = connectTileData;
                    await store.SaveContactBindingAsync(contactBinding);
                    title = null;
                    contactBinding = null;
                    connectTileData = null;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("ProcessConnectData failed", ex);
                }
                remoteId = null;
            }
            return true;
        }

        private /*async Task*/void ProcessDashboard(DownloadDashboardFeedOperation downloadDashboardFeedOperation)
        {
            this.ProcessFeedOperationDetails(downloadDashboardFeedOperation.FeedOperationDetails, new Func<FeedOperationDetails, Task>(this.ProcessSingleDashboardRequest));
        }

        private async Task ProcessContactFeed(DownloadFeedOperation downloadFeedOperation)
        {
            int num = await this.ProcessContactFeeds(((IEnumerable<FeedOperationDetails>)downloadFeedOperation.FeedOperationDetails).ToList<FeedOperationDetails>()) ? 1 : 0;
        }

        private async Task<bool> ProcessSingleDashboardRequest(FeedOperationDetails fod)
        {
            BackendResult<WallData, ResultCode> wall = await WallService.Current.GetWall(RemoteIdHelper.GetItemIdByRemoteId(fod.OwnerRemoteId), 0, 1, "owner");
            if (wall.ResultCode == ResultCode.Succeeded && wall.ResultData.wall.Count > 0)
            {
                WallPost wallPost = wall.ResultData.wall[0];
                FeedItem feedItem = this.CreateFeedItem(wallPost, wall.ResultData.groups, wall.ResultData.profiles, false);
                if (feedItem != null)
                {
                    DashboardItem dashboardItem = await SocialManager.OpenContactDashboardAsync(fod);
                    dashboardItem.DefaultTarget = feedItem.DefaultTarget;
                    dashboardItem.Timestamp = new DateTimeOffset(ExtensionsBase.UnixTimeStampToDateTime((double)wallPost.date, true));
                    if (!string.IsNullOrEmpty(feedItem.PrimaryContent.Title) || !string.IsNullOrEmpty(feedItem.PrimaryContent.Message))
                    {
                        dashboardItem.Content.Title = feedItem.PrimaryContent.Title;
                        dashboardItem.Content.Message = feedItem.PrimaryContent.Message;
                    }
                    else
                    {
                        dashboardItem.Content.Title = feedItem.SecondaryContent.Title;
                        dashboardItem.Content.Message = feedItem.SecondaryContent.Message;
                    }
                    dashboardItem.Content.Target = feedItem.PrimaryContent.Target;
                    await dashboardItem.SaveAsync();
                    return true;
                }
                wallPost = null;
                feedItem = null;
            }
            return false;
        }

        private /*async Task*/void ProcessFeedOperationDetails(IList<FeedOperationDetails> feedOperationDetails, Func<FeedOperationDetails, Task> processAsyncFunc)//re
        {
            try
            {
                List<Task> taskList = new List<Task>();
                using (IEnumerator<FeedOperationDetails> enumerator = ((IEnumerable<FeedOperationDetails>)feedOperationDetails).GetEnumerator())
                {
                    while (((IEnumerator)enumerator).MoveNext())
                    {
                        Task task = processAsyncFunc(enumerator.Current);
                        taskList.Add(task);
                        if (taskList.Count > 6)
                        {
                            Task.WaitAll(taskList.ToArray());
                            taskList.Clear();
                        }
                    }
                }
                if (taskList.Count <= 0)
                    return;
                Task.WaitAll(taskList.ToArray());
                taskList.Clear();
            }
            catch
            {
            }
        }

        private async Task<bool> ProcessContactFeeds(List<FeedOperationDetails> fodList)
        {
            List<WallService.WallRequestData> requestData = new List<WallService.WallRequestData>();
            using (List<FeedOperationDetails>.Enumerator enumerator = fodList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FeedOperationDetails current = enumerator.Current;
                    requestData.Add(new WallService.WallRequestData()
                    {
                        UserId = RemoteIdHelper.GetItemIdByRemoteId(current.OwnerRemoteId),
                        Offset = this.GetRequiredOffset(current),
                        Count = current.ItemCount
                    });
                }
            }
            BackendResult<List<WallData>, ResultCode> res = await WallService.Current.GetWallForManyUsers(requestData);
            if (res.ResultCode != ResultCode.Succeeded)
                return false;
            for (int i = 0; i < fodList.Count && i < res.ResultData.Count; ++i)
            {
                FeedOperationDetails fod = fodList[i];
                WallData currentWallData = res.ResultData[i];
                int nextOffset = this.GetRequiredOffset(fod) + currentWallData.wall.Count;
                Feed feed = await SocialManager.OpenContactFeedAsync(fod);
                foreach (WallPost wallPost in currentWallData.wall)
                {
                    FeedItem feedItem = this.CreateFeedItem(wallPost, currentWallData.groups, currentWallData.profiles, true);
                    if (feedItem != null)
                    {
                        this.SetRequiredOffset(feedItem, nextOffset);
                        ((ICollection<FeedItem>)feed.Items).Add(feedItem);
                    }
                }
                await feed.SaveAsync();
                currentWallData = null;
            }
            return true;
        }

        private void SetRequiredOffset(FeedItem feedItem, int nextOffset)
        {
            feedItem.Timestamp = feedItem.Timestamp.AddTicks(-(feedItem.Timestamp.Ticks % 10000L) + (long)nextOffset);
        }

        private int GetRequiredOffset(FeedOperationDetails fod)
        {
            if (!string.IsNullOrEmpty(fod.FeedItemRemoteId))
                return (int)(fod.FeedItemTimestamp.Ticks % 10000L);
            return 0;
        }

        private async Task<bool> ProcessHomeFeed(DownloadHomeFeedOperation downloadHomeFeedOperation)
        {
            try
            {
                List<Task> taskList = new List<Task>();
                FeedOperationDetails op = downloadHomeFeedOperation.FeedOperationDetails;
                BackendResult<NewsFeedData, ResultCode> res = await NewsFeedService.Current.GetNewsFeed(this.CreateNewsFeedGetParams(op));
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    NewsFeedData newsFeedData = res.ResultData;
                    Feed feed = await SocialManager.OpenHomeFeedAsync(op);
                    foreach (NewsItem newsFeedItem in newsFeedData.items)
                    {
                        FeedItem feedItem = this.CreateFeedItem(newsFeedItem, newsFeedData.groups, newsFeedData.profiles);
                        if (feedItem != null)
                            ((ICollection<FeedItem>)feed.Items).Add(feedItem);
                    }
                    if (((IEnumerable<FeedItem>)feed.Items).Any<FeedItem>())
                        RemoteIdHelper.NewsFeedNewFromData.Instance.SetParams(((IEnumerable<FeedItem>)feed.Items).Last<FeedItem>().RemoteId, res.ResultData.new_offset, res.ResultData.next_from);
                    await feed.SaveAsync();
                    newsFeedData = (NewsFeedData)null;
                }
                return res.ResultCode == ResultCode.Succeeded;
            }
            catch
            {
                Debugger.Break();
                return false;
            }
        }

        private FeedItem CreateFeedItem(NewsItem newsFeedItem, List<Group> groups, List<User> profiles)
        {
            FeedItem feedItem = null;
            switch (newsFeedItem.NewsItemType)
            {
                case NewsItemType.post:
                    feedItem = this.CreateFeedItem(WallPost.CreateFromNewsItem(newsFeedItem), groups, profiles, true);
                    break;
                case NewsItemType.photo:
                case NewsItemType.photo_tag:
                    feedItem = this.CreatePhotoPhotoTagItem(newsFeedItem, groups, profiles);
                    break;
            }
            return feedItem;
        }

        private FeedItem CreatePhotoPhotoTagItem(NewsItem newsFeedItem, List<Group> groups, List<User> profiles)
        {
            FeedItem feedItem = new FeedItem();
            WallPost fromNewsItem = WallPost.CreateFromNewsItem(newsFeedItem);
            feedItem.DefaultTarget = "/default";
            feedItem.Author.DisplayName = fromNewsItem.GetAuthorDisplayName(groups, profiles);
            feedItem.Author.RemoteId = RemoteIdHelper.GenerateUniqueRemoteId(fromNewsItem.from_id.ToString(), RemoteIdHelper.RemoteIdItemType.UserOrGroup);
            feedItem.Author.Username = feedItem.Author.DisplayName;
            feedItem.ReactionDisplayKind = (ReactionDisplayKind)0;
            feedItem.Timestamp = new DateTimeOffset(ExtensionsBase.UnixTimeStampToDateTime((double)fromNewsItem.date, true));
            feedItem.Style = (FeedItemStyle)1;
            feedItem.RemoteId = RemoteIdHelper.GenerateUniqueRemoteId(fromNewsItem.GloballyUniqueId, RemoteIdHelper.RemoteIdItemType.WallPost);
            string photoTagFeedText = this.GetPhotoPhotoTagFeedText(newsFeedItem, groups, profiles);
            if (string.IsNullOrEmpty(photoTagFeedText))
                return null;
            feedItem.PrimaryContent.Title = photoTagFeedText;
            string photosMode = "Photos";
            if (newsFeedItem.Photo_tags != null && newsFeedItem.Photo_tags.Count > 0)
                photosMode = "PhotoTags";
            this.ProcessAttachmentsIntoFeedItem(fromNewsItem, feedItem, null, null, photosMode);
            if (((ICollection<FeedMediaThumbnail>)feedItem.Thumbnails).Count > 0)
                feedItem.DefaultTarget = feedItem.Thumbnails[0].Target;
            return feedItem;
        }

        private FeedItem CreateFeedItem(WallPost wallPost, List<Group> groups, List<User> profiles, bool allowFocusComments = true)
        {
            FeedItem feedItem = new FeedItem();
            string format = "/default?Action=WallPost&PostId={0}&OwnerId={1}&FocusComments={2}&PollId={3}&PollOwnerId={4}&HideLayout=True";
            long num1 = 0;
            long num2 = 0;
            if (wallPost.attachments != null)
            {
                Attachment attachment = wallPost.attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>)(a => a.poll != null));
                if (attachment != null)
                {
                    num1 = attachment.poll.poll_id;
                    num2 = wallPost.IsRepost() ? wallPost.copy_history[0].owner_id : wallPost.to_id;
                }
            }
            feedItem.DefaultTarget = string.Format(format, (object)wallPost.id, (object)wallPost.to_id, allowFocusComments, (object)num1, (object)num2);
            string str1 = string.Format(format, (object)wallPost.id, (object)wallPost.to_id, false, num1, (object)num2);
            feedItem.PrimaryContent.Target = str1;
            feedItem.SecondaryContent.Target = str1;
            feedItem.Author.DisplayName = wallPost.GetAuthorDisplayName(groups, profiles);
            Actor author1 = feedItem.Author;
            long num3 = wallPost.from_id;
            string uniqueRemoteId1 = RemoteIdHelper.GenerateUniqueRemoteId(num3.ToString(), RemoteIdHelper.RemoteIdItemType.UserOrGroup);
            author1.RemoteId = uniqueRemoteId1;
            feedItem.ReactionDisplayKind = (ReactionDisplayKind)2;
            feedItem.ReactionDisplayCount = wallPost.comments == null ? 0 : wallPost.comments.count;
            feedItem.Timestamp = new DateTimeOffset(ExtensionsBase.UnixTimeStampToDateTime((double)wallPost.date, true));
            feedItem.Style = FeedItemStyle.Photo;
            feedItem.RemoteId = RemoteIdHelper.GenerateUniqueRemoteId(wallPost.GloballyUniqueId, RemoteIdHelper.RemoteIdItemType.WallPost);
            string secContentTitle = SocialDataManager.FormatSecondaryContentTitle(wallPost.attachments, wallPost.geo);
            string secContentMessage = SocialDataManager.FormatSecondaryContentMessage(wallPost.attachments, wallPost.geo);
            if (wallPost.IsRepost())
            {
                string str2 = string.Format(format, (object)wallPost.copy_history[0].WallPostOrReplyPostId, (object)wallPost.copy_history[0].owner_id, (object)false, (object)num1, (object)num2);
                if (!string.IsNullOrEmpty(wallPost.text))
                    feedItem.PrimaryContent.Message = TextPreprocessor.PreprocessText(wallPost.text);
                else
                    feedItem.PrimaryContent.Title = wallPost.GetIsMale(profiles) ? BaseResources.SharedPostMale : BaseResources.SharedPostFemale;
                feedItem.ChildItem = new FeedChildItem();
                feedItem.ChildItem.PrimaryContent.Target = str2;
                feedItem.ChildItem.SecondaryContent.Target = str2;
                bool isMale = false;
                if (wallPost.IsProfilePhotoUpdatePost(profiles, out isMale))
                    feedItem.ChildItem.PrimaryContent.Title = isMale ? BaseResources.Photo_UpdatedProfileMale : BaseResources.Photo_UpdatedProfileFemale;
                feedItem.ChildItem.Timestamp = feedItem.Timestamp;
                feedItem.ChildItem.Author.DisplayName = wallPost.GetChildAuthorDisplayName(groups, profiles);
                Actor author2 = feedItem.ChildItem.Author;
                num3 = wallPost.GetChildItemFromId();
                string uniqueRemoteId2 = RemoteIdHelper.GenerateUniqueRemoteId(num3.ToString(), RemoteIdHelper.RemoteIdItemType.UserOrGroup);
                author2.RemoteId = uniqueRemoteId2;
                feedItem.ChildItem.DefaultTarget = str2;
                if (!string.IsNullOrWhiteSpace(wallPost.copy_history[0].text))
                    feedItem.ChildItem.PrimaryContent.Message = TextPreprocessor.PreprocessText(wallPost.copy_history[0].text);
                if (wallPost.copy_history[0].attachments != null)
                {
                    List<Attachment> attachments = wallPost.copy_history[0].attachments;
                    List<Photo> list = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.photo != null)).Select<Attachment, Photo>((Func<Attachment, Photo>)(a => a.photo)).ToList<Photo>();
                    string str3 = CacheManager.TrySerializeToString((IBinarySerializable)new PhotosList()
                    {
                        Photos = list
                    }).ForURL();
                    foreach (Attachment attachment in attachments)
                    {
                        if (attachment.photo != null)
                        {
                            FeedMediaThumbnail feedMediaThumbnail = new FeedMediaThumbnail();
                            feedMediaThumbnail.ImageUrl = attachment.photo.src_big;
                            string str4 = string.Format("/default?Action=ShowPhotos&ViewerMode={0}&PhotosCount={1}&SelectedPhotoIndex={2}&Photos={3}&HideLayout=True", "PhotosByIds", list.Count, ((ICollection<FeedMediaThumbnail>)feedItem.ChildItem.Thumbnails).Count, str3);
                            feedMediaThumbnail.Target = str4;
                            if (((ICollection<FeedMediaThumbnail>)feedItem.ChildItem.Thumbnails).Count < 3)
                                ((ICollection<FeedMediaThumbnail>)feedItem.ChildItem.Thumbnails).Add(feedMediaThumbnail);
                        }
                        if (attachment.link != null)
                        {
                            FeedItemSharedStory sharedStoryFromLink = SocialDataManager.TryCreateSharedStoryFromLink(feedItem.Timestamp, attachment.link);
                            if (sharedStoryFromLink != null)
                                feedItem.ChildItem.SharedStory = sharedStoryFromLink;
                        }
                        if (attachment.Page != null)
                        {
                            FeedItemSharedStory sharedStoryFromPage = this.TryCreateSharedStoryFromPage(feedItem.Timestamp, attachment.Page);
                            if (sharedStoryFromPage != null)
                                feedItem.ChildItem.SharedStory = sharedStoryFromPage;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(secContentTitle) && !string.IsNullOrEmpty(secContentMessage))
                {
                    feedItem.ChildItem.SecondaryContent.Title = secContentTitle;
                    feedItem.ChildItem.SecondaryContent.Message = secContentMessage;
                }
                if (string.IsNullOrEmpty(feedItem.ChildItem.PrimaryContent.Message) && string.IsNullOrEmpty(feedItem.ChildItem.SecondaryContent.Title) && string.IsNullOrEmpty(feedItem.ChildItem.SecondaryContent.Message))
                {
                    string titleIfNecessary = this.GenerateTitleIfNecessary(wallPost.copy_history[0]);
                    if (!string.IsNullOrEmpty(titleIfNecessary))
                    {
                        feedItem.ChildItem.PrimaryContent.Title = titleIfNecessary;
                    }
                    else
                    {
                        Debugger.Break();
                        feedItem = null;
                    }
                }
            }
            else
            {
                bool isMale = false;
                if (wallPost.text != string.Empty)
                    feedItem.PrimaryContent.Message = TextPreprocessor.PreprocessText(wallPost.text);
                this.ProcessAttachmentsIntoFeedItem(wallPost, feedItem, secContentTitle, secContentMessage, "PhotosByIds");
                if (wallPost.IsProfilePhotoUpdatePost(profiles, out isMale))
                    feedItem.PrimaryContent.Title = isMale ? BaseResources.Photo_UpdatedProfileMale : BaseResources.Photo_UpdatedProfileFemale;
                if (string.IsNullOrEmpty(feedItem.PrimaryContent.Title) && string.IsNullOrEmpty(feedItem.PrimaryContent.Message) && (string.IsNullOrEmpty(feedItem.SecondaryContent.Title) && string.IsNullOrEmpty(feedItem.SecondaryContent.Message)))
                {
                    string titleIfNecessary = this.GenerateTitleIfNecessary(wallPost);
                    if (!string.IsNullOrEmpty(titleIfNecessary))
                    {
                        feedItem.PrimaryContent.Title = titleIfNecessary;
                    }
                    else
                    {
                        Debugger.Break();
                        feedItem = null;
                    }
                }
                if (wallPost.from_id != wallPost.to_id && feedItem != null)
                    feedItem.PrimaryContent.Title = wallPost.GetFromToString(groups, profiles);
            }
            return feedItem;
        }

        public string GenerateTitleIfNecessary(WallPost wallPost)
        {
            string str = "";
            if (wallPost.attachments == null)
                return str;
            if ((wallPost.attachments == null ? 0 : wallPost.attachments.Count<Attachment>((Func<Attachment, bool>)(a => a.photo != null))) > 0)
                str = BaseFormatterHelper.FormatNumberOfSomething(wallPost.attachments.Count, BaseResources.OnePhotoFrm, BaseResources.TwoFourPhotosFrm, BaseResources.FivePhotosFrm, true, null, false);
            List<Attachment> attachments1 = wallPost.attachments;
            Func<Attachment, bool> predicate1 = (Func<Attachment, bool>)(a => a.link != null);
            if (attachments1.Any<Attachment>(predicate1))
                str = BaseResources.Link;
            List<Attachment> attachments2 = wallPost.attachments;
            Func<Attachment, bool> predicate2 = (Func<Attachment, bool>)(a => a.Page != null);
            if (attachments2.Any<Attachment>(predicate2))
                str = BaseResources.WikiPage;
            return str;
        }

        private void ProcessAttachmentsIntoFeedItem(WallPost wallPost, FeedItem feedItem, string secContentTitle = null, string secContentMessage = null, string photosMode = "PhotosByIds")
        {
            if (secContentTitle == null && secContentMessage == null)
            {
                secContentTitle = SocialDataManager.FormatSecondaryContentTitle(wallPost.attachments, wallPost.geo);
                secContentMessage = SocialDataManager.FormatSecondaryContentMessage(wallPost.attachments, wallPost.geo);
            }
            if (wallPost.attachments != null)
            {
                List<Photo> list = wallPost.attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.photo != null)).Select<Attachment, Photo>((Func<Attachment, Photo>)(a => a.photo)).ToList<Photo>();
                string str1 = CacheManager.TrySerializeToString((IBinarySerializable)new PhotosList()
                {
                    Photos = list
                }).ForURL();
                foreach (Attachment attachment in wallPost.attachments)
                {
                    if (attachment.photo != null)
                    {
                        FeedMediaThumbnail feedMediaThumbnail = new FeedMediaThumbnail();
                        feedMediaThumbnail.ImageUrl = attachment.photo.src_big;
                        string str2 = string.Format("/default?Action=ShowPhotos&ViewerMode={0}&PhotosCount={1}&SelectedPhotoIndex={2}&Photos={3}&HideLayout=True", (object)photosMode, (object)list.Count, (object)((ICollection<FeedMediaThumbnail>)feedItem.Thumbnails).Count, (object)str1);
                        feedMediaThumbnail.Target = str2;
                        if (((ICollection<FeedMediaThumbnail>)feedItem.Thumbnails).Count < 3)
                            ((ICollection<FeedMediaThumbnail>)feedItem.Thumbnails).Add(feedMediaThumbnail);
                    }
                    if (attachment.link != null)
                    {
                        FeedItemSharedStory sharedStoryFromLink = SocialDataManager.TryCreateSharedStoryFromLink(feedItem.Timestamp, attachment.link);
                        if (sharedStoryFromLink != null)
                            feedItem.SharedStory = sharedStoryFromLink;
                    }
                    if (attachment.Page != null)
                    {
                        FeedItemSharedStory sharedStoryFromPage = this.TryCreateSharedStoryFromPage(feedItem.Timestamp, attachment.Page);
                        if (sharedStoryFromPage != null)
                            feedItem.SharedStory = sharedStoryFromPage;
                    }
                }
            }
            if (string.IsNullOrEmpty(secContentTitle) || string.IsNullOrEmpty(secContentMessage))
                return;
            feedItem.SecondaryContent.Title = secContentTitle;
            feedItem.SecondaryContent.Message = secContentMessage;
        }

        private string GetPhotoPhotoTagFeedText(NewsItem newsFeedItem, List<Group> groups, List<User> users)
        {
            User user = users.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == newsFeedItem.source_id));
            if (user != null)
            {
                bool flag = user.sex == 2;
                if (newsFeedItem.Photo_tags != null && newsFeedItem.Photo_tags.Count > 0)
                {
                    string str1 = flag ? BaseResources.Photo_WasTaggedMale : BaseResources.Photo_WasTaggedFemale;
                    string str2 = BaseFormatterHelper.FormatNumberOfSomething(newsFeedItem.PhotoTagsCount, BaseResources.Photo_OnOnePhotoFrm, BaseResources.Photo_OnFivePhotosFrm, BaseResources.Photo_OnFivePhotosFrm, true, null, false);
                    string str3 = " ";
                    string str4 = str2;
                    return str1 + str3 + str4;
                }
                if (newsFeedItem.Photos != null && newsFeedItem.Photos.Count > 0)
                {
                    string str1 = flag ? BaseResources.Photo_AddedMale : BaseResources.Photo_AddedFemale;
                    string str2 = BaseFormatterHelper.FormatNumberOfSomething(newsFeedItem.PhotosCount, BaseResources.Photo_OnePhotoAddedFrm, BaseResources.Photo_TwoFourPhotosAddedFrm, BaseResources.Photo_FivePhotosAddedFrm, true, null, false);
                    string str3 = " ";
                    string str4 = str2;
                    return str1 + str3 + str4;
                }
            }
            return "";
        }

        private FeedItemSharedStory TryCreateSharedStoryFromPage(DateTimeOffset timestamp, Wiki wiki)
        {
            FeedItemSharedStory feedItemSharedStory = new FeedItemSharedStory();
            feedItemSharedStory.Content.Title = BaseResources.WikiPage;
            feedItemSharedStory.Content.Message = wiki.title;
            string str1 = string.Format("https://vk.com/club{0}?w=page-{0}_{1}", (object)wiki.gid, (object)wiki.pid);
            feedItemSharedStory.Content.Target = str1;
            string str2 = str1;
            feedItemSharedStory.DefaultTarget = str2;
            DateTimeOffset dateTimeOffset = timestamp;
            feedItemSharedStory.Timestamp = dateTimeOffset;
            return feedItemSharedStory;
        }

        public static string FormatSecondaryContentTitle(List<Attachment> attachments, Geo geo)
        {
            if (attachments == null)
                attachments = new List<Attachment>();
            attachments = attachments.Where<Attachment>((Func<Attachment, bool>)(a =>
            {
                if (a.photo == null && a.link == null)
                    return a.Page == null;
                return false;
            })).ToList<Attachment>();
            int count = attachments.Count;
            if (geo != null)
                ++count;
            if (count == 0)
                return "";
            if (count == 1)
            {
                if (geo != null)
                    return BaseResources.Location;
                Attachment attachment = attachments[0];
                if (attachment.video != null)
                    return BaseResources.Video;
                if (attachment.audio != null)
                    return BaseResources.Audio;
                if (attachment.doc != null)
                    return BaseResources.Document;
                if (attachment.note != null)
                    return BaseResources.Note;
                if (attachment.poll != null)
                    return BaseResources.Poll;
            }
            if (count > 1)
            {
                if (!SocialDataManager.AreAllMultiAttachmentsOfOneType(attachments, geo))
                    return BaseResources.Attachments;
                List<Attachment> source1 = attachments;
                Func<Attachment, bool> predicate1 = (Func<Attachment, bool>)(a => a.video != null);
                if (source1.Where<Attachment>(predicate1).Count<Attachment>() > 0)
                    return BaseResources.Videos;
                List<Attachment> source2 = attachments;
                Func<Attachment, bool> predicate2 = (Func<Attachment, bool>)(a => a.audio != null);
                if (source2.Where<Attachment>(predicate2).Count<Attachment>() > 0)
                    return BaseResources.Audios;
                List<Attachment> source3 = attachments;
                Func<Attachment, bool> predicate3 = (Func<Attachment, bool>)(a => a.doc != null);
                if (source3.Where<Attachment>(predicate3).Count<Attachment>() > 0)
                    return BaseResources.Documents;
                List<Attachment> source4 = attachments;
                Func<Attachment, bool> predicate4 = (Func<Attachment, bool>)(a => a.note != null);
                if (source4.Where<Attachment>(predicate4).Count<Attachment>() > 0)
                    return BaseResources.Notes;
                List<Attachment> source5 = attachments;
                Func<Attachment, bool> predicate5 = (Func<Attachment, bool>)(a => a.poll != null);
                if (source5.Where<Attachment>(predicate5).Count<Attachment>() > 0)
                    return BaseResources.Poll;
            }
            return "";
        }

        public static string FormatSecondaryContentMessage(List<Attachment> attachments, Geo geo)
        {
            if (attachments == null)
                attachments = new List<Attachment>();
            bool flag = SocialDataManager.AreAllMultiAttachmentsOfOneType(attachments, geo);
            string str1 = "";
            string str2 = "";
            List<Attachment> list1 = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.video != null)).ToList<Attachment>();
            if (list1.Count > 0)
            {
                if (!flag)
                    str2 = str2 + SocialDataManager.FormatSecondaryContentTitle(attachments, null) + Environment.NewLine;
                foreach (Attachment attachment in list1)
                    str2 = str2 + SocialDataManager.FormatVideoStr(attachment.video) + Environment.NewLine;
            }
            if (str2 != "")
                str1 = str1 + str2 + Environment.NewLine;
            List<Attachment> list2 = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.audio != null)).ToList<Attachment>();
            string str3 = "";
            if (list2.Count > 0)
            {
                if (!flag)
                    str3 = SocialDataManager.FormatSecondaryContentTitle(attachments, null) + Environment.NewLine;
                foreach (Attachment attachment in list2)
                    str3 = str3 + SocialDataManager.FormatAudioStr(attachment.audio) + Environment.NewLine;
            }
            if (str3 != "")
                str1 = str1 + str3 + Environment.NewLine;
            List<Attachment> list3 = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.doc != null)).ToList<Attachment>();
            string str4 = "";
            if (list3.Count > 0)
            {
                if (!flag)
                    str4 = SocialDataManager.FormatSecondaryContentTitle(attachments, null) + Environment.NewLine;
                foreach (Attachment attachment in list3)
                    str4 = str4 + SocialDataManager.FormatDocStr(attachment.doc) + Environment.NewLine;
            }
            if (str4 != "")
                str1 += str4;
            List<Attachment> list4 = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.poll != null)).ToList<Attachment>();
            string str5 = "";
            if (list4.Count > 0)
            {
                if (!flag)
                    str5 = BaseResources.Poll + Environment.NewLine;
                str5 = str5 + list4[0].poll.question + Environment.NewLine;
            }
            if (str5 != "")
                str1 = str1 + str5 + Environment.NewLine;
            List<Attachment> list5 = attachments.Where<Attachment>((Func<Attachment, bool>)(a => a.note != null)).ToList<Attachment>();
            string str6 = "";
            if (list5.Count > 0)
            {
                if (!flag)
                    str6 = SocialDataManager.FormatSecondaryContentTitle(attachments, null) + Environment.NewLine;
                foreach (Attachment attachment in list5)
                    str6 = str6 + SocialDataManager.FormatNoteStr(attachment.note) + Environment.NewLine;
            }
            if (str6 != "")
                str1 = str1 + str6 + Environment.NewLine;
            return str1;
        }

        private static string FormatNoteStr(Note note)
        {
            return note.title;
        }

        private static string FormatDocStr(Doc doc)
        {
            return doc.title + ", " + SocialDataManager.FormatSize((double)doc.size);
        }

        private static string FormatAudioStr(AudioObj audioObj)
        {
            return audioObj.artist + " — " + audioObj.title;
        }

        private static string FormatVideoStr(VKClient.Common.Backend.DataObjects.Video video)
        {
            return video.title + ", " + SocialDataManager.FormatDuration(video.duration);
        }

        private static string FormatSize(double size)
        {
            string format = "#.#";
            string str = size.ToString(format) + " B";
            if (size >= 1000.0 && size < 1000000.0)
                str = (size / 1000.0).ToString(format) + " KB";
            else if (size >= 1000000.0)
                str = (size / 1000000.0).ToString(format) + " MB";
            return str;
        }

        public static string FormatDuration(int durationSeconds)
        {
            if (durationSeconds < 3600)
                return TimeSpan.FromSeconds((double)durationSeconds).ToString("m\\:ss");
            return TimeSpan.FromSeconds((double)durationSeconds).ToString("h\\:mm\\:ss");
        }

        public static bool AreAllMultiAttachmentsOfOneType(List<Attachment> attachments, Geo geo)
        {
            if (attachments == null || attachments.Count == 0)
                return true;
            List<Attachment> list = attachments.Where<Attachment>((Func<Attachment, bool>)(a =>
            {
                if (a.photo == null && a.poll == null && a.link == null)
                    return a.Page == null;
                return false;
            })).ToList<Attachment>();
            if (list.Count == 0)
                return true;
            if (geo != null)
                return false;
            string type = list[0].type;
            foreach (Attachment attachment in list)
            {
                if (type != attachment.type)
                    return false;
            }
            return true;
        }

        private static FeedItemSharedStory TryCreateSharedStoryFromLink(DateTimeOffset timestamp, Link link)
        {
            if (string.IsNullOrEmpty(link.url) || string.IsNullOrEmpty(link.title) && string.IsNullOrEmpty(link.description))
                return null;
            FeedItemSharedStory feedItemSharedStory1 = new FeedItemSharedStory();
            string url1 = link.url;
            feedItemSharedStory1.DefaultTarget = url1;
            FeedItemSharedStory feedItemSharedStory2 = feedItemSharedStory1;
            feedItemSharedStory2.Content.Title = link.title;
            feedItemSharedStory2.Content.Message = link.description;
            feedItemSharedStory2.Content.Target = link.url;
            Photo photo = link.photo;
            if (photo != null && !string.IsNullOrEmpty(photo.src))
            {
                FeedMediaThumbnail feedMediaThumbnail1 = new FeedMediaThumbnail();
                string src = photo.src;
                feedMediaThumbnail1.ImageUrl = src;
                string url2 = link.url;
                feedMediaThumbnail1.Target = url2;
                FeedMediaThumbnail feedMediaThumbnail2 = feedMediaThumbnail1;
                feedItemSharedStory2.Thumbnail = feedMediaThumbnail2;
            }
            feedItemSharedStory2.Timestamp = timestamp;
            return feedItemSharedStory2;
        }

        private NewsFeedGetParams CreateNewsFeedGetParams(FeedOperationDetails op)
        {
            NewsFeedGetParams newsFeedGetParams = new NewsFeedGetParams();
            //if (op.Direction == null)
            //    Debugger.Break();
            if (!string.IsNullOrEmpty(op.FeedItemRemoteId))
                newsFeedGetParams = RemoteIdHelper.GetNewsFeedGetParamsBy(op.FeedItemRemoteId, op.ItemCount);
            newsFeedGetParams.NewsListId = -10;
            return newsFeedGetParams;
        }
    }
}
