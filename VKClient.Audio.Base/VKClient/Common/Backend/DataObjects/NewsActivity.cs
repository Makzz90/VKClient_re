namespace VKClient.Common.Backend.DataObjects
{
  public class NewsActivity
  {
    private string _type;

    public string type
    {
      get
      {
        return this._type;
      }
      set
      {
        this._type = value;
        string type = this._type;
        if (!(type == "likes"))
        {
          if (!(type == "comment"))
            return;
          this.Type = NewsActivityType.comment;
        }
        else
          this.Type = NewsActivityType.likes;
      }
    }

    public NewsActivityType Type { get; private set; }

    public NewsActivityLikes likes { get; set; }

    public NewsActivityComment comment { get; set; }
  }
}
