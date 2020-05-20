using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.Library
{
  public class StatsEventsTracker : IHandle<OpenUserEvent>, IHandle, IHandle<OpenGroupEvent>, IHandle<ViewPostEvent>, IHandle<ViewBlockEvent>, IHandle<OpenPostEvent>, IHandle<OpenVideoEvent>, IHandle<VideoPlayEvent>, IHandle<MenuClickEvent>, IHandle<TransitionFromPostEvent>, IHandle<SubscriptionFromPostEvent>, IHandle<HyperlinkClickedEvent>, IHandle<OpenGamesEvent>, IHandle<GamesActionEvent>, IHandle<MarketItemActionEvent>, IHandle<FriendRecommendationShowedEvent>, IHandle<AppActivatedEvent>, IHandle<AdImpressionEvent>, IHandle<AdPixelEvent>, IHandle<ProfileBlockClickEvent>, IHandle<DiscoverActionEvent>, IHandle<MarketContactEvent>, IHandle<BalanceTopupEvent>, IHandle<StickersPurchaseFunnelEvent>, IHandle<GifPlayEvent>, IHandle<PostActionEvent>, IHandle<AudioMessagePlayEvent>, IHandle<PostInteractionEvent>, IHandle<GiftsPurchaseStepsEvent>
  {
    private static readonly int SEND_THRESHOLD = 10;
    private List<object> _pendingEvents = new List<object>();
    private List<Tuple<DateTime, AdImpressionEvent>> _handledAdImpressionEvents = new List<Tuple<DateTime, AdImpressionEvent>>();
    private Dictionary<string, string> _loadedUris = new Dictionary<string, string>();
    private static StatsEventsTracker _instance;

    public static StatsEventsTracker Instance
    {
      get
      {
        if (StatsEventsTracker._instance == null)
          StatsEventsTracker._instance = new StatsEventsTracker();
        return StatsEventsTracker._instance;
      }
    }

    public List<object> PendingEvents
    {
      get
      {
        return this._pendingEvents;
      }
      set
      {
        this._pendingEvents = value ?? new List<object>();
      }
    }

    public StatsEventsTracker()
    {
      EventAggregator.Current.Subscribe(this);
    }

    private void HandleEvent(object message, bool forceNow, bool preventProcessing = false)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        Logger.Instance.Info("StatsEventsTracker.HandleEvent: " + message.ToString());
        this._pendingEvents.Add(message);
        if (((this._pendingEvents.Count < StatsEventsTracker.SEND_THRESHOLD ? 0 : (!preventProcessing ? 1 : 0)) | (forceNow ? 1 : 0)) == 0 && !AppGlobalStateManager.Current.GlobalState.ForceStatsSend)
          return;
        this.SendEvents();
      }));
    }

    private void SendEvents()
    {
      List<object> inProgressEvents = new List<object>();
      this._pendingEvents = this._pendingEvents.Where<object>((Func<object, bool>) (pe => (pe as StatEventBase).ShouldSendImmediately)).Concat<object>(this._pendingEvents.Where<object>((Func<object, bool>) (pe => !(pe as StatEventBase).ShouldSendImmediately))).ToList<object>();
      List<AppEventBase> appEvents = StatsEventsTracker.ConvertToAppEvents((IEnumerable<object>) this._pendingEvents);
      inProgressEvents.AddRange((IEnumerable<object>) this._pendingEvents);
      this._pendingEvents.Clear();
      if (appEvents.Count <= 0)
        return;
      AccountService.Instance.StatsTrackEvents(appEvents, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          return;
        Execute.ExecuteOnUIThread((Action) (() => this._pendingEvents.AddRange((IEnumerable<object>) inProgressEvents)));
      }));
    }

    public static List<AppEventBase> ConvertToAppEvents(IEnumerable<object> enumerable)
    {
      List<AppEventBase> appEventBaseList = new List<AppEventBase>();
      AppEventOpenUser appEventOpenUser1 = enumerable.OfType<OpenUserEvent>().Where<OpenUserEvent>((Func<OpenUserEvent, bool>) (e => string.IsNullOrEmpty(e.Source))).Select<OpenUserEvent, AppEventOpenUser>((Func<OpenUserEvent, AppEventOpenUser>) (oue => new AppEventOpenUser()
      {
        user_ids = new List<long>() { oue.UserId }
      })).Aggregate<AppEventOpenUser, AppEventOpenUser>(new AppEventOpenUser()
      {
        user_ids = new List<long>()
      }, (Func<AppEventOpenUser, AppEventOpenUser, AppEventOpenUser>) ((appEvent1, appEvent2) => new AppEventOpenUser()
      {
        user_ids = appEvent1.user_ids.Concat<long>((IEnumerable<long>) appEvent2.user_ids).ToList<long>()
      }));
      if (appEventOpenUser1.user_ids.Count > 0)
      {
        appEventOpenUser1.user_ids = appEventOpenUser1.user_ids.Distinct<long>().ToList<long>();
        appEventBaseList.Add((AppEventBase) appEventOpenUser1);
      }
      AppEventOpenUser appEventOpenUser2 = enumerable.OfType<OpenUserEvent>().Where<OpenUserEvent>((Func<OpenUserEvent, bool>) (e => !string.IsNullOrEmpty(e.Source))).Select<OpenUserEvent, AppEventOpenUser>((Func<OpenUserEvent, AppEventOpenUser>) (oue =>
      {
        AppEventOpenUser appEventOpenUser3 = new AppEventOpenUser();
        appEventOpenUser3.user_ids = new List<long>()
        {
          oue.UserId
        };
        string source = oue.Source;
        appEventOpenUser3.source = source;
        return appEventOpenUser3;
      })).Aggregate<AppEventOpenUser, AppEventOpenUser>(new AppEventOpenUser()
      {
        user_ids = new List<long>()
      }, (Func<AppEventOpenUser, AppEventOpenUser, AppEventOpenUser>) ((appEvent1, appEvent2) => new AppEventOpenUser()
      {
        user_ids = appEvent1.user_ids.Concat<long>((IEnumerable<long>) appEvent2.user_ids).ToList<long>(),
        source = appEvent2.source
      }));
      if (appEventOpenUser2.user_ids.Count > 0)
      {
        appEventOpenUser2.user_ids = appEventOpenUser2.user_ids.Distinct<long>().ToList<long>();
        appEventBaseList.Add((AppEventBase) appEventOpenUser2);
      }
      foreach (string str in enumerable.OfType<OpenGroupEvent>().Select<OpenGroupEvent, string>((Func<OpenGroupEvent, string>) (e => e.Source)).Distinct<string>())
      {
        string source = str;
        AppEventOpenGroup appEventOpenGroup1 = new AppEventOpenGroup()
        {
          group_ids = new List<long>()
        };
        foreach (OpenGroupEvent openGroupEvent in enumerable.OfType<OpenGroupEvent>().Where<OpenGroupEvent>((Func<OpenGroupEvent, bool>) (e => e.Source == source)))
        {
          AppEventOpenGroup appEventOpenGroup2 = new AppEventOpenGroup();
          appEventOpenGroup2.group_ids = new List<long>()
          {
            openGroupEvent.GroupId
          };
          string source1 = openGroupEvent.Source;
          appEventOpenGroup2.source = source1;
          AppEventOpenGroup appEventOpenGroup3 = appEventOpenGroup2;
          appEventOpenGroup1 = new AppEventOpenGroup()
          {
            group_ids = appEventOpenGroup1.group_ids.Concat<long>((IEnumerable<long>) appEventOpenGroup3.group_ids).ToList<long>(),
            source = appEventOpenGroup3.source
          };
        }
        if (appEventOpenGroup1.group_ids.Count > 0)
        {
          appEventOpenGroup1.group_ids = appEventOpenGroup1.group_ids.Distinct<long>().ToList<long>();
          appEventBaseList.Add((AppEventBase) appEventOpenGroup1);
        }
      }
      AppEventFriendRecommendationShowed recommendationShowed = enumerable.OfType<FriendRecommendationShowedEvent>().Select<FriendRecommendationShowedEvent, AppEventFriendRecommendationShowed>((Func<FriendRecommendationShowedEvent, AppEventFriendRecommendationShowed>) (oue => new AppEventFriendRecommendationShowed()
      {
        user_ids = new List<long>() { oue.UserId }
      })).Aggregate<AppEventFriendRecommendationShowed, AppEventFriendRecommendationShowed>(new AppEventFriendRecommendationShowed()
      {
        user_ids = new List<long>()
      }, (Func<AppEventFriendRecommendationShowed, AppEventFriendRecommendationShowed, AppEventFriendRecommendationShowed>) ((appEvent1, appEvent2) => new AppEventFriendRecommendationShowed()
      {
        user_ids = appEvent1.user_ids.Concat<long>((IEnumerable<long>) appEvent2.user_ids).ToList<long>()
      }));
      if (recommendationShowed.user_ids.Any<long>())
      {
        recommendationShowed.user_ids = recommendationShowed.user_ids.Distinct<long>().ToList<long>();
        appEventBaseList.Add((AppEventBase) recommendationShowed);
      }
      IEnumerable<AppEventAppActivated> eventAppActivateds = enumerable.OfType<AppActivatedEvent>().Select<AppActivatedEvent, AppEventAppActivated>((Func<AppActivatedEvent, AppEventAppActivated>) (mce => new AppEventAppActivated()
      {
        @ref = mce.Reason.ToString("g"),
        type = mce.ReasonSubtype
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventAppActivateds);
      List<string> list1 = enumerable.OfType<ViewPostEvent>().Select<ViewPostEvent, string>((Func<ViewPostEvent, string>) (vpe => vpe.PostIdExtended)).ToList<string>();
      List<string> list2 = enumerable.OfType<ViewPostEvent>().SelectMany<ViewPostEvent, string>((Func<ViewPostEvent, IEnumerable<string>>) (vpe => (IEnumerable<string>) vpe.CopyPostIds)).ToList<string>();
      if (list1.Count > 0)
      {
        AppEventViewPost appEventViewPost = new AppEventViewPost()
        {
          post_ids = list1,
          repost_ids = list2
        };
        appEventBaseList.Add((AppEventBase) appEventViewPost);
      }
      List<string> list3 = enumerable.OfType<ViewBlockEvent>().Select<ViewBlockEvent, string>((Func<ViewBlockEvent, string>) (e => e.ItemType + "|" + e.Position)).ToList<string>();
      if (list3.Count > 0)
      {
        AppEventViewBlock appEventViewBlock = new AppEventViewBlock()
        {
          blocks = list3
        };
        appEventBaseList.Add((AppEventBase) appEventViewBlock);
      }
      List<string> list4 = enumerable.OfType<OpenPostEvent>().Select<OpenPostEvent, string>((Func<OpenPostEvent, string>) (vpe => vpe.PostId)).ToList<string>();
      List<string> list5 = enumerable.OfType<OpenPostEvent>().SelectMany<OpenPostEvent, string>((Func<OpenPostEvent, IEnumerable<string>>) (vpe => (IEnumerable<string>) vpe.CopyPostIds)).ToList<string>();
      if (list4.Count > 0)
      {
        AppEventOpenPost appEventOpenPost = new AppEventOpenPost()
        {
          post_ids = list4,
          repost_ids = list5
        };
        appEventBaseList.Add((AppEventBase) appEventOpenPost);
      }
      List<string> list6 = enumerable.OfType<TransitionFromPostEvent>().Select<TransitionFromPostEvent, string>((Func<TransitionFromPostEvent, string>) (tps => tps.post_id)).ToList<string>();
      IEnumerable<TransitionFromPostEvent> source2 = enumerable.OfType<TransitionFromPostEvent>();
      Func<TransitionFromPostEvent, string> func1 = (Func<TransitionFromPostEvent, string>) (tps => tps.post_id);
      Dictionary<string, string> dictionary = source2.ToDictionary<TransitionFromPostEvent, string, string>(func1, (Func<TransitionFromPostEvent, string>)(tps => tps.parent_id));
      if (list6.Count > 0)
      {
        AppEventTransitionFromPost transitionFromPost = new AppEventTransitionFromPost()
        {
          post_ids = list6,
          parent_ids = dictionary
        };
        appEventBaseList.Add((AppEventBase) transitionFromPost);
      }
      List<string> list7 = enumerable.OfType<SubscriptionFromPostEvent>().Select<SubscriptionFromPostEvent, string>((Func<SubscriptionFromPostEvent, string>) (sfp => sfp.post_id)).ToList<string>();
      if (list7.Count > 0)
      {
        AppEventSubscriptionFromPost subscriptionFromPost = new AppEventSubscriptionFromPost()
        {
          post_ids = list7
        };
        appEventBaseList.Add((AppEventBase) subscriptionFromPost);
      }
      List<string> list8 = enumerable.OfType<HyperlinkClickedEvent>().Select<HyperlinkClickedEvent, string>((Func<HyperlinkClickedEvent, string>) (hce => hce.HyperlinkOwnerId)).ToList<string>();
      if (list8.Count > 0)
      {
        AppEventPostLinkClick eventPostLinkClick = new AppEventPostLinkClick()
        {
          post_ids = list8
        };
        appEventBaseList.Add((AppEventBase) eventPostLinkClick);
      }
      IEnumerable<AppEventOpenVideo> appEventOpenVideos = enumerable.OfType<OpenVideoEvent>().Select<OpenVideoEvent, AppEventOpenVideo>((Func<OpenVideoEvent, AppEventOpenVideo>) (ove => new AppEventOpenVideo()
      {
        video_id = ove.id,
        source = ove.Source.ToString(),
        context = ove.context
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) appEventOpenVideos);
      IEnumerable<AppEventVideoPlay> source3 = enumerable.OfType<VideoPlayEvent>().Select<VideoPlayEvent, AppEventVideoPlay>((Func<VideoPlayEvent, AppEventVideoPlay>) (vpe => new AppEventVideoPlay()
      {
        video_id = vpe.id,
        position = vpe.Position.ToString(),
        source = vpe.Source.ToString(),
        quality = vpe.quality,
        context = vpe.Context
      }));
      //appEventBaseList.AddRange((IEnumerable<AppEventBase>) source3.Distinct<AppEventVideoPlay, string>((Func<AppEventVideoPlay, string>) (e => e.video_id)));//todo: bug
      IEnumerable<AppEventAudioPlay> appEventAudioPlays = enumerable.OfType<AudioPlayEvent>().Select<AudioPlayEvent, AppEventAudioPlay>((Func<AudioPlayEvent, AppEventAudioPlay>) (ape => new AppEventAudioPlay()
      {
        audio_id = ape.OwnerAndAudioId,
        source = ape.Source.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) appEventAudioPlays);
      IEnumerable<AppEventMenuClick> appEventMenuClicks = enumerable.OfType<MenuClickEvent>().Select<MenuClickEvent, AppEventMenuClick>((Func<MenuClickEvent, AppEventMenuClick>) (mce => new AppEventMenuClick()
      {
        item = mce.item
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) appEventMenuClicks);
      IEnumerable<AppEventGamesVisit> appEventGamesVisits = enumerable.OfType<OpenGamesEvent>().Select<OpenGamesEvent, AppEventGamesVisit>((Func<OpenGamesEvent, AppEventGamesVisit>) (e => new AppEventGamesVisit()
      {
        visit_source = e.visit_source.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) appEventGamesVisits);
      IEnumerable<AppEventGamesAction> eventGamesActions = enumerable.OfType<GamesActionEvent>().Select<GamesActionEvent, AppEventGamesAction>((Func<GamesActionEvent, AppEventGamesAction>) (e => new AppEventGamesAction()
      {
        game_id = e.game_id,
        request_name = e.request_name,
        action_type = e.action_type.ToString(),
        visit_source = e.visit_source.ToString(),
        click_source = e.click_source.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventGamesActions);
      IEnumerable<IGrouping<MarketItemSource, MarketItemActionEvent>> source4 = enumerable.OfType<MarketItemActionEvent>().GroupBy<MarketItemActionEvent, MarketItemSource>((Func<MarketItemActionEvent, MarketItemSource>) (item => item.source));
      Func<IGrouping<MarketItemSource, MarketItemActionEvent>, MarketItemSource> func2 = (Func<IGrouping<MarketItemSource, MarketItemActionEvent>, MarketItemSource>) (item => item.Key);
      IEnumerable<AppEventMarketItemAction> marketItemActions = source4.ToDictionary<IGrouping<MarketItemSource, MarketItemActionEvent>, MarketItemSource, List<string>>(func2, (Func<IGrouping<MarketItemSource, MarketItemActionEvent>, List<string>>)(item => item.Select<MarketItemActionEvent, string>((Func<MarketItemActionEvent, string>)(x => x.itemId)).ToList<string>())).Select<KeyValuePair<MarketItemSource, List<string>>, AppEventMarketItemAction>((Func<KeyValuePair<MarketItemSource, List<string>>, AppEventMarketItemAction>)(item => new AppEventMarketItemAction()
      {
        source = item.Key.ToString(),
        item_ids = item.Value
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) marketItemActions);
      List<AppEventProfileBlockClick> source5 = new List<AppEventProfileBlockClick>();
      foreach (ProfileBlockClickEvent profileBlockClickEvent in enumerable.OfType<ProfileBlockClickEvent>())
      {
        ProfileBlockClickEvent item = profileBlockClickEvent;
        AppEventProfileBlockClick profileBlockClick = source5.FirstOrDefault<AppEventProfileBlockClick>((Func<AppEventProfileBlockClick, bool>) (i => i.user_id == item.UserId));
        if (profileBlockClick == null)
        {
          profileBlockClick = new AppEventProfileBlockClick()
          {
            user_id = item.UserId,
            blocks = new ProfileBlocksClickData()
          };
          source5.Add(profileBlockClick);
        }
        ProfileBlocksClickData blocks = profileBlockClick.blocks;
        switch (item.BlockType)
        {
          case ProfileBlockType.friends:
            ProfileBlocksClickData profileBlocksClickData1 = blocks;
            ProfileBlocksClickData profileBlocksClickData2 = blocks;
            int? friends = profileBlocksClickData2.friends;
            int? nullable1 = friends.HasValue ? new int?(friends.GetValueOrDefault() + 1) : new int?();
            int? nullable2 = nullable1;
            profileBlocksClickData2.friends = nullable2;
            int? nullable3 = new int?(nullable1 ?? 1);
            profileBlocksClickData1.friends = nullable3;
            continue;
          case ProfileBlockType.followers:
            ProfileBlocksClickData profileBlocksClickData3 = blocks;
            ProfileBlocksClickData profileBlocksClickData4 = blocks;
            int? followers = profileBlocksClickData4.followers;
            int? nullable4 = followers.HasValue ? new int?(followers.GetValueOrDefault() + 1) : new int?();
            int? nullable5 = nullable4;
            profileBlocksClickData4.followers = nullable5;
            int? nullable6 = new int?(nullable4 ?? 1);
            profileBlocksClickData3.followers = nullable6;
            continue;
          case ProfileBlockType.photos:
            ProfileBlocksClickData profileBlocksClickData5 = blocks;
            ProfileBlocksClickData profileBlocksClickData6 = blocks;
            int? photos = profileBlocksClickData6.photos;
            int? nullable7 = photos.HasValue ? new int?(photos.GetValueOrDefault() + 1) : new int?();
            int? nullable8 = nullable7;
            profileBlocksClickData6.photos = nullable8;
            int? nullable9 = new int?(nullable7 ?? 1);
            profileBlocksClickData5.photos = nullable9;
            continue;
          case ProfileBlockType.videos:
            ProfileBlocksClickData profileBlocksClickData7 = blocks;
            ProfileBlocksClickData profileBlocksClickData8 = blocks;
            int? videos = profileBlocksClickData8.videos;
            int? nullable10 = videos.HasValue ? new int?(videos.GetValueOrDefault() + 1) : new int?();
            int? nullable11 = nullable10;
            profileBlocksClickData8.videos = nullable11;
            int? nullable12 = new int?(nullable10 ?? 1);
            profileBlocksClickData7.videos = nullable12;
            continue;
          case ProfileBlockType.audios:
            ProfileBlocksClickData profileBlocksClickData9 = blocks;
            ProfileBlocksClickData profileBlocksClickData10 = blocks;
            int? audios = profileBlocksClickData10.audios;
            int? nullable13 = audios.HasValue ? new int?(audios.GetValueOrDefault() + 1) : new int?();
            int? nullable14 = nullable13;
            profileBlocksClickData10.audios = nullable14;
            int? nullable15 = new int?(nullable13 ?? 1);
            profileBlocksClickData9.audios = nullable15;
            continue;
          case ProfileBlockType.gifts:
            ProfileBlocksClickData profileBlocksClickData11 = blocks;
            ProfileBlocksClickData profileBlocksClickData12 = blocks;
            int? gifts = profileBlocksClickData12.gifts;
            int? nullable16 = gifts.HasValue ? new int?(gifts.GetValueOrDefault() + 1) : new int?();
            int? nullable17 = nullable16;
            profileBlocksClickData12.gifts = nullable17;
            int? nullable18 = new int?(nullable16 ?? 1);
            profileBlocksClickData11.gifts = nullable18;
            continue;
          case ProfileBlockType.docs:
            ProfileBlocksClickData profileBlocksClickData13 = blocks;
            ProfileBlocksClickData profileBlocksClickData14 = blocks;
            int? docs = profileBlocksClickData14.docs;
            int? nullable19 = docs.HasValue ? new int?(docs.GetValueOrDefault() + 1) : new int?();
            int? nullable20 = nullable19;
            profileBlocksClickData14.docs = nullable20;
            int? nullable21 = new int?(nullable19 ?? 1);
            profileBlocksClickData13.docs = nullable21;
            continue;
          case ProfileBlockType.subscriptions:
            ProfileBlocksClickData profileBlocksClickData15 = blocks;
            ProfileBlocksClickData profileBlocksClickData16 = blocks;
            int? subscriptions = profileBlocksClickData16.subscriptions;
            int? nullable22 = subscriptions.HasValue ? new int?(subscriptions.GetValueOrDefault() + 1) : new int?();
            int? nullable23 = nullable22;
            profileBlocksClickData16.subscriptions = nullable23;
            int? nullable24 = new int?(nullable22 ?? 1);
            profileBlocksClickData15.subscriptions = nullable24;
            continue;
          default:
            continue;
        }
      }
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) source5);
      IEnumerable<AppEventDiscoverAction> eventDiscoverActions = enumerable.OfType<DiscoverActionEvent>().Select<DiscoverActionEvent, AppEventDiscoverAction>((Func<DiscoverActionEvent, AppEventDiscoverAction>) (e => new AppEventDiscoverAction()
      {
        action_type = e.ActionType.ToString(),
        action_param = e.ActionParam
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventDiscoverActions);
      IEnumerable<AppEventAdImpression> eventAdImpressions = enumerable.OfType<AdImpressionEvent>().Select<AdImpressionEvent, AppEventAdImpression>((Func<AdImpressionEvent, AppEventAdImpression>) (e => new AppEventAdImpression()
      {
        ad_data_impression = e.AdDataImpression
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventAdImpressions);
      IEnumerable<AppEventMarketContact> eventMarketContacts = enumerable.OfType<MarketContactEvent>().Select<MarketContactEvent, AppEventMarketContact>((Func<MarketContactEvent, AppEventMarketContact>) (e => new AppEventMarketContact()
      {
        item_id = e.ItemId,
        action = e.Action.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventMarketContacts);
      IEnumerable<AppEventBalanceTopup> eventBalanceTopups = enumerable.OfType<BalanceTopupEvent>().Select<BalanceTopupEvent, AppEventBalanceTopup>((Func<BalanceTopupEvent, AppEventBalanceTopup>) (e => new AppEventBalanceTopup()
      {
        source = e.Source.ToString(),
        action = e.Action.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) eventBalanceTopups);
      IEnumerable<AppEventStickersPurchaseFunnel> stickersPurchaseFunnels = enumerable.OfType<StickersPurchaseFunnelEvent>().Select<StickersPurchaseFunnelEvent, AppEventStickersPurchaseFunnel>((Func<StickersPurchaseFunnelEvent, AppEventStickersPurchaseFunnel>) (e => new AppEventStickersPurchaseFunnel()
      {
        source = e.Source.ToString(),
        action = e.Action.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) stickersPurchaseFunnels);
      IEnumerable<AppEventGifPlay> appEventGifPlays = enumerable.OfType<GifPlayEvent>().Select<GifPlayEvent, AppEventGifPlay>((Func<GifPlayEvent, AppEventGifPlay>) (e => new AppEventGifPlay()
      {
        gif_id = e.GifId,
        start_type = e.StartType.ToString(),
        source = e.Source.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) appEventGifPlays);
      AppEventPostAction appEventPostAction = new AppEventPostAction();
      IEnumerable<PostActionEvent> source6 = enumerable.OfType<PostActionEvent>();
      appEventPostAction.expand = source6.Where<PostActionEvent>((Func<PostActionEvent, bool>) (e => e.ActionType == PostActionType.Expanded)).Select<PostActionEvent, string>((Func<PostActionEvent, string>) (e => e.PostId)).ToArray<string>();
      appEventPostAction.photo = source6.Where<PostActionEvent>((Func<PostActionEvent, bool>) (e => e.ActionType == PostActionType.PhotoOpened)).Select<PostActionEvent, string>((Func<PostActionEvent, string>) (e => e.PostId)).ToArray<string>();
      appEventPostAction.video = source6.Where<PostActionEvent>((Func<PostActionEvent, bool>) (e => e.ActionType == PostActionType.VideoOpened)).Select<PostActionEvent, string>((Func<PostActionEvent, string>) (e => e.PostId)).ToArray<string>();
      appEventPostAction.audio = source6.Where<PostActionEvent>((Func<PostActionEvent, bool>) (e => e.ActionType == PostActionType.AudioOpened)).Select<PostActionEvent, string>((Func<PostActionEvent, string>) (e => e.PostId)).ToArray<string>();
      appEventBaseList.Add((AppEventBase) appEventPostAction);
      IEnumerable<AppEventAudioMessagePlay> audioMessagePlays = enumerable.OfType<AudioMessagePlayEvent>().Select<AudioMessagePlayEvent, AppEventAudioMessagePlay>((Func<AudioMessagePlayEvent, AppEventAudioMessagePlay>) (e => new AppEventAudioMessagePlay()
      {
        audio_message_id = e.AudioMessageId
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) audioMessagePlays);
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) enumerable.OfType<PostInteractionEvent>().Where<PostInteractionEvent>((Func<PostInteractionEvent, bool>) (e => !string.IsNullOrEmpty(e.PostId))).Select<PostInteractionEvent, AppEventPostInteraction>((Func<PostInteractionEvent, AppEventPostInteraction>) (e => new AppEventPostInteraction()
      {
        post_id = e.PostId,
        action = e.Action.ToString("G"),
        link = e.Link
      })));
      IEnumerable<AppEventGiftsPurchaseSteps> giftsPurchaseStepses = enumerable.OfType<GiftsPurchaseStepsEvent>().Select<GiftsPurchaseStepsEvent, AppEventGiftsPurchaseSteps>((Func<GiftsPurchaseStepsEvent, AppEventGiftsPurchaseSteps>) (e => new AppEventGiftsPurchaseSteps()
      {
        source = e.Source.ToString(),
        action = e.Action.ToString()
      }));
      appEventBaseList.AddRange((IEnumerable<AppEventBase>) giftsPurchaseStepses);
      return appEventBaseList;
    }

    public void Handle(OpenUserEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(OpenGroupEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(ViewPostEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(ViewBlockEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(OpenPostEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(OpenVideoEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(VideoPlayEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(MenuClickEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(TransitionFromPostEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(SubscriptionFromPostEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(HyperlinkClickedEvent message)
    {
      if (string.IsNullOrEmpty(message.HyperlinkOwnerId))
        return;
      this.HandleEvent(message, false, true);
    }

    public void Handle(OpenGamesEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(GamesActionEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(MarketItemActionEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(FriendRecommendationShowedEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(AppActivatedEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(AdImpressionEvent message)
    {
      if (this._handledAdImpressionEvents.Any<Tuple<DateTime, AdImpressionEvent>>((Func<Tuple<DateTime, AdImpressionEvent>, bool>) (t =>
      {
        if ((DateTime.Now - t.Item1).TotalMinutes <= 5.0)
          return t.Item2.AdDataImpression == message.AdDataImpression;
        return false;
      })))
        return;
      this.HandleEvent((StatEventBase) message);
      this._handledAdImpressionEvents.Add(new Tuple<DateTime, AdImpressionEvent>(DateTime.Now, message));
    }

    public void Handle(AdPixelEvent message)
    {
      this.LoadUris(message.UrlToLoad);
    }

    private void LoadUris(List<string> list)
    {
      foreach (string str in list)
      {
        string uri = str;
        if (!this._loadedUris.ContainsKey(uri))
          JsonWebRequest.Download(uri, 0, 0, (Action<HttpStatusCode, long, byte[]>) ((statusCode, b, c) =>
          {
            if (statusCode != HttpStatusCode.OK)
              return;
            Execute.ExecuteOnUIThread((Action) (() => this._loadedUris[uri] = "true"));
          }));
      }
    }

    private void HandleEvent(StatEventBase eventBase)
    {
      this.HandleEvent(eventBase, eventBase.ShouldSendImmediately, false);
    }

    public void Handle(ProfileBlockClickEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(DiscoverActionEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(MarketContactEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(BalanceTopupEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(StickersPurchaseFunnelEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(GifPlayEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(PostActionEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(AudioMessagePlayEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(PostInteractionEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }

    public void Handle(GiftsPurchaseStepsEvent message)
    {
      this.HandleEvent((StatEventBase) message);
    }
  }
}
