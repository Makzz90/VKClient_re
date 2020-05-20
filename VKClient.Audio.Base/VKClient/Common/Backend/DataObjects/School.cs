namespace VKClient.Common.Backend.DataObjects
{
  public class School
  {
    public string id { get; set; }

    public long country { get; set; }

    public long city { get; set; }

    public string name { get; set; }

    public long school_group_id { get; set; }

    public int year_from { get; set; }

    public int year_to { get; set; }

    public int year_graduated { get; set; }

    public string @class { get; set; }

    public string speciality { get; set; }

    public int type { get; set; }

    public string type_str { get; set; }
  }
}
