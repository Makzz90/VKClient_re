using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.Backend.DataObjects
{
  public class PushSettings : IBinarySerializable
  {
    public static readonly string On = "on";
    public static readonly string Off = "off";
    public static readonly string FrOfFr = "fr_of_fr";
    public static readonly string NoText = "no_text";

    public bool msg { get; set; }

    public bool msg_no_text { get; set; }

    public bool chat { get; set; }

    public bool chat_no_text { get; set; }

    public bool friend { get; set; }

    public bool friend_mutual { get; set; }

    public bool friend_found { get; set; }

    public bool friend_accepted { get; set; }

    public bool reply { get; set; }

    public bool comment { get; set; }

    public bool comment_fr_of_fr { get; set; }

    public bool mention { get; set; }

    public bool mention_fr_of_fr { get; set; }

    public bool like { get; set; }

    public bool like_fr_of_fr { get; set; }

    public bool repost { get; set; }

    public bool repost_fr_of_fr { get; set; }

    public bool wall_post { get; set; }

    public bool wall_publish { get; set; }

    public bool group_invite { get; set; }

    public bool group_accepted { get; set; }

    public bool event_soon { get; set; }

    public bool tag_photo { get; set; }

    public bool tag_photo_fr_of_fr { get; set; }

    public bool app_request { get; set; }

    public bool sdk_open { get; set; }

    public bool new_post { get; set; }

    public bool birthday { get; set; }

    public string ToJsonString()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["msg"] = PushSettings.GetOnOffStr(this.msg);
      if (this.msg_no_text)
        dictionary["msg"] = "no_text";
      dictionary["chat"] = PushSettings.GetOnOffStr(this.chat);
      if (this.chat_no_text)
        dictionary["chat"] = "no_text";
      dictionary["friend"] = PushSettings.GetOnOffStr(this.friend);
      if (this.friend_mutual)
        dictionary["friend"] = "mutual";
      dictionary["friend_found"] = PushSettings.GetOnOffStr(this.friend_found);
      dictionary["friend_accepted"] = "on";
      dictionary["reply"] = PushSettings.GetOnOffStr(this.reply);
      dictionary["comment"] = PushSettings.GetOnOffStr(this.comment);
      if (this.comment_fr_of_fr)
        dictionary["comment"] = "fr_of_fr";
      dictionary["mention"] = PushSettings.GetOnOffStr(this.mention);
      if (this.mention_fr_of_fr)
        dictionary["mention"] = "fr_of_fr";
      dictionary["like"] = PushSettings.GetOnOffStr(this.like);
      if (this.like_fr_of_fr)
        dictionary["like"] = "fr_of_fr";
      dictionary["repost"] = PushSettings.GetOnOffStr(this.repost);
      if (this.repost_fr_of_fr)
        dictionary["repost"] = "fr_of_fr";
      dictionary["wall_post"] = PushSettings.GetOnOffStr(this.wall_post);
      dictionary["wall_publish"] = "on";
      dictionary["group_invite"] = PushSettings.GetOnOffStr(this.group_invite);
      dictionary["group_accepted"] = "on";
      dictionary["event_soon"] = PushSettings.GetOnOffStr(this.event_soon);
      dictionary["tag_photo"] = PushSettings.GetOnOffStr(this.tag_photo);
      if (this.tag_photo_fr_of_fr)
        dictionary["tag_photo"] = "fr_of_fr";
      if (AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
      {
        dictionary["app_request"] = PushSettings.GetOnOffStr(this.app_request);
        dictionary["sdk_open"] = "on";
      }
      else
      {
        dictionary["app_request"] = "off";
        dictionary["sdk_open"] = "off";
      }
      dictionary["open_url"] = "on";
      dictionary["new_post"] = "on";
      dictionary["birthday"] = PushSettings.GetOnOffStr(this.birthday);
      dictionary["money_transfer"] = "on";
      return JsonConvert.SerializeObject(dictionary);
    }

    public static string GetOnOffStr(bool b)
    {
      if (!b)
        return PushSettings.Off;
      return PushSettings.On;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.msg);
      writer.Write(this.msg_no_text);
      writer.Write(this.chat);
      writer.Write(this.chat_no_text);
      writer.Write(this.friend);
      writer.Write(this.friend_mutual);
      writer.Write(this.friend_found);
      writer.Write(this.friend_accepted);
      writer.Write(this.reply);
      writer.Write(this.comment);
      writer.Write(this.comment_fr_of_fr);
      writer.Write(this.mention);
      writer.Write(this.mention_fr_of_fr);
      writer.Write(this.like);
      writer.Write(this.like_fr_of_fr);
      writer.Write(this.repost);
      writer.Write(this.repost_fr_of_fr);
      writer.Write(this.wall_post);
      writer.Write(this.wall_publish);
      writer.Write(this.group_invite);
      writer.Write(this.group_accepted);
      writer.Write(this.event_soon);
      writer.Write(this.tag_photo);
      writer.Write(this.tag_photo_fr_of_fr);
      writer.Write(this.app_request);
      writer.Write(this.new_post);
      writer.Write(this.birthday);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.msg = reader.ReadBoolean();
      this.msg_no_text = reader.ReadBoolean();
      this.chat = reader.ReadBoolean();
      this.chat_no_text = reader.ReadBoolean();
      this.friend = reader.ReadBoolean();
      this.friend_mutual = reader.ReadBoolean();
      this.friend_found = reader.ReadBoolean();
      this.friend_accepted = reader.ReadBoolean();
      this.reply = reader.ReadBoolean();
      this.comment = reader.ReadBoolean();
      this.comment_fr_of_fr = reader.ReadBoolean();
      this.mention = reader.ReadBoolean();
      this.mention_fr_of_fr = reader.ReadBoolean();
      this.like = reader.ReadBoolean();
      this.like_fr_of_fr = reader.ReadBoolean();
      this.repost = reader.ReadBoolean();
      this.repost_fr_of_fr = reader.ReadBoolean();
      this.wall_post = reader.ReadBoolean();
      this.wall_publish = reader.ReadBoolean();
      this.group_invite = reader.ReadBoolean();
      this.group_accepted = reader.ReadBoolean();
      this.event_soon = reader.ReadBoolean();
      this.tag_photo = reader.ReadBoolean();
      this.tag_photo_fr_of_fr = reader.ReadBoolean();
      this.app_request = reader.ReadBoolean();
      this.new_post = reader.ReadBoolean();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.birthday = reader.ReadBoolean();
    }

    public void EnableAll()
    {
      this.msg = true;
      this.chat = true;
      this.friend = true;
      this.friend_found = true;
      this.friend_accepted = true;
      this.reply = true;
      this.comment = true;
      this.mention = true;
      this.like = true;
      this.repost = true;
      this.wall_post = true;
      this.wall_publish = true;
      this.group_invite = true;
      this.group_accepted = true;
      this.event_soon = true;
      this.app_request = true;
      this.new_post = true;
      this.birthday = true;
    }
  }
}
