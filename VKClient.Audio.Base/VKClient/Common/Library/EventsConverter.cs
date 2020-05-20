using System.Collections.Generic;
using VKClient.Audio.Base.Events;

namespace VKClient.Common.Library
{
  public static class EventsConverter
  {
      /*
    public static List<object> ConvertToPendingEvents(List<PendingStatisticsEvent> storedEvents)
    {
      List<object> objectList = new List<object>();
      foreach (PendingStatisticsEvent storedEvent in storedEvents)
      {
        string eventName = storedEvent.event_name;
        // ISSUE: reference to a compiler-generated method
        uint stringHash = <PrivateImplementationDetails>.ComputeStringHash(eventName);
        if (stringHash <= 2202212194U)
        {
          if (stringHash <= 687097591U)
          {
            if (stringHash <= 401341008U)
            {
              if ((int) stringHash != 151622979)
              {
                if ((int) stringHash != 329214221)
                {
                  if ((int) stringHash == 401341008 && eventName == "TransitionFromPostEvent")
                    objectList.Add(new TransitionFromPostEvent()
                    {
                      post_id = storedEvent.post_id,
                      parent_id = storedEvent.parent_id
                    });
                }
                else if (eventName == "AudioPlayEvent")
                  objectList.Add(new AudioPlayEvent()
                  {
                    OwnerAndAudioId = storedEvent.audio_id,
                    Source = storedEvent.source
                  });
              }
              else if (eventName == "ViewBlockEvent")
                objectList.Add(new ViewBlockEvent()
                {
                  ItemType = storedEvent.item_type,
                  Position = storedEvent.item_position
                });
            }
            else if ((int) stringHash != 586088214)
            {
              if ((int) stringHash != 627656064)
              {
                if ((int) stringHash == 687097591 && eventName == "ProfileBlockClickEvent")
                  objectList.Add(new ProfileBlockClickEvent()
                  {
                    UserId = storedEvent.user_id,
                    BlockType = storedEvent.profile_block_type
                  });
              }
              else if (eventName == "OpenVideoEvent")
                objectList.Add(new OpenVideoEvent()
                {
                  id = storedEvent.video_id,
                  Source = storedEvent.source,
                  context = storedEvent.video_context
                });
            }
            else if (eventName == "MenuClickEvent")
              objectList.Add(new MenuClickEvent()
              {
                item = storedEvent.item
              });
          }
          else if (stringHash <= 1691564853U)
          {
            if ((int) stringHash != 904108569)
            {
              if ((int) stringHash != 1644786535)
              {
                if ((int) stringHash == 1691564853 && eventName == "MarketContactEvent")
                  objectList.Add(new MarketContactEvent(storedEvent.market_item_id, storedEvent.MarketContactAction));
              }
              else if (eventName == "PostLinkClickEvent")
                objectList.Add(new HyperlinkClickedEvent()
                {
                  HyperlinkOwnerId = storedEvent.post_id
                });
            }
            else if (eventName == "OpenPostEvent")
              objectList.Add(new OpenPostEvent()
              {
                PostId = storedEvent.post_id,
                CopyPostIds = storedEvent.repost_ids
              });
          }
          else if (stringHash <= 2052296550U)
          {
            if ((int) stringHash != 1705416732)
            {
              if ((int) stringHash == 2052296550 && eventName == "GiftsPurchaseStepsEvent")
                objectList.Add(new GiftsPurchaseStepsEvent(storedEvent.GiftPurchaseStepsSource, storedEvent.GiftPurchaseStepsAction));
            }
            else if (eventName == "DiscoverActionEvent")
              objectList.Add(new DiscoverActionEvent()
              {
                ActionType = storedEvent.discover_action_type,
                ActionParam = storedEvent.discover_action_param
              });
          }
          else if ((int) stringHash != 2143617917)
          {
            if ((int) stringHash == -2092755102 && eventName == "VideoPlayEvent")
              objectList.Add(new VideoPlayEvent()
              {
                id = storedEvent.video_id,
                Position = storedEvent.position,
                quality = storedEvent.quality,
                Source = storedEvent.source,
                Context = storedEvent.video_context
              });
          }
          else if (eventName == "BalanceTopupEvent")
            objectList.Add(new BalanceTopupEvent(storedEvent.BalanceTopupSource, storedEvent.BalanceTopupAction));
        }
        else if (stringHash <= 3536281301U)
        {
          if (stringHash <= 3036954520U)
          {
            if ((int) stringHash != -1825925217)
            {
              if ((int) stringHash != -1554958488)
              {
                if ((int) stringHash == -1258012776 && eventName == "OpenGamesEvent")
                  objectList.Add(new OpenGamesEvent()
                  {
                    visit_source = storedEvent.visit_source
                  });
              }
              else if (eventName == "OpenGroupEvent")
                objectList.Add(new OpenGroupEvent()
                {
                  GroupId = storedEvent.group_id,
                  Source = storedEvent.group_source
                });
            }
            else if (eventName == "PostInteractionEvent")
              objectList.Add(new PostInteractionEvent()
              {
                PostId = storedEvent.PostId,
                Action = storedEvent.PostAction,
                Link = storedEvent.Link
              });
          }
          else if ((int) stringHash != -1071685810)
          {
            if ((int) stringHash != -1012143120)
            {
              if ((int) stringHash == -758685995 && eventName == "GifPlayEvent")
                objectList.Add(new GifPlayEvent(storedEvent.GifPlayGifId, storedEvent.GifPlayStartType, storedEvent.source));
            }
            else if (eventName == "MarketItemActionEvent")
              objectList.Add(new MarketItemActionEvent()
              {
                itemId = storedEvent.market_item_id,
                source = storedEvent.market_item_source
              });
          }
          else if (eventName == "ViewPostEvent")
            objectList.Add(new ViewPostEvent()
            {
              PostId = storedEvent.post_id,
              CopyPostIds = storedEvent.repost_ids,
              Position = storedEvent.item_position,
              FeedSource = storedEvent.FeedSource,
              ItemType = storedEvent.ItemType,
              Source = storedEvent.Source
            });
        }
        else if (stringHash <= 3892414414U)
        {
          if ((int) stringHash != -564879486)
          {
            if ((int) stringHash != -428302409)
            {
              if ((int) stringHash == -402552882 && eventName == "AudioMessagePlayEvent")
                objectList.Add(new AudioMessagePlayEvent(storedEvent.AudioMessageId));
            }
            else if (eventName == "PostActionEvent")
              objectList.Add(new PostActionEvent()
              {
                PostId = storedEvent.PostId,
                ActionType = storedEvent.ActionType
              });
          }
          else if (eventName == "SubscriptionFromPostEvent")
            objectList.Add(new SubscriptionFromPostEvent()
            {
              post_id = storedEvent.post_id
            });
        }
        else if (stringHash <= 4054700410U)
        {
          if ((int) stringHash != -292036446)
          {
            if ((int) stringHash == -240266886 && eventName == "GamesActionEvent")
              objectList.Add(new GamesActionEvent()
              {
                game_id = storedEvent.game_id,
                action_type = storedEvent.action_type,
                request_name = storedEvent.request_name,
                click_source = storedEvent.click_source,
                visit_source = storedEvent.visit_source
              });
          }
          else if (eventName == "OpenUserEvent")
            objectList.Add(new OpenUserEvent()
            {
              UserId = storedEvent.user_id,
              Source = storedEvent.user_source
            });
        }
        else if ((int) stringHash != -149511298)
        {
          if ((int) stringHash == -53236589 && eventName == "AdImpressionEvent")
            objectList.Add(new AdImpressionEvent()
            {
              AdDataImpression = storedEvent.ad_data_impression
            });
        }
        else if (eventName == "StickersPurchaseFunnelEvent")
          objectList.Add(new StickersPurchaseFunnelEvent(storedEvent.StickersPurchaseFunnelSource, storedEvent.StickersPurchaseFunnelAction));
      }
      return objectList;
    }
      */
      public static List<object> ConvertToPendingEvents(List<PendingStatisticsEvent> storedEvents)
      {
          List<object> objectList = new List<object>();
          foreach (PendingStatisticsEvent storedEvent in storedEvents)
          {
              string eventName = storedEvent.event_name;

              if (eventName == "AudioPlayEvent")
              {
                  objectList.Add((object)new AudioPlayEvent()
                  {
                      OwnerAndAudioId = storedEvent.audio_id,
                      Source = storedEvent.source
                  });
              }
              else if (eventName == "ViewBlockEvent")
              {
                  objectList.Add((object)new ViewBlockEvent()
                  {
                      ItemType = storedEvent.item_type,
                      Position = storedEvent.item_position
                  });
              }
              else if (eventName == "OpenVideoEvent")
              {
                  objectList.Add((object)new OpenVideoEvent()
                  {
                      id = storedEvent.video_id,
                      Source = storedEvent.source,
                      context = storedEvent.video_context
                  });
              }
              else if (eventName == "MenuClickEvent")
              {
                  objectList.Add((object)new MenuClickEvent()
                  {
                      item = storedEvent.item
                  });
              }
              else if (eventName == "TransitionFromPostEvent")//1
              {
                  objectList.Add((object)new TransitionFromPostEvent()
                  {
                      post_id = storedEvent.post_id,
                      parent_id = storedEvent.parent_id
                  });
              }
              else if (eventName == "PostLinkClickEvent")
              {
                  objectList.Add((object)new HyperlinkClickedEvent()
                  {
                      HyperlinkOwnerId = storedEvent.post_id
                  });
              }
              else if (eventName == "OpenPostEvent")
              {
                  objectList.Add((object)new OpenPostEvent()
                  {
                      PostId = storedEvent.post_id,
                      CopyPostIds = storedEvent.repost_ids
                  });
              }
              else if (eventName == "ProfileBlockClickEvent")
              {
                  objectList.Add((object)new ProfileBlockClickEvent()
                  {
                      UserId = storedEvent.user_id,
                      BlockType = storedEvent.profile_block_type
                  });
              }
              else if (eventName == "BalanceTopupEvent")
              {
                  objectList.Add((object)new BalanceTopupEvent(storedEvent.BalanceTopupSource, storedEvent.BalanceTopupAction));
              }
              else if (eventName == "DiscoverActionEvent")
              {
                  objectList.Add((object)new DiscoverActionEvent()
                  {
                      ActionType = storedEvent.discover_action_type,
                      ActionParam = storedEvent.discover_action_param
                  });
              }
              else if (eventName == "MarketContactEvent")
              {
                  objectList.Add((object)new MarketContactEvent(storedEvent.market_item_id, storedEvent.MarketContactAction));
              }
              else if (eventName == "OpenGroupEvent")
              {
                  objectList.Add((object)new OpenGroupEvent()
                  {
                      GroupId = storedEvent.group_id,
                      Source = storedEvent.group_source
                  });
              }
              else if (eventName == "VideoPlayEvent")
              {
                  objectList.Add((object)new VideoPlayEvent()
                  {
                      id = storedEvent.video_id,
                      Position = storedEvent.position,
                      quality = storedEvent.quality,
                      Source = storedEvent.source,
                      Context = storedEvent.video_context
                  });
              }
              else if (eventName == "MarketItemActionEvent")
              {
                  objectList.Add((object)new MarketItemActionEvent()
                  {
                      itemId = storedEvent.market_item_id,
                      source = storedEvent.market_item_source
                  });
              }
              else if (eventName == "ViewPostEvent")
              {
                  objectList.Add((object)new ViewPostEvent()
                  {
                      PostId = storedEvent.post_id,
                      CopyPostIds = storedEvent.repost_ids,
                      Position = storedEvent.item_position,
                      FeedSource = storedEvent.FeedSource,
                      ItemType = storedEvent.ItemType,
                      Source = storedEvent.Source
                  });
              }
              else if (eventName == "OpenGamesEvent")
              {
                  objectList.Add((object)new OpenGamesEvent()
                  {
                      visit_source = storedEvent.visit_source
                  });
              }
              else if (eventName == "OpenUserEvent")
              {
                  objectList.Add((object)new OpenUserEvent()
                  {
                      UserId = storedEvent.user_id,
                      Source = storedEvent.user_source
                  });
              }
              else if (eventName == "SubscriptionFromPostEvent")
              {
                  objectList.Add((object)new SubscriptionFromPostEvent()
                  {
                      post_id = storedEvent.post_id
                  });
              }
              else if (eventName == "GifPlayEvent")
              {
                  objectList.Add((object)new GifPlayEvent(storedEvent.GifPlayGifId, storedEvent.GifPlayStartType, storedEvent.source));
              }
              else if (eventName == "AdImpressionEvent")
              {
                  objectList.Add((object)new AdImpressionEvent()
                  {
                      AdDataImpression = storedEvent.ad_data_impression
                  });
              }
              else if (eventName == "StickersPurchaseFunnelEvent")
              {
                  objectList.Add((object)new StickersPurchaseFunnelEvent(storedEvent.StickersPurchaseFunnelSource, storedEvent.StickersPurchaseFunnelAction));
              }
              else if (eventName == "GamesActionEvent")
              {
                  objectList.Add((object)new GamesActionEvent()
                  {
                      game_id = storedEvent.game_id,
                      action_type = storedEvent.action_type,
                      request_name = storedEvent.request_name,
                      click_source = storedEvent.click_source,
                      visit_source = storedEvent.visit_source
                  });
              }
              else
              {
                  System.Diagnostics.Debug.WriteLine("-----------------EventsConverter.ConvertToPendingEvents " + eventName);
              }//todo: undone
          }
          return objectList;
      }

    public static List<PendingStatisticsEvent> ConvertFromPendingEvents(List<object> pendingEvents)
    {
      List<PendingStatisticsEvent> pendingStatisticsEventList = new List<PendingStatisticsEvent>();
      foreach (object pendingEvent in pendingEvents)
      {
        PendingStatisticsEvent pendingStatisticsEvent = new PendingStatisticsEvent();
        if (pendingEvent is OpenUserEvent)
        {
          pendingStatisticsEvent.event_name = "OpenUserEvent";
          pendingStatisticsEvent.user_id = (pendingEvent as OpenUserEvent).UserId;
          pendingStatisticsEvent.user_source = (pendingEvent as OpenUserEvent).Source;
        }
        else if (pendingEvent is OpenGroupEvent)
        {
          pendingStatisticsEvent.event_name = "OpenGroupEvent";
          pendingStatisticsEvent.group_id = (pendingEvent as OpenGroupEvent).GroupId;
          pendingStatisticsEvent.group_source = (pendingEvent as OpenGroupEvent).Source;
        }
        else if (pendingEvent is ViewPostEvent)
        {
          ViewPostEvent viewPostEvent = pendingEvent as ViewPostEvent;
          pendingStatisticsEvent.event_name = "ViewPostEvent";
          pendingStatisticsEvent.post_id = viewPostEvent.PostId;
          pendingStatisticsEvent.repost_ids = viewPostEvent.CopyPostIds;
          pendingStatisticsEvent.item_position = viewPostEvent.Position;
          pendingStatisticsEvent.FeedSource = viewPostEvent.FeedSource;
          pendingStatisticsEvent.Source = viewPostEvent.Source;
          pendingStatisticsEvent.ItemType = viewPostEvent.ItemType;
        }
        else if (pendingEvent is ViewBlockEvent)
        {
          ViewBlockEvent viewBlockEvent = pendingEvent as ViewBlockEvent;
          pendingStatisticsEvent.event_name = "ViewBlockEvent";
          pendingStatisticsEvent.item_type = viewBlockEvent.ItemType;
          pendingStatisticsEvent.item_position = viewBlockEvent.Position;
        }
        else if (pendingEvent is OpenPostEvent)
        {
          pendingStatisticsEvent.event_name = "OpenPostEvent";
          OpenPostEvent openPostEvent = pendingEvent as OpenPostEvent;
          pendingStatisticsEvent.post_id = openPostEvent.PostId;
          pendingStatisticsEvent.repost_ids = openPostEvent.CopyPostIds;
        }
        else if (pendingEvent is OpenVideoEvent)
        {
          pendingStatisticsEvent.event_name = "OpenVideoEvent";
          OpenVideoEvent openVideoEvent = pendingEvent as OpenVideoEvent;
          pendingStatisticsEvent.video_id = openVideoEvent.id;
          pendingStatisticsEvent.source = openVideoEvent.Source;
          pendingStatisticsEvent.video_context = openVideoEvent.context;
        }
        else if (pendingEvent is VideoPlayEvent)
        {
          pendingStatisticsEvent.event_name = "VideoPlayEvent";
          VideoPlayEvent videoPlayEvent = pendingEvent as VideoPlayEvent;
          pendingStatisticsEvent.video_id = videoPlayEvent.id;
          pendingStatisticsEvent.position = videoPlayEvent.Position;
          pendingStatisticsEvent.source = videoPlayEvent.Source;
          pendingStatisticsEvent.quality = videoPlayEvent.quality;
          pendingStatisticsEvent.video_context = videoPlayEvent.Context;
        }
        else if (pendingEvent is AudioPlayEvent)
        {
          pendingStatisticsEvent.event_name = "AudioPlayEvent";
          pendingStatisticsEvent.audio_id = (pendingEvent as AudioPlayEvent).OwnerAndAudioId;
          pendingStatisticsEvent.source = (pendingEvent as AudioPlayEvent).Source;
        }
        else if (pendingEvent is MenuClickEvent)
        {
          pendingStatisticsEvent.event_name = "MenuClickEvent";
          MenuClickEvent menuClickEvent = pendingEvent as MenuClickEvent;
          pendingStatisticsEvent.item = menuClickEvent.item;
        }
        else if (pendingEvent is TransitionFromPostEvent)
        {
          pendingStatisticsEvent.event_name = "TransitionFromPostEvent";
          TransitionFromPostEvent transitionFromPostEvent = pendingEvent as TransitionFromPostEvent;
          pendingStatisticsEvent.post_id = transitionFromPostEvent.post_id;
          pendingStatisticsEvent.parent_id = transitionFromPostEvent.parent_id;
        }
        else if (pendingEvent is SubscriptionFromPostEvent)
        {
          pendingStatisticsEvent.event_name = "SubscriptionFromPostEvent";
          SubscriptionFromPostEvent subscriptionFromPostEvent = pendingEvent as SubscriptionFromPostEvent;
          pendingStatisticsEvent.post_id = subscriptionFromPostEvent.post_id;
        }
        else if (pendingEvent is HyperlinkClickedEvent)
        {
          pendingStatisticsEvent.event_name = "PostLinkClickEvent";
          HyperlinkClickedEvent hyperlinkClickedEvent = pendingEvent as HyperlinkClickedEvent;
          pendingStatisticsEvent.post_id = hyperlinkClickedEvent.HyperlinkOwnerId;
        }
        else if (pendingEvent is OpenGamesEvent)
        {
          pendingStatisticsEvent.event_name = "OpenGamesEvent";
          OpenGamesEvent openGamesEvent = pendingEvent as OpenGamesEvent;
          pendingStatisticsEvent.visit_source = openGamesEvent.visit_source;
        }
        else if (pendingEvent is GamesActionEvent)
        {
          pendingStatisticsEvent.event_name = "GamesActionEvent";
          GamesActionEvent gamesActionEvent = pendingEvent as GamesActionEvent;
          pendingStatisticsEvent.game_id = gamesActionEvent.game_id;
          pendingStatisticsEvent.action_type = gamesActionEvent.action_type;
          pendingStatisticsEvent.request_name = gamesActionEvent.request_name;
          pendingStatisticsEvent.click_source = gamesActionEvent.click_source;
          pendingStatisticsEvent.visit_source = gamesActionEvent.visit_source;
        }
        else if (pendingEvent is AdImpressionEvent)
        {
          pendingStatisticsEvent.event_name = "AdImpressionEvent";
          pendingStatisticsEvent.ad_data_impression = (pendingEvent as AdImpressionEvent).AdDataImpression;
        }
        else if (pendingEvent is MarketItemActionEvent)
        {
          pendingStatisticsEvent.event_name = "MarketItemActionEvent";
          MarketItemActionEvent marketItemActionEvent = pendingEvent as MarketItemActionEvent;
          pendingStatisticsEvent.market_item_source = marketItemActionEvent.source;
          pendingStatisticsEvent.market_item_id = marketItemActionEvent.itemId;
        }
        else if (pendingEvent is ProfileBlockClickEvent)
        {
          pendingStatisticsEvent.event_name = "ProfileBlockClickEvent";
          ProfileBlockClickEvent profileBlockClickEvent = pendingEvent as ProfileBlockClickEvent;
          pendingStatisticsEvent.user_id = profileBlockClickEvent.UserId;
          pendingStatisticsEvent.profile_block_type = profileBlockClickEvent.BlockType;
        }
        else if (pendingEvent is DiscoverActionEvent)
        {
          pendingStatisticsEvent.event_name = "DiscoverActionEvent";
          DiscoverActionEvent discoverActionEvent = pendingEvent as DiscoverActionEvent;
          pendingStatisticsEvent.discover_action_type = discoverActionEvent.ActionType;
          pendingStatisticsEvent.discover_action_param = discoverActionEvent.ActionParam;
        }
        else if (pendingEvent is MarketContactEvent)
        {
          pendingStatisticsEvent.event_name = "MarketContactEvent";
          MarketContactEvent marketContactEvent = pendingEvent as MarketContactEvent;
          pendingStatisticsEvent.market_item_id = marketContactEvent.ItemId;
          pendingStatisticsEvent.MarketContactAction = marketContactEvent.Action;
        }
        else if (pendingEvent is BalanceTopupEvent)
        {
          pendingStatisticsEvent.event_name = "BalanceTopupEvent";
          BalanceTopupEvent balanceTopupEvent = pendingEvent as BalanceTopupEvent;
          pendingStatisticsEvent.BalanceTopupSource = balanceTopupEvent.Source;
          pendingStatisticsEvent.BalanceTopupAction = balanceTopupEvent.Action;
        }
        else if (pendingEvent is StickersPurchaseFunnelEvent)
        {
          pendingStatisticsEvent.event_name = "StickersPurchaseFunnelEvent";
          StickersPurchaseFunnelEvent purchaseFunnelEvent = pendingEvent as StickersPurchaseFunnelEvent;
          pendingStatisticsEvent.StickersPurchaseFunnelSource = purchaseFunnelEvent.Source;
          pendingStatisticsEvent.StickersPurchaseFunnelAction = purchaseFunnelEvent.Action;
        }
        else if (pendingEvent is GifPlayEvent)
        {
          pendingStatisticsEvent.event_name = "GifPlayEvent";
          GifPlayEvent gifPlayEvent = pendingEvent as GifPlayEvent;
          pendingStatisticsEvent.GifPlayGifId = gifPlayEvent.GifId;
          pendingStatisticsEvent.GifPlayStartType = gifPlayEvent.StartType;
          pendingStatisticsEvent.source = gifPlayEvent.Source;
        }
        else if (pendingEvent is PostActionEvent)
        {
          pendingStatisticsEvent.event_name = "PostActionEvent";
          PostActionEvent postActionEvent = pendingEvent as PostActionEvent;
          pendingStatisticsEvent.PostId = postActionEvent.PostId;
          pendingStatisticsEvent.ActionType = postActionEvent.ActionType;
        }
        else if (pendingEvent is AudioMessagePlayEvent)
        {
          pendingStatisticsEvent.event_name = "AudioMessagePlayEvent";
          AudioMessagePlayEvent messagePlayEvent = pendingEvent as AudioMessagePlayEvent;
          pendingStatisticsEvent.AudioMessageId = messagePlayEvent.AudioMessageId;
        }
        else if (pendingEvent is GiftsPurchaseStepsEvent)
        {
          pendingStatisticsEvent.event_name = "GiftsPurchaseStepsEvent";
          GiftsPurchaseStepsEvent purchaseStepsEvent = pendingEvent as GiftsPurchaseStepsEvent;
          pendingStatisticsEvent.GiftPurchaseStepsSource = purchaseStepsEvent.Source;
          pendingStatisticsEvent.GiftPurchaseStepsAction = purchaseStepsEvent.Action;
        }
        else if (pendingEvent is PostInteractionEvent)
        {
          pendingStatisticsEvent.event_name = "PostInteractionEvent";
          PostInteractionEvent interactionEvent = pendingEvent as PostInteractionEvent;
          pendingStatisticsEvent.PostId = interactionEvent.PostId;
          pendingStatisticsEvent.PostAction = interactionEvent.Action;
          pendingStatisticsEvent.Link = interactionEvent.Link;
        }
        if (pendingStatisticsEvent != null)
          pendingStatisticsEventList.Add(pendingStatisticsEvent);
      }
      return pendingStatisticsEventList;
    }
  }
}
