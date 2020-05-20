using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftItemData
  {
    private string _message;
    private int _privacy;

    public long id { get; set; }

    public long from_id { get; set; }

    public string message
    {
      get
      {
        return this._message;
      }
      set
      {
        this._message = (value ?? "").ForUI();
      }
    }

    public int date { get; set; }

    public Gift gift { get; set; }

    public int privacy
    {
      get
      {
        return this._privacy;
      }
      set
      {
        this._privacy = value;
        if (value != 1)
        {
          if (value == 2)
            this.Privacy = GiftPrivacy.Hidden;
          else
            this.Privacy = GiftPrivacy.VisibleToAll;
        }
        else
          this.Privacy = GiftPrivacy.VisibleToRecipient;
      }
    }

    public GiftPrivacy Privacy { get; private set; }

    public string gift_hash { get; set; }
  }
}
