using System.ComponentModel;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GameRequestHeader : INotifyPropertyChanged
  {
    private bool _isSeparatorVisible;
    private bool _isRead;
    private double _isReadOpacity;

    public GameRequest GameRequest { get; private set; }

    public Game Game { get; private set; }

    public User User { get; private set; }

    public bool IsSeparatorVisible
    {
      get
      {
        return this._isSeparatorVisible;
      }
      set
      {
        this._isSeparatorVisible = value;
        this.OnPropertyChanged("IsSeparatorVisible");
      }
    }

    public bool IsInvite
    {
      get
      {
        return this.GameRequest.type == "invite";
      }
    }

    public bool IsRead
    {
      get
      {
        return this._isRead;
      }
      private set
      {
        this._isRead = value;
        this.OnPropertyChanged("IsRead");
      }
    }

    public double IsReadOpacity
    {
      get
      {
        return this._isReadOpacity;
      }
      set
      {
        this._isReadOpacity = value;
        this.OnPropertyChanged("IsReadOpacity");
      }
    }

    public string RequestDate
    {
      get
      {
        return UIStringFormatterHelper.FormatShortDate(this.GameRequest.date);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameRequestHeader(GameRequest gameRequest, Game game, User user)
      : this(gameRequest)
    {
      this.Game = game;
      this.User = user;
    }

    public GameRequestHeader(GameRequest gameRequest)
    {
      this.GameRequest = gameRequest;
      this.IsSeparatorVisible = true;
      this.IsRead = gameRequest.unread == 0;
    }

    public void MarkAsRead()
    {
      this.IsRead = true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
