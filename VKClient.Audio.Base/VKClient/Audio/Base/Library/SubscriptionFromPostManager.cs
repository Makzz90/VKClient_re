using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Audio.Base.Library
{
  public class SubscriptionFromPostManager : IHandle<TransitionFromPostEvent>, IHandle, IHandle<FriendRequestSent>, IHandle<GroupMembershipStatusUpdated>, IBinarySerializable
  {
    private List<SubscriptionFromPostManager.TransitionToPostData> _transitionTimes = new List<SubscriptionFromPostManager.TransitionToPostData>();
    private static SubscriptionFromPostManager _instance;

    public static SubscriptionFromPostManager Instance
    {
      get
      {
        if (SubscriptionFromPostManager._instance == null)
          SubscriptionFromPostManager._instance = new SubscriptionFromPostManager();
        return SubscriptionFromPostManager._instance;
      }
    }

    public SubscriptionFromPostManager()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public void Handle(TransitionFromPostEvent message)
    {
      string[] strArray = message.post_id.Split('_');
      long result;
      if (strArray.Length != 2 || !long.TryParse(strArray[0], out result))
        return;
      SubscriptionFromPostManager.TransitionToPostData transitionToPostData = new SubscriptionFromPostManager.TransitionToPostData();
      transitionToPostData.DateTime = DateTime.UtcNow;
      string postId = message.post_id;
      transitionToPostData.PostId = postId;
      long num = result;
      transitionToPostData.OwnerId = num;
      this.Add(transitionToPostData);
    }

    private void Add(SubscriptionFromPostManager.TransitionToPostData transitionToPostData)
    {
      SubscriptionFromPostManager.TransitionToPostData transitionToPostData1 = this._transitionTimes.FirstOrDefault<SubscriptionFromPostManager.TransitionToPostData>((Func<SubscriptionFromPostManager.TransitionToPostData, bool>) (t => t.OwnerId == transitionToPostData.OwnerId));
      if (transitionToPostData1 != null)
        this._transitionTimes.Remove(transitionToPostData1);
      this._transitionTimes.Add(transitionToPostData);
    }

    public void Handle(FriendRequestSent message)
    {
      this.CheckSubscriptionFromPostForId(message.UserId);
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (!message.Joined || message.GroupId == 0L)
        return;
      this.CheckSubscriptionFromPostForId(-message.GroupId);
    }

    private void CheckSubscriptionFromPostForId(long ownerId)
    {
      SubscriptionFromPostManager.TransitionToPostData transitionToPostData = this._transitionTimes.FirstOrDefault<SubscriptionFromPostManager.TransitionToPostData>((Func<SubscriptionFromPostManager.TransitionToPostData, bool>) (t => t.OwnerId == ownerId));
      if (transitionToPostData == null || (DateTime.UtcNow - transitionToPostData.DateTime).TotalHours > 24.0)
        return;
      EventAggregator.Current.Publish(new SubscriptionFromPostEvent()
      {
        post_id = transitionToPostData.PostId
      });
      this._transitionTimes.Remove(transitionToPostData);
    }

    public void Save()
    {
      CacheManager.TrySerialize((IBinarySerializable) this, "TransitionData", false, CacheManager.DataType.CachedData);
    }

    public void Restore()
    {
      CacheManager.TryDeserialize((IBinarySerializable) this, "TransitionData", CacheManager.DataType.CachedData);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList<SubscriptionFromPostManager.TransitionToPostData>((IList<SubscriptionFromPostManager.TransitionToPostData>) this._transitionTimes.Where<SubscriptionFromPostManager.TransitionToPostData>((Func<SubscriptionFromPostManager.TransitionToPostData, bool>) (t => (DateTime.UtcNow - t.DateTime).TotalHours <= 24.0)).ToList<SubscriptionFromPostManager.TransitionToPostData>(), 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._transitionTimes = reader.ReadList<SubscriptionFromPostManager.TransitionToPostData>();
    }

    public class TransitionToPostData : IBinarySerializable
    {
      public DateTime DateTime { get; set; }

      public string PostId { get; set; }

      public long OwnerId { get; set; }

      public void Write(BinaryWriter writer)
      {
        writer.Write(1);
        writer.Write(this.DateTime);
        writer.WriteString(this.PostId);
        writer.Write(this.OwnerId);
      }

      public void Read(BinaryReader reader)
      {
        reader.ReadInt32();
        this.DateTime = reader.ReadDateTime();
        this.PostId = reader.ReadString();
        this.OwnerId = reader.ReadInt64();
      }
    }
  }
}
