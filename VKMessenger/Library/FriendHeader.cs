using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKMessenger.Library
{
  public class FriendHeader : ViewModelBase, IBinarySerializable
  {
    private User _user;
    private long _userId;
    private string _imageUrl;
    private string _mediumImageUrl;
    private string _fullName;
    private Visibility _isOnline;
    private char? _initial;

    public User User
    {
      get
      {
        return this._user;
      }
    }

    public bool IsFriend { get; set; }

    public long UserId
    {
      get
      {
        return this._userId;
      }
    }

    public string ImageUrl
    {
      get
      {
        return this._imageUrl;
      }
      set
      {
        this._imageUrl = value;
        this.NotifyPropertyChanged<string>(() => this.ImageUrl);
      }
    }

    public string MediumImageUrl
    {
      get
      {
        return this._mediumImageUrl;
      }
      set
      {
        this._mediumImageUrl = value;
        this.NotifyPropertyChanged<string>(() => this.MediumImageUrl);
      }
    }

    public string FullName
    {
      get
      {
        return this._fullName;
      }
      set
      {
        this._fullName = value;
        this.Initial = new char?(string.IsNullOrEmpty(value) ? ' ' : ((IEnumerable<char>) ((string) value).ToLower()).First<char>());
        this.NotifyPropertyChanged<string>(() => this.FullName);
      }
    }

    public Visibility IsOnline
    {
      get
      {
        return this._isOnline;
      }
      set
      {
        if (this._isOnline == value)
          return;
        this._isOnline = value;
        this.NotifyPropertyChanged<Visibility>(() => this.IsOnline);
      }
    }

    public char? Initial
    {
      get
      {
        return this._initial;
      }
      set
      {
        this._initial = value;
        base.NotifyPropertyChanged<char?>(() => this.Initial);
      }
    }

    public FriendHeader()
    {
    }

    public FriendHeader(User user)
    {
      this._user = user;
      this.BindToUser();
      this.IsFriend = true;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write<User>(this._user, false);
      writer.Write(this.IsFriend);
    }

    public void Read(BinaryReader reader)
    {
      this._user = reader.ReadGeneric<User>();
      this.IsFriend = reader.ReadBoolean();
      this.BindToUser();
    }

    public bool Matches(IList<string> searchStrings)
    {
      if (this._user == null)
        return false;
      bool flag1 = true;
      IEnumerator<string> enumerator = searchStrings.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          string current = enumerator.Current;
          bool flag2 = !string.IsNullOrEmpty(this._user.first_name) && ((string) this._user.first_name).StartsWith(current, (StringComparison) 3) || !string.IsNullOrEmpty(this._user.last_name) && ((string) this._user.last_name).StartsWith(current, (StringComparison) 3);
          flag1 &= flag2;
          if (!flag1)
            break;
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      return flag1;
    }

    internal void BindToUser()
    {
      if (this._user == null)
        return;
      this.ImageUrl = this._user.photo_max;
      this.MediumImageUrl = this._user.photo_max;
      this.FullName = string.Format("{0} {1}", this._user.first_name, this._user.last_name);
      this.SetIsOnline((uint) this._user.online > 0U);
      this._userId = this._user.uid;
    }

    public void SetIsOnline(bool status)
    {
      this.IsOnline = status ? Visibility.Visible : Visibility.Collapsed;
    }
  }
}
