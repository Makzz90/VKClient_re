namespace VKClient.Common.Backend.DataObjects
{
  public class NameChangeRequest
  {
    private string _fName = "";
    private string _lName = "";

    public long id { get; set; }

    public string status { get; set; }

    public string first_name
    {
      get
      {
        return this._fName;
      }
      set
      {
        this._fName = (value ?? "").ForUI();
      }
    }

    public string last_name
    {
      get
      {
        return this._lName;
      }
      set
      {
        this._lName = (value ?? "").ForUI();
      }
    }

    public string lang { get; set; }
  }
}
