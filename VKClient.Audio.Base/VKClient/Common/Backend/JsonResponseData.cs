namespace VKClient.Common.Backend
{
  public class JsonResponseData
  {
    private bool _isSucceeded;
    private string _jsonString;

    public bool IsSucceeded
    {
      get
      {
        return this._isSucceeded;
      }
    }

    public string JsonString
    {
      get
      {
        return this._jsonString;
      }
    }

    public JsonResponseData(bool isSucceeded, string jsonString = "")
    {
      this._isSucceeded = isSucceeded;
      this._jsonString = jsonString;
    }
  }
}
