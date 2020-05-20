using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class EmbeddedUrlResponse
  {
    private string _screenTitle;

    public string original_url { get; set; }

    public string view_url { get; set; }

    public string screen_title
    {
      get
      {
        return this._screenTitle;
      }
      set
      {
        this._screenTitle = (value ?? "").ForUI();
      }
    }
  }
}
