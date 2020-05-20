using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class ChatBase
  {
    private string _title = "";

    public string type { get; set; }

    public long chat_id
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public long id { get; set; }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public string admin_id { get; set; }

    public string photo_100 { get; set; }

    public string photo_200 { get; set; }

    public VKMessenger.Backend.PushSettings push_settings { get; set; }

    public ChatBase()
    {
      this.push_settings = new VKMessenger.Backend.PushSettings();
    }
  }
}
