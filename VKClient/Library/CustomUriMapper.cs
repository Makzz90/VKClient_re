using Facebook.Client;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace VKClient.Library
{
  public class CustomUriMapper : UriMapperBase
  {
    public bool NeedHandleActivation;

    public CustomUriMapper()
    {
      //base.\u002Ector();
    }

    public override Uri MapUri(Uri uri)
		{
			Logger.Instance.Info("Requested uri " + uri.ToString(), new object[0]);
			string originalString = uri.OriginalString;
			bool needHandleActivation = this.NeedHandleActivation;
			this.NeedHandleActivation = false;
			if (AppGlobalStateManager.Current.LoggedInUserId == 0L)
			{
				if (originalString.StartsWith("/Protocol"))
				{
					PageBase.ProtocolLaunchAfterLogin = this.MapProtocolLaunchUri(uri);
				}
				return uri;
			}
			if (originalString.StartsWith("/Protocol"))
			{
				if (needHandleActivation)
				{
					StatsEventsTracker.Instance.Handle(new AppActivatedEvent
					{
						Reason = AppActivationReason.other_app,
						ReasonSubtype = "unknown"
					});
				}
				return this.MapProtocolLaunchUri(uri);
			}
			if (originalString.Contains("NewsPage.xaml"))
			{
				Dictionary<string, string> paramDict = uri.ParseQueryString();
				if (paramDict.ContainsKey("msg_id") && paramDict.ContainsKey("uid"))
				{
					string arg_114_0 = paramDict["uid"];
					if (needHandleActivation)
					{
						StatsEventsTracker.Instance.Handle(new AppActivatedEvent
						{
							Reason = AppActivationReason.push,
							ReasonSubtype = "message"
						});
					}
					long num;
					if (long.TryParse(arg_114_0, out num))
					{
						bool isChat = false;
						if (num - 2000000000L > 0L)
						{
							num -= 2000000000L;
							isChat = true;
						}
						return new Uri(NavigatorImpl.GetNavToConversationStr(num, isChat, false, "", 0, false) + "&ClearBackStack=true", UriKind.Relative);
					}
				}
				if (paramDict.ContainsKey("type"))
				{
					string text = paramDict["type"];
					bool arg_1B3_0 = AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled;
					if (needHandleActivation)
					{
						StatsEventsTracker.Instance.Handle(new AppActivatedEvent
						{
							Reason = AppActivationReason.push,
							ReasonSubtype = text
						});
					}
					if (arg_1B3_0 && (text == "sdk_open" || text == "sdk_request" || text == "sdk_invite"))
					{
						long gameId = 0L;
						if (paramDict.ContainsKey("app_id"))
						{
							long.TryParse(paramDict["app_id"], out gameId);
						}
						if (text == "sdk_open")
						{
							Navigator.Current.OpenGame(gameId);
						}
						if (text == "sdk_invite")
						{
							return new Uri(NavigatorImpl.GetGamesNavStr(0, true), UriKind.Relative);
						}
						return new Uri(NavigatorImpl.GetGamesNavStr(gameId, true), UriKind.Relative);
					}
					else
					{
						if (text == "open_url" && paramDict.ContainsKey("url"))
						{
							return new Uri(NavigatorImpl.GetOpenUrlPageStr(paramDict["url"]), UriKind.Relative);
						}
						if (text == "friend_found")
						{
							return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(long.Parse(paramDict["uid"]), "", false, "") + "?ClearBackStack=true", UriKind.Relative);
						}
						if (text == "friend")
						{
							return new Uri("/VKClient.Common;component/FriendRequestsPage.xaml" + "?ClearBackStack=true", UriKind.Relative);
						}
						if (text == "money_transfer")
						{
							return new Uri(NavigatorImpl.GetNavToFeedbackStr() + "?ClearBackStack=true", UriKind.Relative);
						}
						if (text == "birthday")
						{
							EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.push, GiftPurchaseStepsAction.birthdays));
							return new Uri(NavigatorImpl.GetNavToBirthdaysStr() + "?ClearBackStack=true", UriKind.Relative);
						}
						return new Uri(NavigatorImpl.GetNavToFeedbackStr() + "?ClearBackStack=true", UriKind.Relative);
					}
				}
				else
				{
					if (paramDict.ContainsKey("place"))
					{
						if (needHandleActivation && !paramDict.ContainsKey("type"))
						{
							StatsEventsTracker.Instance.Handle(new AppActivatedEvent
							{
								Reason = AppActivationReason.push,
								ReasonSubtype = "place"
							});
						}
						string text2 = paramDict["place"];
						long ownerId = long.Parse(text2.Remove(text2.IndexOf('_')).Remove(0, 4));
						return new Uri(NavigatorImpl.GetNavigateToPostCommentsNavStr(long.Parse(text2.Remove(0, text2.IndexOf('_') + 1)), ownerId, false, 0, 0, "") + "&ClearBackStack=true", UriKind.Relative);
					}
					if (paramDict.ContainsKey("group_id"))
					{
						long groupId = 0L;
						if (needHandleActivation && !paramDict.ContainsKey("type"))
						{
							StatsEventsTracker.Instance.Handle(new AppActivatedEvent
							{
								Reason = AppActivationReason.push,
								ReasonSubtype = "group"
							});
						}
						if (long.TryParse(paramDict["group_id"], out groupId))
						{
							return new Uri(NavigatorImpl.GetNavigateToGroupNavStr(groupId, "", false) + "&ClearBackStack=true", UriKind.Relative);
						}
					}
					if (paramDict.ContainsKey("uid"))
					{
						long uid = 0L;
						if (needHandleActivation && !paramDict.ContainsKey("type"))
						{
							StatsEventsTracker.Instance.Handle(new AppActivatedEvent
							{
								Reason = AppActivationReason.push,
								ReasonSubtype = "user"
							});
						}
						if (long.TryParse(paramDict["uid"], out uid))
						{
							return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(uid, "", false, "") + "&ClearBackStack=true", UriKind.Relative);
						}
					}
					if (paramDict.ContainsKey("from_id"))
					{
						long num2 = 0L;
						if (long.TryParse(paramDict["from_id"], out num2))
						{
							if (num2 < 0L)
							{
								if (needHandleActivation && !paramDict.ContainsKey("type"))
								{
									StatsEventsTracker.Instance.Handle(new AppActivatedEvent
									{
										Reason = AppActivationReason.push,
										ReasonSubtype = "group"
									});
								}
								return new Uri(NavigatorImpl.GetNavigateToGroupNavStr(-num2, "", false) + "&ClearBackStack=true", UriKind.Relative);
							}
							if (needHandleActivation && !paramDict.ContainsKey("type"))
							{
								StatsEventsTracker.Instance.Handle(new AppActivatedEvent
								{
									Reason = AppActivationReason.push,
									ReasonSubtype = "user"
								});
							}
							return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(num2, "", false, "") + "&ClearBackStack=true", UriKind.Relative);
						}
					}
					if (paramDict.ContainsKey("device_token") && paramDict.ContainsKey("url"))
					{
                        return new Uri(NavigatorImpl.GetWebViewPageNavStr(HttpUtility.UrlDecode(paramDict["url"]), false) + "&ClearBackStack=true", UriKind.Relative);
					}
					if (paramDict.ContainsKey("confirm_hash"))
					{
						Execute.ExecuteOnUIThread(delegate
						{
							MessageBoxResult messageBoxResult = MessageBox.Show(HttpUtility.UrlDecode(paramDict["confirm"]), CommonResources.VK, MessageBoxButton.OKCancel);
							Dictionary<string, string> expr_26 = new Dictionary<string, string>();
							expr_26.Add("confirm", (messageBoxResult == MessageBoxResult.OK) ? "1" : "0");
							expr_26.Add("hash", paramDict["confirm_hash"]);
							Dictionary<string, string> parameters = expr_26;
							Action<BackendResult<object, ResultCode>> arg_7C_0 = new Action<BackendResult<object, ResultCode>>((BackendResult<object, ResultCode> e)=>{});
							
							Action<BackendResult<object, ResultCode>> callback = arg_7C_0;
							VKRequestsDispatcher.DispatchRequestToVK<object>("account.validateAction", parameters, callback, null, false, true, default(CancellationToken?), null);
						});
					}
                    
					ShareOperation shareOperation = (Application.Current as IAppStateInfo).ShareOperation;
					if (shareOperation != null)
					{
						DataPackageView data = shareOperation.Data;
						if (data.Contains(StandardDataFormats.StorageItems) || data.Contains(StandardDataFormats.WebLink) || data.Contains(StandardDataFormats.Text))
						{
							ShareContentDataProviderManager.StoreDataProvider(new ShareExternalContentDataProvider(shareOperation));
							return new Uri(NavigatorImpl.GetShareExternalContentpageNavStr(), UriKind.Relative);
						}
					}
					if (originalString.Contains("ShareContent") && originalString.Contains("FileId"))
					{
						this.SetChoosenPhoto(HttpUtility.UrlDecode(paramDict["FileId"]));
						ParametersRepository.SetParameterForId("FromPhotoPicker", true);
						return new Uri(NavigatorImpl.GetNavToNewPostStr(0, false, 0, false, false, false) + "&ClearBackStack=true", UriKind.Relative);
					}
					if (originalString.Contains("Action=WallPost") && paramDict.ContainsKey("PostId") && paramDict.ContainsKey("OwnerId") && paramDict.ContainsKey("FocusComments"))
					{
						if (needHandleActivation && !paramDict.ContainsKey("type"))
						{
							StatsEventsTracker.Instance.Handle(new AppActivatedEvent
							{
								Reason = AppActivationReason.other_app,
								ReasonSubtype = "contacts"
							});
						}
						long arg_846_0 = long.Parse(paramDict["PostId"]);
						long ownerId2 = long.Parse(paramDict["OwnerId"]);
						bool focusCommentsField = bool.Parse(paramDict["FocusComments"]);
						long pollId = long.Parse(paramDict["PollId"]);
						long pollOwnerId = long.Parse(paramDict["PollOwnerId"]);
						return new Uri(NavigatorImpl.GetNavigateToPostCommentsNavStr(arg_846_0, ownerId2, focusCommentsField, pollId, pollOwnerId, "") + "&ClearBackStack=True", UriKind.Relative);
					}
					if (originalString.Contains("Action=ShowPhotos"))
					{
						if (needHandleActivation && !paramDict.ContainsKey("type"))
						{
							StatsEventsTracker.Instance.Handle(new AppActivatedEvent
							{
								Reason = AppActivationReason.other_app,
								ReasonSubtype = "contacts"
							});
						}
						return new Uri(originalString.Replace("/VKClient.Common;component/NewsPage.xaml", "/VKClient.Common;component/ImageViewerBasePage.xaml") + "&ClearBackStack=True", UriKind.Relative);
					}
				}
			}
			if (originalString.Contains("PeopleExtension"))
			{
				Dictionary<string, string> dictionary = uri.ParseQueryString();
				if (needHandleActivation)
				{
					StatsEventsTracker.Instance.Handle(new AppActivatedEvent
					{
						Reason = AppActivationReason.other_app,
						ReasonSubtype = "contacts"
					});
				}
				if (dictionary.ContainsKey("accountaction") && dictionary["accountaction"] == "manage")
				{
					return new Uri(NavigatorImpl.GetNavigateToSettingsStr() + "?ClearBackStack=true", UriKind.Relative);
				}
				if (dictionary.ContainsKey("action"))
				{
					string[] array = new string[0];
					if (dictionary.ContainsKey("contact_ids"))
					{
						array = dictionary["contact_ids"].FromURL().Split(new char[]
						{
							','
						});
					}
					string text3 = dictionary["action"];
					if (text3 == "Show_Contact")
					{
						Enumerable.FirstOrDefault<string>(array);
						long itemIdByRemoteId = RemoteIdHelper.GetItemIdByRemoteId(array[0]);
						string text4;
						if (itemIdByRemoteId > 0L)
						{
							text4 = NavigatorImpl.GetNavigateToUserProfileNavStr(itemIdByRemoteId, "", true, "");
						}
						else
						{
							text4 = NavigatorImpl.GetNavigateToGroupNavStr(-itemIdByRemoteId, "", true);
						}
						text4 += "&ClearBackStack=True";
						return new Uri(text4, UriKind.Relative);
					}
					if (text3 == "Post_Update")
					{
						return new Uri(NavigatorImpl.GetNavToNewPostStr(0, false, 0, false, false, false) + "&ClearBackStack=True", UriKind.Relative);
					}
				}
			}
			return uri;
		}

    private Uri MapProtocolLaunchUri(Uri uri)
    {
      string originalString = uri.OriginalString;
      Dictionary<string, string> parametersAsDict = QueryStringHelper.GetParametersAsDict(originalString);
      string str1 = "/VKClient.Common;component/NewsPage.xaml";
      string str2;
      if (parametersAsDict.ContainsKey("encodedLaunchUri"))
      {
        str2 = HttpUtility.UrlDecode(parametersAsDict["encodedLaunchUri"]);
        if (parametersAsDict.ContainsKey("sourceAppIdentifier"))
        {
          string str3 = parametersAsDict["sourceAppIdentifier"];
          if (str2.Contains("/?"))
            str2 = str2.Replace("/?", "?");
          string oldValue = "vkappconnect://authorize";
          if (!str2.StartsWith(oldValue))
          {
            if (str2.StartsWith("vk://"))
              return new Uri(NavigatorImpl.GetOpenUrlPageStr(str2.Replace("vk://", "https://vk.com/")), UriKind.Relative);
            MessageBox.Show("Unsupported protocol: " + str2);
            str2 = str1;
          }
          else if (str2.Contains("RedirectUri=vkc"))
          {
            MessageBox.Show("Unsupported redirect uri. Please, use the latest version of WP SDK.");
            str2 = str1;
          }
          else
            str2 = str2.Replace(oldValue, "/VKClient.Common;component/SDKAuthPage.xaml") + "&SDKGUID=" + str3.ToLowerInvariant();
        }
        else if (str2.StartsWith("fb128749580520227://authorize"))
        {
          if (((UriMapperBase) new FacebookUriMapper()).MapUri(uri).OriginalString.StartsWith("/SuccessfulFacebookLogin.xaml"))
            return new Uri("/VKClient.Common;component/FriendsImportFacebookPage.xaml", UriKind.Relative);
        }
        else if (str2.StartsWith("com.vk.vkclient:/gmail-oauth/code"))
        {
          Dictionary<string, string> queryString = CustomUriMapper.ParseQueryString(str2);
          if (queryString != null && queryString.ContainsKey("code"))
          {
            string str3 = queryString["code"];
            if (!string.IsNullOrEmpty(str3))
              return new Uri(string.Format("/VKClient.Common;component/FriendsImportGmailPage.xaml?code={0}", str3), UriKind.Relative);
          }
        }
        else if (str2.StartsWith("com.vk.vkclient://twitter-oauth/callback"))
        {
          Dictionary<string, string> queryString = CustomUriMapper.ParseQueryString(str2);
          if (queryString != null && queryString.ContainsKey("oauth_token") && queryString.ContainsKey("oauth_verifier"))
          {
            string str3 = queryString["oauth_token"];
            string str4 = queryString["oauth_verifier"];
            if (!string.IsNullOrEmpty(str3) && !string.IsNullOrEmpty(str4))
              return new Uri(string.Format("/VKClient.Common;component/FriendsImportTwitterPage.xaml?oauthToken={0}&oauthVerifier={1}", str3, str4), UriKind.Relative);
          }
        }
      }
      else
      {
        MessageBox.Show("Unable to identify source app or uri launch from uri " + originalString);
        str2 = str1;
      }
      return new Uri(str2, UriKind.Relative);
    }

    private static Dictionary<string, string> ParseQueryString(string uri)
		{
			IEnumerable<string> arg_4D_0 = uri.Substring((uri.LastIndexOf('?') == -1) ? 0 : (uri.LastIndexOf('?') + 1)).Split(new char[]
			{
				'&'
			});
			Func<string, string[]> arg_4D_1 = new Func<string, string[]>((string piece)=>
			{
				return piece.Split(new char[]
				{
					'='
				});
			});
			
			IEnumerable<string[]> arg_90_0 = Enumerable.Select<string, string[]>(arg_4D_0, arg_4D_1);
			Func<string[], string> arg_90_1 = new Func<string[], string>((string[] pair)=>
			{
				return pair[0];
			});
			
			Func<string[], string> arg_90_2 = new Func<string[], string>((string[] pair)=>
			{
				return pair[1];
			});
			
			return Enumerable.ToDictionary<string[], string, string>(arg_90_0, arg_90_1, arg_90_2);
		}

    private void SetChoosenPhoto(string fileId)
    {
      using (MediaLibrary mediaLibrary = new MediaLibrary())
      {
        using (Picture pictureFromToken = mediaLibrary.GetPictureFromToken(fileId))
        {
          ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", new List<Stream>()
          {
            pictureFromToken.GetThumbnail()
          });
          ParametersRepository.SetParameterForId("ChoosenPhotos", new List<Stream>()
          {
            pictureFromToken.GetImage()
          });
        }
      }
    }
  }
}
