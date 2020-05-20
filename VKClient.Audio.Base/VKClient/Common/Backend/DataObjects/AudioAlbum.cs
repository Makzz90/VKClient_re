namespace VKClient.Common.Backend.DataObjects
{
  public class AudioAlbum
  {
    private string _title = "";

    public long owner_id { get; set; }

    public long album_id
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
  }
}
