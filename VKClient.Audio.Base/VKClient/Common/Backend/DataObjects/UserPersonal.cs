using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class UserPersonal
  {
    private string _inspired_by = "";

    public int political { get; set; }

    public List<string> langs { get; set; }

    public string religion { get; set; }

    public string inspired_by
    {
      get
      {
        return this._inspired_by;
      }
      set
      {
        this._inspired_by = (value ?? "").ForUI();
      }
    }

    public int people_main { get; set; }

    public int life_main { get; set; }

    public int smoking { get; set; }

    public int alcohol { get; set; }
  }
}
