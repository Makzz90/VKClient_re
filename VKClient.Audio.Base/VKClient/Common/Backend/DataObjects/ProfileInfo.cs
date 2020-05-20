using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class ProfileInfo
  {
    private string _fName = "";
    private string _lName = "";
    private string _sName = "";

    public string photo_max { get; set; }

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

    public string screen_name
    {
      get
      {
        return this._sName;
      }
      set
      {
        this._sName = (value ?? "").ForUI();
      }
    }

    public int sex { get; set; }

    public int relation { get; set; }

    public User relation_partner { get; set; }

    public int relation_pending { get; set; }

    public List<User> relation_requests { get; set; }

    public string bdate { get; set; }

    public int bdate_visibility { get; set; }

    public City city { get; set; }

    public Country country { get; set; }

    public string home_town { get; set; }

    public NameChangeRequest name_request { get; set; }

    public string phone { get; set; }
  }
}
