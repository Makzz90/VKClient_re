namespace VKClient.Common.Backend.DataObjects
{
  public class NewsCaption
  {
    private string _text;

    public string text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = (value ?? "").ForUI();
      }
    }
  }
}
