using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PendingStatisticsEvent : IBinarySerializable
  {
    public string event_name { get; set; }

    public long user_id { get; set; }

    public string user_source { get; set; }

    public string post_id { get; set; }

    public string parent_id { get; set; }

    public List<string> repost_ids { get; set; }

    public long group_id { get; set; }

    public string group_source { get; set; }

    public string video_id { get; set; }

    public StatisticsVideoPosition position { get; set; }

    public StatisticsActionSource source { get; set; }

    public string video_context { get; set; }

    public string audio_id { get; set; }

    public int quality { get; set; }

    public string item { get; set; }

    public string item_type { get; set; }

    public int item_position { get; set; }

    public long game_id { get; set; }

    public string request_name { get; set; }

    public int misc_value1 { get; set; }

    public int misc_value2 { get; set; }

    public int misc_value3 { get; set; }

    public string ad_data_impression { get; set; }

    public string market_item_id { get; set; }

    public MarketItemSource market_item_source { get; set; }

    public ProfileBlockType profile_block_type { get; set; }

    public DiscoverActionType discover_action_type { get; set; }

    public string discover_action_param { get; set; }

    public GamesActionType action_type
    {
      get
      {
        return (GamesActionType) this.misc_value1;
      }
      set
      {
        this.misc_value1 = (int) value;
      }
    }

    public GamesVisitSource visit_source
    {
      get
      {
        return (GamesVisitSource) this.misc_value2;
      }
      set
      {
        this.misc_value2 = (int) value;
      }
    }

    public GamesClickSource click_source
    {
      get
      {
        return (GamesClickSource) this.misc_value3;
      }
      set
      {
        this.misc_value3 = (int) value;
      }
    }

    public NewsFeedItemType ItemType
    {
      get
      {
        return (NewsFeedItemType) this.misc_value1;
      }
      set
      {
        this.misc_value1 = (int) value;
      }
    }

    public ViewPostSource Source
    {
      get
      {
        return (ViewPostSource) this.misc_value2;
      }
      set
      {
        this.misc_value2 = (int) value;
      }
    }

    public NewsSourcesPredefined FeedSource
    {
      get
      {
        return (NewsSourcesPredefined) this.misc_value3;
      }
      set
      {
        this.misc_value3 = (int) value;
      }
    }

    public MarketContactAction MarketContactAction { get; set; }

    public BalanceTopupSource BalanceTopupSource { get; set; }

    public BalanceTopupAction BalanceTopupAction { get; set; }

    public StickersPurchaseFunnelSource StickersPurchaseFunnelSource { get; set; }

    public StickersPurchaseFunnelAction StickersPurchaseFunnelAction { get; set; }

    public string GifPlayGifId { get; set; }

    public GifPlayStartType GifPlayStartType { get; set; }

    public string GifPlayContext { get; set; }

    public string PostId { get; set; }

    public PostActionType ActionType { get; set; }

    public string AudioMessageId { get; set; }

    public GiftPurchaseStepsSource GiftPurchaseStepsSource { get; set; }

    public GiftPurchaseStepsAction GiftPurchaseStepsAction { get; set; }

    public PostInteractionAction PostAction { get; set; }

    public string Link { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(16);
      writer.WriteString(this.event_name);
      writer.Write(this.user_id);
      writer.WriteString(this.post_id);
      writer.WriteList(this.repost_ids);
      writer.Write(this.group_id);
      writer.WriteString(this.video_id);
      writer.Write((int) this.position);
      writer.Write((int) this.source);
      writer.WriteString(this.audio_id);
      writer.Write(this.quality);
      writer.WriteString(this.item);
      writer.Write(this.game_id);
      writer.WriteString(this.request_name);
      writer.Write(this.misc_value1);
      writer.Write(this.misc_value2);
      writer.Write(this.misc_value3);
      writer.WriteString(this.ad_data_impression);
      writer.WriteString(this.item_type);
      writer.Write(this.item_position);
      writer.WriteString(this.user_source);
      writer.WriteString(this.market_item_id);
      writer.Write((int) this.market_item_source);
      writer.WriteString(this.video_context);
      writer.WriteString(this.group_source);
      writer.WriteString(this.parent_id);
      writer.Write((int) this.profile_block_type);
      writer.Write((int) this.MarketContactAction);
      writer.Write((int) this.BalanceTopupSource);
      writer.Write((int) this.BalanceTopupAction);
      writer.Write((int) this.StickersPurchaseFunnelSource);
      writer.Write((int) this.StickersPurchaseFunnelAction);
      writer.WriteString(this.GifPlayGifId);
      writer.Write((int) this.GifPlayStartType);
      writer.WriteString(this.GifPlayContext);
      writer.WriteString(this.PostId);
      writer.Write((int) this.ActionType);
      writer.Write((int) this.GiftPurchaseStepsSource);
      writer.Write((int) this.GiftPurchaseStepsAction);
      writer.Write((int) this.PostAction);
      writer.WriteString(this.Link);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.event_name = reader.ReadString();
      this.user_id = reader.ReadInt64();
      this.post_id = reader.ReadString();
      this.repost_ids = reader.ReadList();
      this.group_id = reader.ReadInt64();
      this.video_id = reader.ReadString();
      this.position = (StatisticsVideoPosition) reader.ReadInt32();
      this.source = (StatisticsActionSource) reader.ReadInt32();
      this.audio_id = reader.ReadString();
      this.quality = reader.ReadInt32();
      int num2 = 2;
      if (num1 >= num2)
        this.item = reader.ReadString();
      int num3 = 3;
      if (num1 >= num3)
      {
        this.game_id = reader.ReadInt64();
        this.request_name = reader.ReadString();
        this.misc_value1 = reader.ReadInt32();
        this.misc_value2 = reader.ReadInt32();
        this.misc_value3 = reader.ReadInt32();
      }
      int num4 = 4;
      if (num1 >= num4)
        this.ad_data_impression = reader.ReadString();
      int num5 = 5;
      if (num1 >= num5)
      {
        this.item_type = reader.ReadString();
        this.item_position = reader.ReadInt32();
        this.user_source = reader.ReadString();
      }
      int num6 = 6;
      if (num1 >= num6)
      {
        this.market_item_id = reader.ReadString();
        this.market_item_source = (MarketItemSource) reader.ReadInt32();
      }
      int num7 = 7;
      if (num1 >= num7)
        this.video_context = reader.ReadString();
      int num8 = 8;
      if (num1 >= num8)
        this.group_source = reader.ReadString();
      int num9 = 9;
      if (num1 >= num9)
        this.parent_id = reader.ReadString();
      int num10 = 10;
      if (num1 >= num10)
        this.profile_block_type = (ProfileBlockType) reader.ReadInt32();
      int num11 = 11;
      if (num1 >= num11)
      {
        this.MarketContactAction = (MarketContactAction) reader.ReadInt32();
        this.BalanceTopupSource = (BalanceTopupSource) reader.ReadInt32();
        this.BalanceTopupAction = (BalanceTopupAction) reader.ReadInt32();
        this.StickersPurchaseFunnelSource = (StickersPurchaseFunnelSource) reader.ReadInt32();
        this.StickersPurchaseFunnelAction = (StickersPurchaseFunnelAction) reader.ReadInt32();
      }
      int num12 = 12;
      if (num1 >= num12)
      {
        this.GifPlayGifId = reader.ReadString();
        this.GifPlayStartType = (GifPlayStartType) reader.ReadInt32();
        this.GifPlayContext = reader.ReadString();
      }
      int num13 = 13;
      if (num1 >= num13)
      {
        this.PostId = reader.ReadString();
        this.ActionType = (PostActionType) reader.ReadInt32();
      }
      int num14 = 14;
      if (num1 >= num14)
      {
        this.GiftPurchaseStepsSource = (GiftPurchaseStepsSource) reader.ReadInt32();
        this.GiftPurchaseStepsAction = (GiftPurchaseStepsAction) reader.ReadInt32();
      }
      int num15 = 15;
      if (num1 >= num15)
        this.PostAction = (PostInteractionAction) reader.ReadInt32();
      int num16 = 16;
      if (num1 < num16)
        return;
      this.Link = reader.ReadString();
    }
  }
}
