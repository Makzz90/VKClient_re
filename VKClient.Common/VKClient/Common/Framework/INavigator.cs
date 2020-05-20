using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Photos.Library;

namespace VKClient.Common.Framework
{
    public interface INavigator
    {
        List<string> History { get; }

        void NavigateToMainPage();

        void NavigateToPhotoAlbum(long userOrGroupId, bool isGroup, string type, string aid, string albumName = "", int photosCount = 0, string title = "", string description = "", bool pickMode = false, int adminLevel = 0);

        bool NavigateToUserProfile(long uid, string userName = "", string source = "", bool needClearStack = false);

        bool NavigateToGroup(long groupId, string name = "", bool needClearStack = false);

        void NavigateToPostsSearch(long ownerId, string nameGen = "");

        void NavigateToWebUri(string uri, bool forceWebNavigation = false, bool fromPush = false);

        void NavigateToPhotoAlbums(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0);

        void NavigateToVideo(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, bool forceAllowVideoUpload = false);

        void NavigateToVideoAlbum(long albumId, string albumName, bool pickMode = false, long userOrGroupId = 0, bool isGroup = false);

        void NavigateToAudio(int pickMode = 0, long userOrGroupId = 0, bool isGroup = false, long albumId = 0, long excludeAlbumId = 0, string albumName = "");

        void NavigateToFriends(long userId, string name, bool mutual, FriendsPageMode mode = FriendsPageMode.Default);

        void NavigateToGroups(long userId, string name = "", bool pickManaged = false, long owner_id = 0, long pic_id = 0, string text = "", bool isGif = false, string accessKey = "");

        void NavigateToNewWallPost(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false);

        void GoBack();

        void NavigateToWallPostComments(long postId, long ownerId, bool focusCommentsField, long pollId = 0, long pollOwnerId = 0, string adData = "");

        void NavigateToPostponedPosts(long groupId = 0);

        void NavigateToFriendsList(long lid, string listName);

        void NavigateToFollowers(long userOrGroupId, bool isGroup, string name);

        void NavigateToFriendRequests(bool areSuggestedFriends);

        void NavigateToSettings();

        void NavigateToImageViewer(string aid, int albumType, long userOrGroupId, bool isGroup, int photosCount, int selectedPhotoIndex, List<Photo> photos, Func<int, Image> getImageByIdFunc);

        void NavigateToImageViewerPhotosOrGifs(int selectedIndex, List<PhotoOrDocument> photosOrDocuments, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false, FrameworkElement currentViewControl = null, Action<int> setContextOnCurrentViewControl = null, Action<int, bool> showHideOverlay = null, bool shareButtonOnly = false);

        void NavigateToImageViewer(int photosCount, int initialOffset, int selectedPhotoIndex, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, string viewerMode, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false);

        void NaviateToImageViewerPhotoFeed(long userOrGroupId, bool isGroup, string aid, int photosCount, int selectedPhotoIndex, int date, List<Photo> photos, string mode, Func<int, Image> getImageByIdFunc);

        void NavigateToPhotoWithComments(Photo photo, PhotoWithFullInfo photoWithFullInfo, long ownerId, long pid, string accessKey, bool fromDialog = false, bool friendsOnly = false);

        void NavigateToLikesPage(long ownerId, long itemId, int type, int likesCount = 0);

        void PickAlbumToMovePhotos(long userOrGroupId, bool isGroup, string excludeAid, List<long> list, int adminLevel = 0);

        void NavigateToMap(bool pick, double latitude, double longitude);

        void NavigateToPostSchedule(DateTime? dateTime = null);

        void NavigateToGroupDiscussions(long gid, string name, int adminLevel, bool isPublicPage, bool canCreateTopic);

        void NavigateToGroupDiscussion(long gid, long tid, string topicName, int knownCommentsCount, bool loadFromEnd, bool canComment);

        void NavigateToFeedback();

        void NavigateToVideoWithComments(VKClient.Common.Backend.DataObjects.Video video, long ownerId, long videoId, string accessKey = "");

        void NavigateToConversationsSearch();

        void NavigateToConversation(long userOrChatId, bool isChat, bool fromLookup = false, string newMessage = "", long message_id = 0, bool isContactSellerMode = false);

        void NavigateToWelcomePage();

        void NavigateToPickUser(bool createChat, long initialUserId, bool goBackOnResult, int currentCountInChat = 0, PickUserMode mode = PickUserMode.PickForMessage, string customTitle = "", int sexFilter = 0);

        void NavigateToPickConversation();

        void NavigateToAudioPlayer(bool startPlaying = false);

        void NavigateToGroupInvitations();

        void NavigateToDocuments(long ownerId = 0, bool isOwnerCommunityAdmined = false);

        void NavigateToSubscriptions(long userId);

        void NavigateToFavorites();

        void NavigateToGames(long gameId = 0, bool fromPush = false);

        void NavigateToMyGames();

        void NavigateToGamesFriendsActivity(long gameId = 0, string gameName = "");

        void NavigateToGamesInvites();

        void OpenGame(Game game);

        void OpenGame(long gameId);

        void NavigateToGameSettings(long gameId);

        void NavigateToManageSources(ManageSourcesMode mode = ManageSourcesMode.ManageHiddenNewsSources);

        void NavigateToPhotoPickerPhotos(int maxAllowedToSelect, bool ownPhooPick = false, bool pickToStorageFile = false);

        void NavigationToValidationPage(string validationUri);

        void NavigateTo2FASecurityCheck(string username, string password, string phoneMask, string validationType, string validationSid);

        void NavigateToAddNewVideo(string filePath, long ownerId);

        void NavigateToEditVideo(long ownerId, long videoId, VKClient.Common.Backend.DataObjects.Video video);

        void NavigateToNewsFeed(int newsSourceId = 0, bool photoFeedMoveTutorial = false);

        void NavigateToNewsSearch(string query = "");

        void NavigateToConversations();

        IPhotoPickerPhotosViewModel GetPhotoPickerPhotosViewModelInstance(int maxAllowedToSelect);

        void NavigateToBlackList();

        void NavigateToBirthdaysPage();

        void NavigateToSuggestedPostponedPostsPage(long userOrGroupId, bool isGroup, int mode);

        void NavigateToHelpPage();

        void NavigateToAboutPage();

        void NavigateFromSDKAuthPage(string callbackUriToLaunch);

        void NavigateToEditPrivacy(EditPrivacyPageInputData inputData);

        void NavigateToSettingsPrivacy();

        void NavigateToSettingsGeneral();

        void NavigateToSettingsAccount();

        void NavigateToChangePassword();

        void NavigateToSettingsNotifications();

        void NavigateToEditProfile();

        void NavigateToChangeShortName(string currentShortName);

        void NavigateToCreateEditPoll(long ownerId, long pollId = 0, Poll poll = null);

        void NavigateToPollVoters(long ownerId, long pollId, long answerId, string answerText);

        void NavigateToUsersSearch(string query = "");

        void NavigateToUsersSearchNearby();

        void NavigateToFriendsSuggestions();

        void NavigateToUsersSearchParams();

        void NavigateToRegistrationPage();

        void NavigateToFriendsImportFacebook();

        void NavigateToFriendsImportGmail();

        void NavigateToFriendsImportTwitter(string oauthToken, string oauthVerifier);

        void NavigateToFriendsImportContacts();

        void NavigateToGroupRecommendations(int categoryId = 0, string categoryName = "");

        void NavigateToVideoCatalog();

        void NavigateToWebViewPage(string uri);

        void NavigateToMarket(long ownerId);

        void NavigateToProduct(Product product);

        void NavigateToProduct(long ownerId, long productId);

        void NavigateToProductsSearchParams(long priceFrom, long priceTo, int currencyId, string currencyName);

        void NavigateToMarketAlbums(long ownerId);

        void NavigateToMarketAlbumProducts(long ownerId, long albumId, string albumName);

        void NavigateToVideoAlbumsList(long ownerId, bool forceAllowCreateAlbum);

        void NavigateToVideoList(VKList<VideoCatalogItem> catalogItems, int source, string context, string categoryId = "", string next = "", string name = "");

        void NavigateToCreateEditVideoAlbum(long albumId = 0, long groupId = 0, string name = "", PrivacyInfo pi = null);

        void NavigateToAddVideoToAlbum(long ownerId, long videoId);

        void NavigateToConversationMaterials(long peerId);

        void NavigateToDocumentsPicker();

        void NavigateToCommunityCreation();

        void NavigateToCommunitySubscribers(long communityId, GroupType communityType, bool isManagement = false, bool isPicker = false, bool isBlockingPicker = false);

        void NavigateToStickersStore();

        void NavigateToStickersManage();

        void NavigateToBalance();

        void NavigateToCustomListPickerSelection(CustomListPicker parentPicker);

        void NavigateToGraffitiDrawPage(long userIdChatId, bool isChat, string title);// NEW: 4.8.0

        void NavigateToCommunityManagement(long communityId, GroupType communityType, bool isAdministrator);

        void NavigateToCommunityManagementInformation(long communityId);

        void NavigateToCommunityManagementPlacementSelection(long communityId, Place place);

        void NavigateToCommunityManagementServices(long communityId);

        void NavigateToCommunityManagementServiceSwitch(CommunityService service, CommunityServiceState currentState);

        void NavigateToCommunityManagementManagers(long communityId, GroupType communityType);

        void NavigateToCommunityManagementManagerAdding(long communityId, User user, bool fromPicker);

        void NavigateToCommunityManagementManagerEditing(long communityId, User manager, bool isContact, string position, string email, string phone);

        void NavigateToCommunityManagementRequests(long communityId);

        void NavigateToCommunityManagementInvitations(long communityId);

        void NavigateToCommunityManagementBlacklist(long communityId, GroupType communityType);

        void NavigateToCommunityManagementBlockAdding(long communityId, User user, bool isOpenedWithoutPicker = false);

        void NavigateToCommunityManagementBlockEditing(long communityId, User user, User manager);

        void NavigateToCommunityManagementBlockDurationPicker(int durationUnixTime);

        void NavigateToCommunityManagementLinks(long communityId);

        void NavigateToCommunityManagementLinkCreation(long communityId, GroupLink link);
    }
}
