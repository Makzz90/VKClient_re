using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Utils;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Library.Posts;
using VKClient.Common.Profiles.Shared.Views;
using VKClient.Common.Stickers.Views;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.Storage;
using Windows.System;

namespace VKClient.Library
{
    public class NavigatorImpl : INavigator
    {
        private static readonly Regex _friendsReg = new Regex("/friends(\\?id=[0-9])?");
        private static readonly Regex _communitiesReg = new Regex("/groups(\\s|$)");
        private static readonly Regex _dialogsReg = new Regex("/(im|mail)(\\s|$)");
        private static readonly Regex _dialogReg = new Regex("/write[-0-9]+");
        private static readonly Regex _wallReg = new Regex("/wall[-0-9]+_[0-9]+");
        private static readonly Regex _feedWallReg = new Regex("/feed?w=wall[-0-9]+_[0-9]+");
        private static readonly Regex _audiosReg = new Regex("/audios[-0-9]+");
        private static readonly Regex _newsReg = new Regex("/feed(\\s|$)");
        private static readonly Regex _recommendedNewsReg = new Regex("/feed\\?section=recommended(\\s|$)");
        private static readonly Regex _feedbackReg = new Regex("/feed\\?section=notifications(\\s|$)");
        private static readonly Regex _profileReg = new Regex("/(id|wall)[0-9]+");
        private static readonly Regex _communityReg = new Regex("/(club|event|public|wall)[-0-9]+");
        private static readonly Regex _photosReg = new Regex("/(photos|albums)[-0-9]+");
        private static readonly Regex _photoReg = new Regex("/photo[-0-9]+_[0-9]+");
        private static readonly Regex _albumReg = new Regex("/album[-0-9]+_[0-9]+");
        private static readonly Regex _tagReg = new Regex("/tag[0-9]+");
        private static readonly Regex _videosReg = new Regex("/videos[-0-9]+");
        private static readonly Regex _videoReg = new Regex("/video[-0-9]+_[0-9]+");
        private static readonly Regex _boardReg = new Regex("/board[0-9]+");
        private static readonly Regex _topicReg = new Regex("/topic[-0-9]+_[0-9]+");
        private static readonly Regex _stickersSettingsReg = new Regex("/stickers/settings(\\s|$)");
        private static readonly Regex _settingsReg = new Regex("/settings(\\s|$)");
        private static readonly Regex _stickersReg = new Regex("/stickers(\\s|\\?|$)");
        private static readonly Regex _stickersPackReg = new Regex("/stickers([\\/A-Za-z0-9]+)");
        private static readonly Regex _faveReg = new Regex("/fave(\\s|$)");
        private static readonly Regex _appsReg = new Regex("/apps(\\s|$)");
        private static readonly Regex _appReg = new Regex("/app[-0-9]+_[-0-9]+");
        private static readonly Regex _marketAlbumReg = new Regex("/market[-0-9]+\\?section=album_[-0-9]+");
        private static readonly Regex _marketReg = new Regex("/market[-0-9]+");
        private static readonly Regex _productReg = new Regex("/product[-0-9]+_[0-9]+");
        private static readonly Regex _giftsReg = new Regex("/gifts[0-9]+");
        private static readonly Regex _giftsCatalog = new Regex("/gifts(\\s|$)");
        private static readonly Regex _namedObjReg = new Regex("/[A-Za-z0-9\\\\._-]+");
        private readonly List<NavigatorImpl.NavigationTypeMatch> _navTypesList = new List<NavigatorImpl.NavigationTypeMatch>()
    {
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._friendsReg, NavigatorImpl.NavType.friends),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communitiesReg, NavigatorImpl.NavType.communities),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogsReg, NavigatorImpl.NavType.dialogs),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogReg, NavigatorImpl.NavType.dialog),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._wallReg, NavigatorImpl.NavType.wallPost),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedWallReg, NavigatorImpl.NavType.wallPost),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._audiosReg, NavigatorImpl.NavType.audios),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._newsReg, NavigatorImpl.NavType.news),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._recommendedNewsReg, NavigatorImpl.NavType.recommendedNews),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedbackReg, NavigatorImpl.NavType.feedback),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._profileReg, NavigatorImpl.NavType.profile),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communityReg, NavigatorImpl.NavType.community),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photosReg, NavigatorImpl.NavType.albums),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photoReg, NavigatorImpl.NavType.photo),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._albumReg, NavigatorImpl.NavType.album),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._tagReg, NavigatorImpl.NavType.tagPhoto),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videosReg, NavigatorImpl.NavType.videos),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videoReg, NavigatorImpl.NavType.video),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._boardReg, NavigatorImpl.NavType.board),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._topicReg, NavigatorImpl.NavType.topic),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersSettingsReg, NavigatorImpl.NavType.stickersSettings),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._settingsReg, NavigatorImpl.NavType.settings),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._faveReg, NavigatorImpl.NavType.fave),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._appsReg, NavigatorImpl.NavType.apps),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._appReg, NavigatorImpl.NavType.app),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketAlbumReg, NavigatorImpl.NavType.marketAlbum),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketReg, NavigatorImpl.NavType.market),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._productReg, NavigatorImpl.NavType.product),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersReg, NavigatorImpl.NavType.stickers),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersPackReg, NavigatorImpl.NavType.stickersPack),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._giftsReg, NavigatorImpl.NavType.gifts),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._giftsCatalog, NavigatorImpl.NavType.giftsCatalog),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._namedObjReg, NavigatorImpl.NavType.namedObject)
    };
        private List<string> _history = new List<string>();
        private const string VK_ME_DOMAIN = "vk.me/";
        private bool _isResolvingScreenName;
        private bool _isNavigatingToGame;

        private static Frame NavigationService
        {
            get
            {
                return (Frame)Application.Current.RootVisual;
            }
        }

        public List<string> History
        {
            get
            {
                return this._history.Skip<string>(Math.Max(0, this._history.Count<string>() - 10)).Take<string>(10).ToList<string>();
            }
        }

        public void GoBack()
        {
            Logger.Instance.Info("Navigator.GoBack");
            this._history.Add("Back");
            if (!NavigatorImpl.NavigationService.CanGoBack)
            {
                FramePageUtils.CurrentPage.SwitchNavigationEffects();
                ParametersRepository.SetParameterForId("SwitchNavigationEffects", true);
                Navigator.Current.NavigateToMainPage();
            }
            else
                NavigatorImpl.NavigationService.GoBack();
        }

        public void NavigateToWebUri(string uri, bool forceWebNavigation = false, bool fromPush = false)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;
            if (uri.StartsWith("tel:"))
            {
                try
                {
                    PhoneCallTask phoneCallTask = new PhoneCallTask();
                    string str = uri.Substring(4);
                    phoneCallTask.PhoneNumber = str;
                    phoneCallTask.Show();
                }
                catch
                {
                }
            }
            else
            {
                if (!uri.StartsWith("http://", (StringComparison)3) && !uri.StartsWith("https://", (StringComparison)3))
                    uri = "http://" + uri;
                Logger.Instance.Info("Navigator.NavigateToWebUri, uri={0}, forceWebNavigation={1}", uri, forceWebNavigation);
                bool flag = false;
                if (!forceWebNavigation)
                    flag = this.GetWithinAppNavigationUri(uri, fromPush, null);
                if (flag)
                    return;
                uri = this.PrepareWebUri(uri);
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                Uri uri1 = new Uri(uri, UriKind.Absolute);
                webBrowserTask.Uri = uri1;
                webBrowserTask.Show();
            }
        }

        private string PrepareWebUri(string uri)
        {
            if (!NavigatorImpl.IsVKUri(uri))
                return "http://m.vk.com/away.php?to=" + WebUtility.UrlEncode(uri);
            return uri;
        }

        public bool GetWithinAppNavigationUri(string uri, bool fromPush = false, Action<bool> customCallback = null)
        {
            if (!NavigatorImpl.IsVKUri(uri))
                return false;
            string uri1 = uri;
            int num = uri1.IndexOf("://", (StringComparison)2);
            if (num > -1)
                uri1 = uri1.Remove(0, num + 3);
            int count = uri1.IndexOf("/", (StringComparison)2);
            if (count > -1)
                uri1 = uri1.Remove(0, count);
            if (uri1.StartsWith("dev/") || uri1.StartsWith("dev") && uri1.Length == 3)
                return false;
            Dictionary<string, string> queryString = HttpUtils.ParseQueryString(uri);
            if (uri1.StartsWith("/feed") && queryString.ContainsKey("section") && queryString["section"] == "search")
            {
                this.NavigateToNewsSearch(HttpUtility.UrlDecode(queryString.ContainsKey("q") ? queryString["q"] : ""));
                return true;
            }
            long id1;
            string id1String;
            long id2;
            string id2String;
            string objName;
            string objSub;
            NavigatorImpl.NavType navigationType = this.GetNavigationType(uri1, out id1, out id1String, out id2, out id2String, out objName, out objSub);
            if (navigationType == NavigatorImpl.NavType.none)
                return false;
            if (id1 == 0L)
                id1 = AppGlobalStateManager.Current.LoggedInUserId;
            bool flag = true;
            switch (navigationType)
            {
                case NavigatorImpl.NavType.friends:
                    this.NavigateToFriends(id1, "", false, FriendsPageMode.Default);
                    break;
                case NavigatorImpl.NavType.communities:
                    this.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0, 0, "", false, "", 0L);
                    break;
                case NavigatorImpl.NavType.dialogs:
                    this.NavigateToConversations();
                    break;
                case NavigatorImpl.NavType.news:
                    this.NavigateToNewsFeed(0, false);
                    break;
                case NavigatorImpl.NavType.tagPhoto:
                    this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0, "2", "0", "", 0, "", "", false, 0, false);
                    break;
                case NavigatorImpl.NavType.albums:
                    this.NavigateToPhotoAlbums(false, Math.Abs(id1), id1 < 0, 0);
                    break;
                case NavigatorImpl.NavType.profile:
                    this.NavigateToUserProfile(id1, "", "", false);
                    break;
                case NavigatorImpl.NavType.dialog:
                    this.NavigateToConversation(id1, false, false, "", 0, false);
                    break;
                case NavigatorImpl.NavType.community:
                    this.NavigateToGroup(id1, "", false);
                    break;
                case NavigatorImpl.NavType.board:
                    this.NavigateToGroupDiscussions(id1, "", 0, false, false);
                    break;
                case NavigatorImpl.NavType.album:
                    long albumIdLong = AlbumTypeHelper.GetAlbumIdLong(id2String);
                    AlbumType albumType = AlbumTypeHelper.GetAlbumType(albumIdLong);
                    this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0, albumType.ToString(), albumIdLong.ToString(), "", 0, "", "", false, 0, false);
                    break;
                case NavigatorImpl.NavType.video:
                    this.NavigateToVideoWithComments(null, id1, id2, "");
                    break;
                case NavigatorImpl.NavType.audios:
                    this.NavigateToAudio(0, Math.Abs(id1), id1 < 0, 0, 0, "");
                    break;
                case NavigatorImpl.NavType.topic:
                    flag = false;
                    break;
                case NavigatorImpl.NavType.photo:
                    this.NavigateToPhotoWithComments(null, null, id1, id2, "", false, false);
                    break;
                case NavigatorImpl.NavType.wallPost:
                    this.NavigateToWallPostComments(id2, id1, false, 0, 0, "");
                    break;
                case NavigatorImpl.NavType.namedObject:
                    this.ResolveScreenNameNavigationObject(uri, objName, fromPush, customCallback);
                    break;
                case NavigatorImpl.NavType.stickersSettings:
                    this.NavigateToStickersManage();
                    break;
                case NavigatorImpl.NavType.settings:
                    this.NavigateToSettings();
                    break;
                case NavigatorImpl.NavType.feedback:
                    this.NavigateToFeedback();
                    break;
                case NavigatorImpl.NavType.videos:
                    this.NavigateToVideo(false, Math.Abs(id1), id1 < 0, false);
                    break;
                case NavigatorImpl.NavType.fave:
                    this.NavigateToFavorites();
                    break;
                case NavigatorImpl.NavType.apps:
                    if (AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
                    {
                        this.NavigateToGames(0, fromPush);
                        break;
                    }
                    flag = false;
                    break;
                case NavigatorImpl.NavType.marketAlbum:
                    this.NavigateToMarketAlbumProducts(id1, id2, null);
                    break;
                case NavigatorImpl.NavType.market:
                    this.NavigateToMarket(id1);
                    break;
                case NavigatorImpl.NavType.product:
                    this.NavigateToProduct(id1, id2);
                    break;
                case NavigatorImpl.NavType.stickers:
                    this.NavigateToStickersStore(0, false);
                    break;
                case NavigatorImpl.NavType.stickersPack:
                    NavigatorImpl.ShowStickersPack(objSub);
                    break;
                case NavigatorImpl.NavType.recommendedNews:
                    this.NavigateToNewsFeed(NewsSources.Suggestions.PickableItem.ID, false);
                    break;
                case NavigatorImpl.NavType.app:
                    this.NavigateToGame(id1, id2, uri, fromPush, customCallback);
                    break;
                case NavigatorImpl.NavType.gifts:
                    EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.link, GiftPurchaseStepsAction.gifts_page));
                    this.NavigateToGifts(id1, "", "");
                    break;
                case NavigatorImpl.NavType.giftsCatalog:
                    EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.link, GiftPurchaseStepsAction.store));
                    this.NavigateToGiftsCatalog(0, false);
                    break;
            }
            return flag;
        }

        private static bool IsVKUri(string uri)
        {
            uri = uri.ToLowerInvariant();
            uri = uri.Replace("http://", "").Replace("https://", "");
            if (uri.StartsWith("m.") || uri.StartsWith("t.") || uri.StartsWith("0."))
                uri = uri.Remove(0, 2);
            if (uri.StartsWith("www.") || uri.StartsWith("new."))
                uri = uri.Remove(0, 4);
            if (!uri.StartsWith("vk.com/") && !uri.StartsWith("vkontakte.ru/"))
                return uri.StartsWith("vk.me/");
            return true;
        }

        private NavigatorImpl.NavType GetNavigationType(string uri, out long id1, out string id1String, out long id2, out string id2String, out string obj, out string objSub)
        {
            id1 = id2 = 0L;
            id1String = id2String = "";
            obj = objSub = "";
            foreach (NavigatorImpl.NavigationTypeMatch navTypes1 in this._navTypesList)
            {
                if (navTypes1.Check(uri))
                {
                    if (navTypes1.SubTypes.Count > 0)
                    {
                        foreach (string subType in navTypes1.SubTypes)
                        {
                            foreach (NavigatorImpl.NavigationTypeMatch navTypes2 in this._navTypesList)
                            {
                                if (navTypes2.Check(subType))
                                {
                                    id1 = navTypes2.Id1;
                                    id2 = navTypes2.Id2;
                                    id1String = navTypes2.Id1String;
                                    id2String = navTypes2.Id2String;
                                    obj = navTypes2.ObjName;
                                    objSub = navTypes2.ObjSubName;
                                    return navTypes2.MatchType;
                                }
                            }
                        }
                    }
                    id1 = navTypes1.Id1;
                    id2 = navTypes1.Id2;
                    id1String = navTypes1.Id1String;
                    id2String = navTypes1.Id2String;
                    obj = navTypes1.ObjName;
                    objSub = navTypes1.ObjSubName;
                    return navTypes1.MatchType;
                }
            }
            return NavigatorImpl.NavType.none;
        }

        private static void ShowStickersPack(string stickersPackName)
        {
            stickersPackName = stickersPackName.Replace("/", "");
            if (string.IsNullOrWhiteSpace(stickersPackName))
                return;
            CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.message;
            StickersPackView.Show(stickersPackName, "link");
        }

        private void ResolveScreenNameNavigationObject(string uri, string objName, bool fromPush, Action<bool> customCallback = null)
        {
            if (this._isResolvingScreenName)
            {
                return;
            }
            this._isResolvingScreenName = true;
            AccountService.Instance.ResolveScreenName(objName.Replace("/", ""), delegate(BackendResult<ResolvedData, ResultCode> res)
            {
                this._isResolvingScreenName = false;
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    Execute.ExecuteOnUIThread(delegate
                    {
                        ResolvedData expr_0B = res.ResultData;
                        if (((expr_0B != null) ? expr_0B.resolvedObject : null) != null)
                        {
                            ResolvedObject resolvedObject = res.ResultData.resolvedObject;
                            bool flag = false;
                            int num = uri.IndexOf("://", 2);
                            if (num > -1)
                            {
                                string text = uri.Remove(0, num + "://".Length);
                                if (!string.IsNullOrEmpty(text) && text.StartsWith("vk.me/"))
                                {
                                    flag = true;
                                }
                            }
                            if (resolvedObject.type == "user")
                            {
                                if (flag)
                                {
                                    this.NavigateToConversation(resolvedObject.object_id, false, false, "", 0, false);
                                }
                                else
                                {
                                    this.NavigateToUserProfile(resolvedObject.object_id, "", "", false);
                                }
                                Action<bool> expr_E3 = customCallback;
                                if (expr_E3 == null)
                                {
                                    return;
                                }
                                expr_E3.Invoke(true);
                                return;
                            }
                            else if (resolvedObject.type == "group")
                            {
                                if (flag)
                                {
                                    this.NavigateToConversation(-resolvedObject.object_id, false, false, "", 0, false);
                                }
                                else
                                {
                                    this.NavigateToGroup(resolvedObject.object_id, "", false);
                                }
                                Action<bool> expr_14F = customCallback;
                                if (expr_14F == null)
                                {
                                    return;
                                }
                                expr_14F.Invoke(true);
                                return;
                            }
                            else
                            {
                                if (resolvedObject.type == "application" && res.ResultData.app != null && AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
                                {
                                    Game app = res.ResultData.app;
                                    this.NavigateToGame(app, 0, uri, fromPush, customCallback);
                                    return;
                                }
                                if (customCallback == null)
                                {
                                    this.NavigateToWebUri(uri, true, false);
                                    return;
                                }
                                customCallback.Invoke(false);
                                return;
                            }
                        }
                        else
                        {
                            if (customCallback == null)
                            {
                                this.NavigateToWebUri(uri, true, false);
                                return;
                            }
                            customCallback.Invoke(false);
                            return;
                        }
                    });
                    return;
                }
                if (customCallback == null)
                {
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                    return;
                }
                customCallback.Invoke(false);
            });
        }


        private void NavigateToGame(long appId, long sourceId, string uri, bool fromPush, Action<bool> customCallback)
        {
            if (this._isNavigatingToGame)
                return;
            this._isNavigatingToGame = true;
            AppsService.Instance.GetApp(appId, (Action<BackendResult<VKList<Game>, ResultCode>>)(result =>
            {
                this._isNavigatingToGame = false;
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    VKList<Game> resultData = result.ResultData;
                    Game app;
                    if (resultData == null)
                    {
                        app = null;
                    }
                    else
                    {
                        List<Game> items = resultData.items;
                        app = items != null ? items.FirstOrDefault<Game>() : null;
                    }
                    this.NavigateToGame(app, sourceId, uri, fromPush, customCallback);
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }));
        }

        private void NavigateToGame(Game app, long sourceId, string uri, bool fromPush, Action<bool> customCallback)
        {
            if (app == null)
                return;
            long id = app.id;
            int num1 = this.TryOpenGame(new List<Game>() { app }, fromPush) ? 1 : 0;
            string utmParamsStr = "";
            if (!string.IsNullOrEmpty(uri))
            {
                int num2 = uri.IndexOf("?", (StringComparison)2);
                int num3 = uri.IndexOf("#", (StringComparison)2);
                if (num2 > -1 || num3 > -1)
                {
                    int startIndex = -1;
                    if (num2 > -1)
                    {
                        if (num3 > -1)
                            startIndex = num2 >= num3 ? num3 : num2 + 1;
                    }
                    else
                        startIndex = num3;
                    if (startIndex > -1)
                        utmParamsStr = uri.Substring(startIndex);
                }
            }
            if (num1 == 0 && id != 0L)
            {
                if (customCallback == null)
                    this.NavigateToProfileAppPage(id, sourceId, utmParamsStr);
                else
                    customCallback(false);
            }
            else
            {
                if (customCallback == null)
                    return;
                customCallback(true);
            }
        }

        private bool TryOpenGame(List<Game> games, bool fromPush)
        {
            bool result = false;
            if (games.Count > 0)
            {
                Game game = games[0];
                if (!string.IsNullOrEmpty(game.platform_id) && game.is_in_catalog == 1)
                {
                    result = true;
                    Execute.ExecuteOnUIThread(delegate
                    {
                        PageBase currentPage = FramePageUtils.CurrentPage;
                        if (currentPage == null || currentPage is OpenUrlPage)
                        {
                            this.NavigateToGames(game.id, false);
                            return;
                        }
                        Grid grid = currentPage.Content as Grid;
                        FrameworkElement root = null;
                        if (((grid != null) ? grid.Children : null) != null && grid.Children.Count > 0)
                        {
                            root = (grid.Children[0] as FrameworkElement);
                        }
                        PageBase arg_94_0 = currentPage;
                        List<object> expr_70 = new List<object>();
                        expr_70.Add(game);
                        arg_94_0.OpenGamesPopup(expr_70, fromPush ? GamesClickSource.push : GamesClickSource.catalog, "", 0, root);
                    });
                }
            }
            return result;
        }

        private static string GetNavigateToUserProfileString(long uid, string userName = "", string source = "")
        {
            if (((ContentControl)((App)Application.Current).RootFrame).Content is ProfilePage && (((ContentControl)((App)Application.Current).RootFrame).Content as ProfilePage).ViewModel.Id == Math.Abs(uid))
                return null;
            if (uid < 0L)
                return null;
            return NavigatorImpl.GetNavigateToUserProfileNavStr(uid, userName, false, source);
        }

        public bool NavigateToUserProfile(long uid, string userName = "", string source = "", bool needClearStack = false)
        {
            string navStr = NavigatorImpl.GetNavigateToUserProfileString(uid, userName, source);
            if (navStr == null)
                return false;
            if (needClearStack)
                navStr = (!navStr.Contains("?") ? navStr + "?" : navStr + "&") + "ClearBackStack=true";
            this.Navigate(navStr);
            return true;
        }

        public static string GetNavigateToUserProfileNavStr(long uid, string userName = "", bool forbidOverrideGoBack = false, string source = "")
        {
            if (userName == null)
                userName = "";
            return string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ForbidOverrideGoBack={2}&Source={3}", uid, Extensions.ForURL(userName), forbidOverrideGoBack, source);
        }

        private static string GetNavigateTrGroupString(long groupId, string name = "")
        {
            if (((ContentControl)((App)Application.Current).RootFrame).Content is ProfilePage && (((ContentControl)((App)Application.Current).RootFrame).Content as ProfilePage).ViewModel.Id == -Math.Abs(groupId))
                return null;
            return NavigatorImpl.GetNavigateToGroupNavStr(groupId, name, false);
        }

        public bool NavigateToGroup(long groupId, string name = "", bool needClearStack = false)
        {
            string navStr = NavigatorImpl.GetNavigateTrGroupString(groupId, name);
            if (navStr == null)
                return false;
            if (needClearStack)
                navStr = (!navStr.Contains("?") ? navStr + "?" : navStr + "&") + "ClearBackStack=true";
            this.Navigate(navStr);
            return true;
        }

        public static string GetNavigateToGroupNavStr(long groupId, string name = "", bool forbidOverrideGoBack = false)
        {
            groupId = -Math.Abs(groupId);
            return string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ForbidOverrideGoBack={2}&Source={3}", groupId, Extensions.ForURL(name), forbidOverrideGoBack, CurrentCommunitySource.ToString(CurrentCommunitySource.Source));
        }

        public void NavigateToPostsSearch(long ownerId, string nameGen = "")
        {
            this.Navigate(string.Format("/VKClient.Common;component/Profiles/Shared/Views/PostsSearchPage.xaml?OwnerId={0}&NameGen={1}", ownerId, nameGen));
        }

        private static string GetNavigateToPhotoAlbumsString(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string str = string.Format("/VKClient.Photos;component/PhotosMainPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&AdminLevel={3}", pickMode, userOrGroupId, isGroup, adminLevel);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToPhotoAlbums(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoAlbumsString(pickMode, userOrGroupId, isGroup, adminLevel));
        }

        public void NavigateToNewWallPost(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false)
        {
            this.Navigate(NavigatorImpl.GetNavToNewPostStr(userOrGroupId, isGroup, adminLevel, isPublicPage, isNewTopicMode, isPostponed));
        }

        public static string GetNavToNewPostStr(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            WallPostViewModel.Mode mode1 = WallPostViewModel.Mode.NewWallPost;
            if (isNewTopicMode)
                mode1 = WallPostViewModel.Mode.NewTopic;
            WallPostViewModel.Mode mode2 = WallPostViewModel.Mode.EditDiscussionComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditDiscussionComment;
            mode2 = WallPostViewModel.Mode.EditPhotoComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditPhotoComment;
            mode2 = WallPostViewModel.Mode.EditVideoComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditVideoComment;
            mode2 = WallPostViewModel.Mode.EditProductComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditProductComment;
            mode2 = WallPostViewModel.Mode.EditWallComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditWallComment;
            mode2 = WallPostViewModel.Mode.EditWallPost;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditWallPost;
            mode2 = WallPostViewModel.Mode.PublishWallPost;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.PublishWallPost;
            bool flag = FramePageUtils.CurrentPage is PostCommentsPage;
            return string.Format("/VKClient.Common;component/NewPost.xaml?UserOrGroupId={0}&IsGroup={1}&AdminLevel={2}&IsPublicPage={3}&IsNewTopicMode={4}&Mode={5}&FromWallPostPage={6}&IsPostponed={7}&IsPopupNavigation=True", userOrGroupId, isGroup, adminLevel, isPublicPage, isNewTopicMode, mode1, flag, isPostponed);
        }

        public static string GetShareExternalContentpageNavStr()
        {
            return "/VKClient.Common;component/ShareExternalContentPage.xaml";
        }

        private static string GetNavigateToVideoString(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, bool forceAllowVideoUpload = false)
        {
            if (!pickMode && isGroup)
                return string.Format("/VKClient.Video;component/VideoCatalog/GroupVideosPage.xaml?OwnerId={0}", -userOrGroupId);
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            if (!pickMode && userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId)
                return NavigatorImpl.GetNavigateToVideoCatalogString();
            string str = string.Format("/VKClient.Video;component/VideoPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&ForceAllowVideoUpload={3}", pickMode.ToString(), (userOrGroupId == 0L ? AppGlobalStateManager.Current.LoggedInUserId : userOrGroupId), isGroup, forceAllowVideoUpload);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToVideo(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, bool forceAllowVideoUpload = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoString(pickMode, userOrGroupId, isGroup, forceAllowVideoUpload));
        }

        public void NavigateToVideoAlbum(long albumId, string albumName, bool pickMode = false, long userOrGroupId = 0, bool isGroup = false)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string navStr = string.Format("/VKClient.Video;component/VideoPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&AlbumId={3}&AlbumName={4}", pickMode.ToString(), userOrGroupId, isGroup, albumId.ToString(), Extensions.ForURL(albumName));
            if (pickMode)
                navStr += "&IsPopupNavigation=True";
            this.Navigate(navStr);
        }

        private static string GetNavigateToAudioString(int pickMode = 0, long userOrGroupId = 0, bool isGroup = false, long albumId = 0, long excludeAlbumId = 0, string albumName = "")
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string str = string.Format("/VKClient.Audio;component/AudioPage.xaml?PageMode={0}&UserOrGroupId={1}&IsGroup={2}&AlbumId={3}&ExcludeAlbumId={4}&AlbumName={5}", pickMode, userOrGroupId, isGroup, albumId, excludeAlbumId, Extensions.ForURL(albumName));
            if (pickMode != 0)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToAudio(int pickMode = 0, long userOrGroupId = 0, bool isGroup = false, long albumId = 0, long excludeAlbumId = 0, string albumName = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToAudioString(pickMode, userOrGroupId, isGroup, albumId, excludeAlbumId, albumName));
        }

        public void NavigateToDocuments(long ownerId = 0, bool isOwnerCommunityAdmined = false)
        {
            this.Navigate(string.Format("/VKClient.Common;component/DocumentsPage.xaml?OwnerId={0}&IsOwnerCommunityAdmined={1}", ownerId, isOwnerCommunityAdmined));
        }

        public void NavigateToPostponedPosts(long groupId = 0)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PostponedPostsPage.xaml"));
        }

        public void NavigateToWallPostComments(long postId, long ownerId, bool focusCommentsField, long pollId = 0, long pollOwnerId = 0, string adData = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToPostCommentsNavStr(postId, ownerId, focusCommentsField, pollId, pollOwnerId, adData));
        }

        public static string GetNavigateToPostCommentsNavStr(long postId, long ownerId, bool focusCommentsField, long pollId = 0, long pollOwnerId = 0, string adData = "")
        {
            return string.Format("/VKClient.Common;component/PostCommentsPage.xaml?PostId={0}&OwnerId={1}&FocusComments={2}&PollId={3}&PollOwnerId={4}&AdData={5}", postId, ownerId, focusCommentsField, pollId, pollOwnerId, adData);
        }

        public void NavigateToFriendsList(long lid, string listName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendsPage.xaml?ListId={0}&ListName={1}", lid.ToString(), Extensions.ForURL(listName)));
        }

        public void NavigateToFollowers(long userOrGroupId, bool isGroup, string name)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FollowersPage.xaml?UserOrGroupId={0}&IsGroup={1}&Name={2}", userOrGroupId, isGroup, Extensions.ForURL(name)));
        }

        public void NavigateToSubscriptions(long userId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Profiles/Users/Views/SubscriptionsPage.xaml?UserId={0}", userId));
        }

        private static string GetNavigateToFriendsString(long userId, string name, bool mutual, FriendsPageMode mode = FriendsPageMode.Default)
        {
            return string.Format("/VKClient.Common;component/FriendsPage.xaml?UserId={0}&Name={1}&Mutual={2}&Mode={3}", userId, Extensions.ForURL(name), mutual.ToString(), mode.ToString());
        }

        public void NavigateToFriends(long userId, string name, bool mutual, FriendsPageMode mode = FriendsPageMode.Default)
        {
            this.Navigate(NavigatorImpl.GetNavigateToFriendsString(userId, name, mutual, mode));
        }

        public void NavigateToFriendRequests(bool areSuggestedFriends)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendRequestsPage.xaml?AreSuggestedFriends={0}", areSuggestedFriends));
        }

        public static string GetNavigateToSettingsStr()
        {
            return "/VKClient.Common;component/SettingsPage.xaml";
        }

        public void NavigateToSettings()
        {
            this.Navigate(NavigatorImpl.GetNavigateToSettingsStr());
        }

        public void NavigateToImageViewer(string aid, int albumType, long userOrGroupId, bool isGroup, int photosCount, int selectedPhotoIndex, List<Photo> photos, Func<int, Image> getImageByIdFunc)
        {
            ImageViewerDecoratorUC.ShowPhotosFromAlbum(aid, albumType, userOrGroupId, isGroup, photosCount, selectedPhotoIndex, photos, getImageByIdFunc);
        }

        public void NavigateToImageViewerPhotosOrGifs(int selectedIndex, List<PhotoOrDocument> photosOrDocuments, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false, FrameworkElement currentViewControl = null, Action<int> setContextOnCurrentViewControl = null, Action<int, bool> showHideOverlay = null, bool shareButtonOnly = false)
        {
            ImageViewerDecoratorUC.ShowPhotosOrGifsById(selectedIndex, photosOrDocuments, fromDialog, friendsOnly, getImageByIdFunc, page, hideActions, currentViewControl, setContextOnCurrentViewControl, showHideOverlay, shareButtonOnly);
        }

        public void NavigateToImageViewer(int photosCount, int initialOffset, int selectedPhotoIndex, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, string viewerMode, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false)
        {
            ImageViewerDecoratorUC.ShowPhotosById(photosCount, initialOffset, selectedPhotoIndex, photoIds, ownerIds, accessKeys, photos, fromDialog, friendsOnly, getImageByIdFunc, page, hideActions);
        }

        public void NaviateToImageViewerPhotoFeed(long userOrGroupId, bool isGroup, string aid, int photosCount, int selectedPhotoIndex, int date, List<Photo> photos, string mode, Func<int, Image> getImageByIdFunc)
        {
            ImageViewerDecoratorUC.ShowPhotosFromFeed(userOrGroupId, isGroup, aid, photosCount, selectedPhotoIndex, date, photos, mode, getImageByIdFunc);
        }

        private static string GetNavigateToPhotoWithComments(Photo photo, PhotoWithFullInfo photoWithFullInfo, long ownerId, long pid, string accessKey, bool fromDialog = false, bool friendsOnly = false)
        {
            ParametersRepository.SetParameterForId("Photo", photo);
            ParametersRepository.SetParameterForId("PhotoWithFullInfo", photoWithFullInfo);
            return string.Format("/VKClient.Photos;component/PhotoCommentsPage.xaml?ownerId={0}&pid={1}&accessKey={2}&FromDialog={3}&FriendsOnly={4}", ownerId, pid, Extensions.ForURL(accessKey), fromDialog, friendsOnly);
        }

        public void NavigateToPhotoWithComments(Photo photo, PhotoWithFullInfo photoWithFullInfo, long ownerId, long pid, string accessKey, bool fromDialog = false, bool friendsOnly = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoWithComments(photo, photoWithFullInfo, ownerId, pid, accessKey, fromDialog, friendsOnly));
        }

        public void NavigateToLikesPage(long ownerId, long itemId, int type, int knownCount = 0, bool selectFriendLikes = false)
        {
            this.Navigate(string.Format("/VKClient.Common;component/LikesPage.xaml?OwnerId={0}&ItemId={1}&Type={2}&knownCount={3}&SelectFriendLikes={4}", ownerId, itemId, type, knownCount, selectFriendLikes));
        }

        public void PickAlbumToMovePhotos(long userOrGroupId, bool isGroup, string excludeAid, List<long> list, int adminLevel = 0)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            this.Navigate(string.Format("/VKClient.Photos;component/PhotosMainPage.xaml?UserOrGroupId={0}&IsGroup={1}&SelectForMove=True&ExcludeId={2}&SelectedPhotos={3}&AdminLevel={4}", userOrGroupId, isGroup, excludeAid, list.GetCommaSeparated(), adminLevel) + "&IsPopupNavigation=True");
        }

        private static string GetNavigateToGroupsString(long userId, string name = "", bool pickManaged = false, long owner_id = 0, long pic_id = 0, string text = "", bool isGif = false, string accessKey = "", long excludedId = 0)
        {
            ParametersRepository.SetParameterForId("ShareText", text);
            string str = string.Format("/VKClient.Groups;component/GroupsListPage.xaml?UserId={0}&Name={1}&PickManaged={2}&OwnerId={3}&PicId={4}&IsGif={5}&AccessKey={6}&ExcludedId={7}", userId, name, pickManaged, owner_id, pic_id, isGif, Extensions.ForURL(accessKey), excludedId);
            if (pickManaged)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToGroups(long userId, string name = "", bool pickManaged = false, long owner_id = 0, long pic_id = 0, string text = "", bool isGif = false, string accessKey = "", long excludedId = 0)
        {
            this.Navigate(NavigatorImpl.GetNavigateToGroupsString(userId, name, pickManaged, owner_id, pic_id, text, isGif, accessKey, excludedId));
        }

        public void NavigateToMap(bool pick, double latitude, double longitude)
        {
            this.Navigate(string.Format("/VKClient.Common;component/MapAttachmentPage.xaml?latitude={0}&longitude={1}&Pick={2}", latitude.ToString((IFormatProvider)CultureInfo.InvariantCulture), longitude.ToString((IFormatProvider)CultureInfo.InvariantCulture), pick.ToString()));
        }

        public void NavigateToPostSchedule(DateTime? dateTime = null)
        {
            long num = 0;
            if (dateTime.HasValue)
                num = dateTime.Value.Ticks;
            this.Navigate(string.Format("/VKClient.Common;component/PostSchedulePage.xaml?PublishDateTime={0}", num) + "&IsPopupNavigation=True");
        }

        private static string GetNavigateToGroupDiscussionsString(long gid, string name, int adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            return string.Format("/VKClient.Groups;component/GroupDiscussionsPage.xaml?GroupId={0}&Name={1}&AdminLevel={2}&IsPublicPage={3}&CanCreateTopic={4}", gid, name, adminLevel, isPublicPage, canCreateTopic);
        }

        public void NavigateToGroupDiscussions(long gid, string name, int adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            this.Navigate(NavigatorImpl.GetNavigateToGroupDiscussionsString(gid, name, adminLevel, isPublicPage, canCreateTopic));
        }

        public void NavigateToGroupDiscussion(long gid, long tid, string topicName, int knownCommentsCount, bool loadFromEnd, bool canComment)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/GroupDiscussionPage.xaml?GroupId={0}&TopicId={1}&TopicName={2}&KnownCommentsCount={3}&LoadFromTheEnd={4}&CanComment={5}", gid, tid, Extensions.ForURL(topicName), knownCommentsCount, loadFromEnd, canComment));
        }

        public static string GetNavToFeedbackStr()
        {
            return "/VKClient.Common;component/FeedbackPage.xaml";
        }

        public void NavigateToFeedback()
        {
            this.Navigate(NavigatorImpl.GetNavToFeedbackStr());
        }

        private static string GetNavigateToVideoWithCommentsString(VKClient.Common.Backend.DataObjects.Video video, long ownerId, long videoId, string accessKey = "")
        {
            ParametersRepository.SetParameterForId("Video", video);
            StatisticsActionSource videoSource = CurrentMediaSource.VideoSource;
            string videoContext = CurrentMediaSource.VideoContext;
            return string.Format("/VKClient.Video;component/VideoCommentsPage.xaml?OwnerId={0}&VideoId={1}&AccessKey={2}&VideoSource={3}&VideoContext={4}", ownerId, videoId, Extensions.ForURL(accessKey), videoSource, Extensions.ForURL(videoContext));
        }

        public void NavigateToVideoWithComments(VKClient.Common.Backend.DataObjects.Video video, long ownerId, long videoId, string accessKey = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoWithCommentsString(video, ownerId, videoId, accessKey));
        }

        public void NavigateToConversationsSearch()
        {
            this.Navigate("/VKMessenger;component/Views/ConversationsSearch.xaml" + "?IsPopupNavigation=True");
        }

        public static string GetNavToConversationStr(long userOrChatId, bool isChat, bool fromLookup = false, string newMessageContents = "", long messageId = 0, bool isContactSellerMode = false)
        {
            return string.Format("/VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}&FromLookup={2}&NewMessageContents={3}&MessageId={4}&IsContactProductSellerMode={5}", userOrChatId, isChat, fromLookup, !string.IsNullOrEmpty(newMessageContents), messageId, isContactSellerMode);
        }

        public void NavigateToConversation(long userOrChatId, bool isChat, bool fromLookup = false, string newMessageContents = "", long messageId = 0, bool isContactSellerMode = false)
        {
            if (!string.IsNullOrEmpty(newMessageContents))
                ParametersRepository.SetParameterForId("NewMessageContents", newMessageContents);
            this.Navigate(NavigatorImpl.GetNavToConversationStr(userOrChatId, isChat, fromLookup, newMessageContents, messageId, isContactSellerMode));
        }

        public void NavigateToWelcomePage()
        {
            this.Navigate("/VKClient;component/WelcomePage.xaml");
        }

        private void Navigate(string navStr)
        {
            Logger.Instance.Info("Navigator.Navigate, navStr={0}", new object[]
	{
		navStr
	});
            this._history.Add(navStr);
            if (this._history.Count > 100)
            {
                this._history = Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Skip<string>(this._history, Math.Max(0, Enumerable.Count<string>(this._history) - 10)), 10));
            }
            this.HandleIsPopupAnimationIfNeeded(navStr);
            Execute.ExecuteOnUIThread(delegate
            {
                NavigatorImpl.NavigationService.Navigate(new Uri(navStr, UriKind.Relative));
            });
        }


        private void HandleIsPopupAnimationIfNeeded(string navStr)
        {
        }

        public void NavigateToPickUser(bool createChat, long initialUserId, bool goBackOnResult, int currentCountInChat = 0, PickUserMode mode = PickUserMode.PickForMessage, string customTitle = "", int sexFilter = 0, bool isGlobalSearchForbidden = false)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PickUserForNewMessagePage.xaml?CreateChat={0}&InitialUserId={1}&GoBackOnResult={2}&CurrentCountInChat={3}&PickMode={4}&CustomTitle={5}&SexFilter={6}&IsGlobalSearchForbidden={7}", createChat, initialUserId, goBackOnResult, currentCountInChat, mode, customTitle, sexFilter, isGlobalSearchForbidden) + "&IsPopupNavigation=True");
        }

        public void NavigateToPickUser(long productId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PickUserForNewMessagePage.xaml?ProductId={0}&GoBackOnResult={1}&PickMode={2}&IsGlobalSearchForbidden={3}", productId, true, PickUserMode.PickForStickerPackGift, false) + "&IsPopupNavigation=True");
        }

        public void NavigateToPickConversation()
        {
            this.Navigate("/VKMessenger;component/Views/PickConversationPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToAudioPlayer(bool startPlaying = false)
        {
            this.Navigate(string.Format("/VKClient.Audio;component/Views/AudioPlayer.xaml?startPlaying={0}", startPlaying) + "&IsPopupNavigation=True");
        }

        public void NavigateToGroupInvitations()
        {
            this.Navigate("/VKClient.Groups;component/GroupInvitationsPage.xaml");
        }

        private static string GetNavigateToPhotoAlbumString(long userOrGroupId, bool isGroup, string type, string aid, string albumName = "", int photosCount = 0, string title = "", string description = "", bool pickMode = false, int adminLevel = 0, bool forceCanUpload = false)
        {
            string str = string.Format("/VKClient.Photos;component/PhotoAlbumPage.xaml?userOrGroupId={0}&isGroup={1}&albumType={2}&albumId={3}&albumName={4}&photosCount={5}&pageTitle={6}&albumDesc={7}&PickMode={8}&AdminLevel={9}&ForceCanUpload={10}", userOrGroupId, isGroup, type, aid, Extensions.ForURL(albumName), photosCount, Extensions.ForURL(title), Extensions.ForURL(description), pickMode, adminLevel, forceCanUpload);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToPhotoAlbum(long userOrGroupId, bool isGroup, string type, string aid, string albumName = "", int photosCount = 0, string title = "", string description = "", bool pickMode = false, int adminLevel = 0, bool forceCanUpload = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoAlbumString(userOrGroupId, isGroup, type, aid, albumName, photosCount, title, description, pickMode, adminLevel, forceCanUpload));
        }

        public void NavigateToMainPage()
        {
            this.Navigate("/VKClient.Common;component/NewsPage.xaml?ClearBackStack=true");
        }

        private static string GetNavigateToFavoritesString()
        {
            return "/VKClient.Common;component/FavoritesPage.xaml";
        }

        public void NavigateToFavorites()
        {
            this.Navigate(NavigatorImpl.GetNavigateToFavoritesString());
        }

        public void NavigateToGames(long gameId = 0, bool fromPush = false)
        {
            this.Navigate(NavigatorImpl.GetGamesNavStr(gameId, fromPush));
        }

        public static string GetGamesNavStr(long gameId = 0, bool fromPush = false)
        {
            return string.Format("/VKClient.Common;component/GamesMainPage.xaml?GameId={0}&FromPush={1}", gameId, fromPush);
        }

        public void NavigateToMyGames()
        {
            this.Navigate("/VKClient.Common;component/GamesMyPage.xaml");
        }

        public void NavigateToGamesFriendsActivity(long gameId = 0, string gameName = "")
        {
            this.Navigate(string.Format("/VKClient.Common;component/GamesFriendsActivityPage.xaml?GameId={0}&GameName={1}", gameId, gameName));
        }

        public void NavigateToGamesInvites()
        {
            this.Navigate("/VKClient.Common;component/GamesInvitesPage.xaml");
        }

        public void OpenGame(Game game)
        {
            if (game == null)
                return;
            if (InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id))
            {
                this.OpenGame(game.id);
            }
            else
            {
                MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
                string platformId = game.platform_id;
                marketplaceDetailTask.ContentIdentifier = platformId;
                int num = 1;
                marketplaceDetailTask.ContentType = ((MarketplaceContentType)num);
                marketplaceDetailTask.Show();
            }
        }

        public async void OpenGame(long gameId)
        {
            if (gameId <= 0L)
                return;
            await Launcher.LaunchUriAsync(new Uri(string.Format("vk{0}://", gameId)));
        }

        public void NavigateToGameSettings(long gameId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/GameSettingsPage.xaml?GameId={0}", gameId));
        }

        public void NavigateToManageSources(ManageSourcesMode mode = ManageSourcesMode.ManageHiddenNewsSources)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ManageSourcesPage.xaml?Mode={0}", mode));
        }

        public void NavigateToPhotoPickerPhotos(int maxAllowedToSelect, bool ownPhotoPick = false, bool pickToStorageFile = false)
        {
            this.Navigate(string.Format("/VKClient.Photos;component/PhotoPickerPhotos.xaml?MaxAllowedToSelect={0}&OwnPhotoPick={1}&PickToStorageFile={2}&IsPopupNavigation=True", maxAllowedToSelect, ownPhotoPick, pickToStorageFile));
        }

        public void NavigationToValidationPage(string validationUri)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ValidatePage.xaml?ValidationUri={0}", Extensions.ForURL(validationUri)) + "&IsPopupNavigation=True");
        }

        public void NavigateToMoneyTransferAcceptConfirmation(string url, long transferId, long fromId, long toId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ValidatePage.xaml?ValidationUri={0}&IsAcceptMoneyTransfer={1}&TransferId={2}&FromId={3}&ToId={4}", Extensions.ForURL(url), true, transferId, fromId, toId) + "&IsPopupNavigation=True");
        }

        public void NavigateToMoneyTransferSendConfirmation(string url)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ValidatePage.xaml?ValidationUri={0}&IsMoneyTransfer={1}", Extensions.ForURL(url), true) + "&IsPopupNavigation=True");
        }

        public void NavigateTo2FASecurityCheck(string username, string password, string phoneMask, string validationType, string validationSid)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Auth2FAPage.xaml?username={0}&password={1}&phoneMask={2}&validationType={3}&validationSid={4}&IsPopupNavigation=True", Extensions.ForURL(username), Extensions.ForURL(password), Extensions.ForURL(phoneMask), Extensions.ForURL(validationType), Extensions.ForURL(validationSid)));
        }

        public void NavigateToAddNewVideo(string filePath, long ownerId)
        {
            this.Navigate(string.Format("/VKClient.Video;component/AddEditVideoPage.xaml?VideoToUploadPath={0}&OwnerId={1}", Extensions.ForURL(filePath), ownerId) + "&IsPopupNavigation=True");
        }

        public void NavigateToAddNewAudio(StorageFile file)
        {
            ParametersRepository.SetParameterForId("AudioForUpload", file);
            this.Navigate("/VKClient.Audio;component/AddEditAudioPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToEditVideo(long ownerId, long videoId, VKClient.Common.Backend.DataObjects.Video video = null)
        {
            if (video != null)
                ParametersRepository.SetParameterForId("VideoForEdit", video);
            this.Navigate(string.Format("/VKClient.Video;component/AddEditVideoPage.xaml?OwnerId={0}&VideoId={1}", ownerId, videoId) + "&IsPopupNavigation=True");
        }

        public void NavigateToEditAudio(AudioObj audio = null)
        {
            ParametersRepository.SetParameterForId("AudioForEdit", audio);
            this.Navigate("/VKClient.Audio;component/AddEditAudioPage.xaml?IsPopupNavigation=True");
        }

        private static string GetNavigateToNewsFeedString(long newsSourceId = 0, bool photoFeedMoveTutorial = false)
        {
            return string.Format("/VKClient.Common;component/NewsPage.xaml?NewsSourceId={0}&PhotoFeedMoveTutorial={1}", newsSourceId, photoFeedMoveTutorial);
        }

        public void NavigateToNewsFeed(long newsSourceId = 0, bool photoFeedMoveTutorial = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToNewsFeedString(newsSourceId, photoFeedMoveTutorial));
        }

        private string GetNavigateToNewsSearchString(string query = "")
        {
            return string.Format("/VKClient.Common;component/NewsSearchPage.xaml?Query={0}", HttpUtility.UrlEncode(query));
        }

        public void NavigateToNewsSearch(string query = "")
        {
            this.Navigate(this.GetNavigateToNewsSearchString(query));
        }

        private static string GetNavigateToConversationsString()
        {
            return "/VKMessenger;component/ConversationsPage.xaml";
        }

        public void NavigateToConversations()
        {
            this.Navigate(NavigatorImpl.GetNavigateToConversationsString());
        }

        public IPhotoPickerPhotosViewModel GetPhotoPickerPhotosViewModelInstance(int maxAllowedToSelect)
        {
            return (IPhotoPickerPhotosViewModel)new PhotoPickerPhotosViewModel(maxAllowedToSelect, false);
        }

        public void NavigateToBlacklist()
        {
            this.Navigate("/VKClient.Common;component/BannedUsersPage.xaml");
        }

        public void NavigateToBirthdaysPage()
        {
            this.Navigate(NavigatorImpl.GetNavToBirthdaysStr());
        }

        public static string GetNavToBirthdaysStr()
        {
            return "/VKClient.Common;component/BirthdaysPage.xaml";
        }

        public void NavigateToSuggestedPostponedPostsPage(long userOrGroupId, bool isGroup, int mode)
        {
            this.Navigate(string.Format("/VKClient.Common;component/SuggestedPostponedPostsPage.xaml?UserOrGroupId={0}&IsGroup={1}&Mode={2}", userOrGroupId, isGroup, mode));
        }

        public void NavigateToHelpPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/HelpPage.xaml"));
        }

        public void NavigateToAboutPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/AboutPage.xaml"));
        }

        public void NavigateFromSDKAuthPage(string callbackUriToLaunch)
        {
            ParametersRepository.SetParameterForId("CallbackUriToLaunch", callbackUriToLaunch);
            this.GoBack();
        }

        public void NavigateToEditPrivacy(EditPrivacyPageInputData inputData)
        {
            ParametersRepository.SetParameterForId("EditPrivacyInputData", inputData);
            this.Navigate("/VKClient.Common;component/EditPrivacyPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToSettingsPrivacy()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsPrivacyPage.xaml"));
        }

        public void NavigateToSettingsGeneral()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsGeneralPage.xaml"));
        }

        public void NavigateToSettingsAccount()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsAccountPage.xaml"));
        }

        public void NavigateToChangePassword()
        {
            this.Navigate(string.Format("/VKClient.Common;component/ChangePasswordPage.xaml?IsPopupNavigation=True"));
        }

        public void NavigateToChangeShortName(string currentShortName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsChangeShortNamePage.xaml?CurrentShortName={0}&IsPopupNavigation=True", Extensions.ForURL(currentShortName)));
        }

        public void NavigateToSettingsNotifications()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsNotifications.xaml"));
        }

        public void NavigateToEditProfile()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsEditProfilePage.xaml"));
        }

        public void NavigateToCreateEditPoll(long ownerId, long pollId = 0, Poll poll = null)
        {
            if (poll != null)
                ParametersRepository.SetParameterForId("Poll", poll);
            this.Navigate(string.Format("/VKClient.Common;component/CreateEditPollPage.xaml?OwnerId={0}&PollId={1}", ownerId, pollId) + "&IsPopupNavigation=True");
        }

        public void NavigateToPollVoters(long ownerId, long pollId, long answerId, string answerText)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PollVotersPage.xaml?OwnerId={0}&PollId={1}&AnswerId={2}&AnswerText={3}", ownerId, pollId, answerId, answerText));
        }

        public void NavigateToUsersSearch(string query = "")
        {
            this.Navigate("/VKClient.Common;component/UsersSearchResultsPage.xaml?IsPopupNavigation=True&Query=" + query);
        }

        public void NavigateToUsersSearchNearby()
        {
            this.Navigate("/VKClient.Common;component/UsersSearchNearbyPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToUsersSearchParams()
        {
            this.Navigate("/VKClient.Common;component/UsersSearchParamsPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsSuggestions()
        {
            this.Navigate("/VKClient.Common;component/FriendsSuggestionsPage.xaml");
        }

        public void NavigateToRegistrationPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/RegistrationPage.xaml?SessionId={0}", Guid.NewGuid().ToString()));
        }

        public void NavigateToFriendsImportFacebook()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportFacebookPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsImportGmail()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportGmailPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsImportTwitter(string oauthToken, string oauthVerifier)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendsImportTwitterPage.xaml?oauthToken={0}&oauthVerifier={1}&IsPopupNavigation=True", oauthToken, oauthVerifier));
        }

        public void NavigateToFriendsImportContacts()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportContactsPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToGroupRecommendations(int categoryId, string categoryName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/RecommendedGroupsPage.xaml?CategoryId={0}&CategoryName={1}", categoryId, Extensions.ForURL(categoryName)));
        }

        private static string GetNavigateToVideoCatalogString()
        {
            return string.Format("/VKClient.Video;component/VideoCatalog/VideoCatalogPage.xaml");
        }

        public void NavigateToVideoCatalog()
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoCatalogString());
        }

        public static string GetOpenUrlPageStr(string uriToOpen)
        {
            return string.Format("/VKClient.Common;component/OpenUrlPage.xaml?Uri={0}", Extensions.ForURL(uriToOpen));
        }

        public static string GetWebViewPageNavStr(string uri, bool supportInAppNavigation = false)
        {
            return string.Format("/VKClient.Common;component/WebViewPage.xaml?Uri={0}&SupportInAppNavigation={1}", HttpUtility.UrlEncode(uri), supportInAppNavigation);
        }

        public void NavigateToWebViewPage(string uri, bool supportInAppNavigation = false)
        {
            this.Navigate(NavigatorImpl.GetWebViewPageNavStr(uri, supportInAppNavigation));
        }

        private static string GetNavigateToMarketString(long ownerId)
        {
            return string.Format("/VKClient.Common;component/Market/Views/MarketMainPage.xaml?OwnerId={0}", ownerId);
        }

        public void NavigateToMarket(long ownerId)
        {
            this.Navigate(NavigatorImpl.GetNavigateToMarketString(ownerId));
        }

        public void NavigateToProduct(Product product)
        {
            if (product == null)
                return;
            ParametersRepository.SetParameterForId("Product", product);
            this.NavigateToProduct(product.owner_id, product.id);
        }

        private static string GetNavigateToProductString(long ownerId, long productId)
        {
            return string.Format("/VKClient.Common;component/Market/Views/ProductPage.xaml?OwnerId={0}&ProductId={1}", ownerId, productId);
        }

        public void NavigateToProduct(long ownerId, long productId)
        {
            this.Navigate(NavigatorImpl.GetNavigateToProductString(ownerId, productId));
        }

        public void NavigateToProductsSearchParams(long priceFrom, long priceTo, int currencyId, string currencyName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Market/Views/ProductsSearchParamsPage.xaml?PriceFrom={0}&PriceTo={1}&CurrencyId={2}&CurrencyName={3}", priceFrom, priceTo, currencyId, currencyName));
        }

        public void NavigateToMarketAlbums(long ownerId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Market/Views/MarketAlbumsPage.xaml?OwnerId={0}", ownerId));
        }

        private static string GetNavigateToMarketAlbumProductsString(long ownerId, long albumId, string albumName)
        {
            return string.Format("/VKClient.Common;component/Market/Views/MarketAlbumProductsPage.xaml?OwnerId={0}&AlbumId={1}&AlbumName={2}", ownerId, albumId, albumName);
        }

        public void NavigateToMarketAlbumProducts(long ownerId, long albumId, string albumName)
        {
            this.Navigate(NavigatorImpl.GetNavigateToMarketAlbumProductsString(ownerId, albumId, albumName));
        }

        public void NavigateToVideoAlbumsList(long ownerId, bool forceAllowCreateAlbum = false)
        {
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/VideoAlbumsListPage.xaml?OwnerId={0}&ForceAllowCreateAlbum={1}", ownerId, forceAllowCreateAlbum));
        }

        public void NavigateToVideoList(VKList<VideoCatalogItem> catalogItems, int source, string context, string sectionId = "", string next = "", string name = "")
        {
            ParametersRepository.SetParameterForId("CatalogItemsToShow", catalogItems);
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/VideoListPage.xaml?SectionId={0}&Next={1}&Name={2}&Source={3}&Context={4}", Extensions.ForURL(sectionId), Extensions.ForURL(next), Extensions.ForURL(name), source, Extensions.ForURL(context)));
        }

        public void NavigateToCreateEditVideoAlbum(long albumId = 0, long groupId = 0, string name = "", PrivacyInfo pi = null)
        {
            if (pi != null)
                ParametersRepository.SetParameterForId("AlbumPrivacyInfo", pi);
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/CreateEditVideoAlbumPage.xaml?AlbumId={0}&GroupId={1}&Name={2}", albumId, groupId, Extensions.ForURL(name)));
        }

        public void NavigateToAddVideoToAlbum(long ownerId, long videoId)
        {
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/AddToAlbumPage.xaml?VideoId={0}&OwnerId={1}", videoId, ownerId));
        }

        public void NavigateToConversationMaterials(long peerId)
        {
            this.Navigate("/VKMessenger;component/Views/ConversationMaterialsPage.xaml?PeerId=" + peerId);
        }

        public void NavigateToDocumentsPicker(int maxAllowedToSelect)
        {
            this.Navigate(string.Format("/VKClient.Common;component/DocumentsPickerPage.xaml?MaxAllowedToSelect={0}&IsPopupNavigation=true", maxAllowedToSelect));
        }

        public void NavigateToDocumentEditing(long ownerId, long id, string title)
        {
            this.Navigate(string.Format("/VKClient.Common;component/DocumentEditingPage.xaml?OwnerId={0}&Id={1}&Title={2}&IsPopupNavigation=true", ownerId, id, Extensions.ForURL(title)));
        }

        public void NavigateToCommunityCreation()
        {
            this.Navigate("/VKClient.Groups;component/CommunityCreationPage.xaml?IsPopupNavigation=true");
        }

        public void NavigateToCommunitySubscribers(long communityId, GroupType communityType, bool isManagement = false, bool isPicker = false, bool isBlockingPicker = false)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/CommunitySubscribersPage.xaml?CommunityId={0}&CommunityType={1}&IsManagement={2}&IsPicker={3}&IsBlockingPicker={4}", communityId, communityType, isManagement, isPicker, isBlockingPicker));
        }

        public void NavigateToStickersStore(long userOrChatId = 0, bool isChat = false)
        {
            this.Navigate(this.GetNavigateToStickersStoreString(userOrChatId, isChat));
        }

        private string GetNavigateToStickersStoreString(long userOrChatId, bool isChat)
        {
            return string.Format("/VKClient.Common;component/Stickers/Views/StickersStorePage.xaml?UserOrChatId={0}&IsChat={1}", userOrChatId, isChat);
        }

        private static string GetNavigateToStickersManageString()
        {
            return "/VKClient.Common;component/Stickers/Views/StickersManagePage.xaml";
        }

        public void NavigateToStickersManage()
        {
            this.Navigate(NavigatorImpl.GetNavigateToStickersManageString());
        }

        public void NavigateToBalance()
        {
            this.Navigate("/VKClient.Common;component/Balance/Views/BalancePage.xaml");
        }

        public void NavigateToCustomListPickerSelection(CustomListPicker parentPicker)
        {
            ParametersRepository.SetParameterForId("ParentPicker", parentPicker);
            this.Navigate("/VKClient.Common;component/UC/CustomListPicker/SelectionPage.xaml");
        }

        public void NavigateToGraffitiDrawPage(long userOrChatId, bool isChat, string title)
        {
            title = HttpUtility.UrlEncode(title);
            this.Navigate(string.Format("/VKClient.Common;component/Graffiti/Views/GraffitiDrawPage.xaml?UserOrChatId={0}&IsChat={1}&Title={2}", userOrChatId, isChat, title));
        }

        public void NavigateToSendMoneyPage(long targetId, User targetUser = null, int amount = 0, string comment = "")
        {
            if (targetUser != null)
                ParametersRepository.SetParameterForId("MoneyTransferTargetUser", targetUser);
            this.Navigate(string.Format("/VKClient.Common;component/MoneyTransfers/SendMoneyPage.xaml?TargetId={0}&Amount={1}&Comment={2}&IsPopupNavigation=true", targetId, amount, ExtensionsBase.ForURL(comment)));
        }

        public void NavigateToTransfersListPage()
        {
            this.Navigate("/VKClient.Common;component/MoneyTransfers/TransfersListPage.xaml");
        }

        public void NavigateToCommunityManagement(long communityId, GroupType communityType, bool isAdministrator)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/MainPage.xaml?CommunityId={0}&CommunityType={1}&IsAdministrator={2}", communityId, (int)communityType, isAdministrator));
        }

        public void NavigateToCommunityManagementInformation(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/Information/InformationPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementPlacementSelection(long communityId, Place place)
        {
            ParametersRepository.SetParameterForId("PlacementSelectionPlace", place);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/Information/PlacementSelectionPage.xaml?CommunityId={0}&IsPopupNavigation=true", communityId));
        }

        public void NavigateToCommunityManagementServices(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/ServicesPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementServiceSwitch(CommunityService service, CommunityServiceState currentState)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ServiceSwitchPage.xaml?Service={0}&CurrentState={1}&IsPopupNavigation=true", service, currentState));
        }

        public void NavigateToCommunityManagementManagers(long communityId, GroupType communityType)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagersPage.xaml?CommunityId={0}&CommunityType={1}", communityId, (int)communityType));
        }

        public void NavigateToCommunityManagementManagerAdding(long communityId, GroupType communityType, User user, bool fromPicker)
        {
            ParametersRepository.SetParameterForId("CommunityManager", user);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagerEditingPage.xaml?CommunityId={0}&CommunityType={1}&FromPicker={2}&IsPopupNavigation=true", communityId, (int)communityType, fromPicker));
        }

        public void NavigateToCommunityManagementManagerEditing(long communityId, GroupType communityType, User manager, bool isContact, string position, string email, string phone)
        {
            ParametersRepository.SetParameterForId("CommunityManager", manager);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagerEditingPage.xaml?CommunityId={0}&CommunityType={1}&IsContact={2}&Position={3}&Phone={4}&Email={5}&IsPopupNavigation=true", communityId, (int)communityType, isContact, Extensions.ForURL(position), Extensions.ForURL(phone), Extensions.ForURL(email)));
        }

        public void NavigateToCommunityManagementRequests(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/RequestsPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementInvitations(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/InvitationsPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementBlacklist(long communityId, GroupType communityType)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlacklistPage.xaml?CommunityId={0}&CommunityType={1}", communityId, (int)communityType));
        }

        public void NavigateToCommunityManagementBlockAdding(long communityId, User user, bool isOpenedWithoutPicker = false)
        {
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingUser", user);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockEditingPage.xaml?CommunityId={0}&IsEditing={1}&IsOpenedWithoutPicker={2}&IsPopupNavigation=true", communityId, false, isOpenedWithoutPicker));
        }

        public void NavigateToCommunityManagementBlockEditing(long communityId, User user, User manager)
        {
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingUser", user);
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingManager", manager);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockEditingPage.xaml?CommunityId={0}&IsEditing={1}&IsOpenedWithoutPicker={2}&IsPopupNavigation=true", communityId, true, false));
        }

        public void NavigateToCommunityManagementBlockDurationPicker(int durationUnixTime)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockDurationPicker.xaml?DurationUnixTime={0}&IsPopupNavigation=true", durationUnixTime));
        }

        public void NavigateToCommunityManagementLinks(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/LinksPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementLinkCreation(long communityId, GroupLink link)
        {
            if (link != null)
                ParametersRepository.SetParameterForId("CommunityLink", link);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/LinkCreationPage.xaml?CommunityId={0}&IsPopupNavigation=true", communityId));
        }

        public void NavigateToProfileAppPage(long appId, long ownerId = 0, string utmParamsStr = "")
        {
            utmParamsStr = HttpUtility.UrlEncode(utmParamsStr);
            this.Navigate(string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfileAppPage.xaml?AppId={0}&OwnerId={1}&UtmParams={2}", appId, ownerId, utmParamsStr));
        }

        public void NavigateToGifts(long userId, string firstName = "", string firstNameGen = "")
        {
            this.Navigate(string.Format("/VKClient.Common;component/Gifts/Views/GiftsPage.xaml?UserId={0}&FirstName={1}&FirstNameGen={2}", userId, firstName, firstNameGen));
        }

        public void NavigateToGiftsCatalog(long userOrChatId = 0, bool isChat = false)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Gifts/Views/GiftsCatalogPage.xaml?UserOrChatId={0}&IsChat={1}", userOrChatId, isChat));
        }

        public void NavigateToGiftsCatalogCategory(string categoryName, string title, long userOrChatId = 0, bool isChat = false)
        {
            categoryName = HttpUtility.UrlEncode(categoryName);
            title = HttpUtility.UrlEncode(title);
            this.Navigate(string.Format("/VKClient.Common;component/Gifts/Views/GiftsCatalogCategoryPage.xaml?CategoryName={0}&Title={1}&UserOrChatId={2}&IsChat={3}", categoryName, title, userOrChatId, isChat));
        }

        public void NavigateToGiftSend(long giftId, string categoryName, string description, string imageUrl, int price, int giftsLeft, List<long> userIds, bool isProduct = false)
        {
            ParametersRepository.SetParameterForId("Description", description);
            ParametersRepository.SetParameterForId("ImageUrl", imageUrl);
            ParametersRepository.SetParameterForId("Price", price);
            ParametersRepository.SetParameterForId("GiftsLeft", giftsLeft);
            ParametersRepository.SetParameterForId("UserIds", userIds);
            this.Navigate(string.Format("/VKClient.Common;component/Gifts/Views/GiftSendPage.xaml?GiftId={0}&CategoryName={1}&IsProduct={2}", giftId, categoryName, isProduct));
        }

        public void NavigateToDiagnostics()
        {
            this.Navigate("/VKClient.Common;component/Diagnostics/Views/DiagnosticsPage.xaml");
        }

        public class NavigationTypeMatch
        {
            private readonly Regex _idsRegEx = new Regex("\\-?[0-9]+");
            private readonly Regex _queryParamsRegex = new Regex("(\\?|\\&)([^=]+)\\=([^&]+)");
            private readonly Regex _regEx;

            public NavigatorImpl.NavType MatchType { get; private set; }

            public long Id1 { get; private set; }

            public string Id1String { get; private set; }

            public long Id2 { get; private set; }

            public string Id2String { get; private set; }

            public string ObjName { get; private set; }

            public string ObjSubName { get; private set; }

            public List<string> SubTypes { get; private set; }

            public NavigationTypeMatch(Regex regExp, NavigatorImpl.NavType navType)
            {
                this._regEx = regExp;
                this.MatchType = navType;
            }

            public bool Check(string uri)
            {
                MatchCollection matchCollection1 = this._regEx.Matches(uri);
                if (matchCollection1.Count == 0)
                    return false;
                Match match1 = matchCollection1[0];
                this.ObjName = match1.Value;
                if (match1.Groups.Count > 0)
                    this.ObjSubName = match1.Groups[match1.Groups.Count - 1].Value;
                MatchCollection matchCollection2 = this._idsRegEx.Matches(this.ObjName);
                if (matchCollection2.Count > 0)
                {
                    this.Id1String = matchCollection2[0].Value;
                    long result;
                    this.Id1 = long.TryParse(matchCollection2[0].Value, out result) ? result : 0L;
                }
                if (matchCollection2.Count > 1)
                {
                    this.Id2String = matchCollection2[1].Value;
                    long result;
                    this.Id2 = long.TryParse(matchCollection2[1].Value, out result) ? result : 0L;
                }
                MatchCollection matchCollection3 = this._queryParamsRegex.Matches(uri);
                this.SubTypes = new List<string>();
                foreach (Match match2 in matchCollection3)
                {
                    if (match2.Groups.Count == 4 && match2.Groups[2].Value == "w")
                        this.SubTypes.Add("/" + match2.Groups[match2.Groups.Count - 1].Value);
                }
                return true;
            }
        }

        public class NavigationSubtypeMatch
        {
        }

        public enum NavType
        {
            none,
            friends,
            communities,
            dialogs,
            news,
            tagPhoto,
            albums,
            profile,
            dialog,
            community,
            board,
            album,
            video,
            audios,
            topic,
            photo,
            wallPost,
            namedObject,
            stickersSettings,
            settings,
            feedback,
            videos,
            fave,
            apps,
            marketAlbum,
            market,
            product,
            stickers,
            stickersPack,
            recommendedNews,
            app,
            gifts,
            giftsCatalog,
        }
    }
}
