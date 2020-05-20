using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.DataObjects
{
  public class PrivacyInfo
  {
    public PrivacyType PrivacyType { get; set; }

    public List<long> DeniedUsers { get; set; }

    public List<long> DeniedLists { get; set; }

    public List<long> AllowedUsers { get; set; }

    public List<long> AllowedLists { get; set; }

    public PrivacyInfo(List<string> inputStrings)
    {
      this.DeniedUsers = new List<long>();
      this.AllowedUsers = new List<long>();
      this.DeniedLists = new List<long>();
      this.AllowedLists = new List<long>();
      this.Parse(inputStrings);
    }

    public PrivacyInfo()
      : this("all")
    {
    }

    public PrivacyInfo(string privacyStr)
    {
      this.DeniedUsers = new List<long>();
      this.AllowedUsers = new List<long>();
      this.DeniedLists = new List<long>();
      this.AllowedLists = new List<long>();
      this.Parse(((IEnumerable<string>) privacyStr.Split(',')).ToList<string>());
    }

    public void CleanupAllowedDeniedArraysBasedOnPrivacyType()
    {
      switch (this.PrivacyType)
      {
        case PrivacyType.AllUsers:
        case PrivacyType.Friends:
        case PrivacyType.FriendsOfFriends:
        case PrivacyType.FriendsOfFriendsOnly:
          this.AllowedLists.Clear();
          this.AllowedUsers.Clear();
          break;
        case PrivacyType.OnlyMe:
        case PrivacyType.Nobody:
          this.AllowedUsers.Clear();
          this.AllowedLists.Clear();
          this.DeniedUsers.Clear();
          this.DeniedLists.Clear();
          break;
        case PrivacyType.CertainUsers:
          if (this.AllowedLists.Any<long>())
            break;
          this.DeniedUsers.Clear();
          this.DeniedLists.Clear();
          break;
      }
    }

    public List<string> ToStringList()
    {
      return ((IEnumerable<string>) this.ToString().Split(',')).ToList<string>();
    }

    public override string ToString()
    {
      string str = "";
      switch (this.PrivacyType)
      {
        case PrivacyType.AllUsers:
          str = "all";
          break;
        case PrivacyType.Friends:
          str = "friends";
          break;
        case PrivacyType.FriendsOfFriends:
          str = "friends_of_friends";
          break;
        case PrivacyType.OnlyMe:
          str = "only_me";
          break;
        case PrivacyType.FriendsOfFriendsOnly:
          str = "friends_of_friends_only";
          break;
        case PrivacyType.Nobody:
          str = "nobody";
          break;
      }
      List<string> list = this.AllowedUsers.Select<long, string>((Func<long, string>) (uid => uid.ToString())).Union<string>(this.DeniedUsers.Select<long, string>((Func<long, string>) (duid => "-" + duid.ToString()))).Union<string>(this.AllowedLists.Select<long, string>((Func<long, string>) (alid => "list" + alid))).Union<string>(this.DeniedLists.Select<long, string>((Func<long, string>) (dlid => "-list" + dlid))).ToList<string>();
      if (list.Count > 0 && str != "")
        str += ",";
      return str + list.GetCommaSeparated(",");
    }

    private void Parse(List<string> inputStrings)
    {
      if (inputStrings.Count == 0)
        inputStrings.Add("all");
      this.PrivacyType = PrivacyType.CertainUsers;
      foreach (string inputString in inputStrings)
      {
        if (inputString == "all")
          this.PrivacyType = PrivacyType.AllUsers;
        else if (inputString == "friends")
          this.PrivacyType = PrivacyType.Friends;
        else if (inputString == "friends_of_friends")
          this.PrivacyType = PrivacyType.FriendsOfFriends;
        else if (inputString == "friends_of_friends_only")
          this.PrivacyType = PrivacyType.FriendsOfFriendsOnly;
        else if (inputString == "only_me")
        {
          this.PrivacyType = PrivacyType.OnlyMe;
        }
        else
        {
          if (inputString == "nobody")
            this.PrivacyType = PrivacyType.Nobody;
          if (inputString.StartsWith("list"))
            this.AllowedLists.Add(long.Parse(inputString.Substring(4)));
          else if (inputString.StartsWith("-list"))
            this.DeniedLists.Add(long.Parse(inputString.Substring(5)));
          else if (inputString.StartsWith("-"))
          {
            this.DeniedUsers.Add(long.Parse(inputString.Substring(1)));
          }
          else
          {
            long result = 0;
            if (long.TryParse(inputString, out result))
              this.AllowedUsers.Add(result);
          }
        }
      }
    }
  }
}
