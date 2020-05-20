namespace VKClient.Common.Backend.DataObjects
{
  public class University
  {
    public long id { get; set; }

    public long country { get; set; }

    public long city { get; set; }

    public string name { get; set; }

    public long university_group_id { get; set; }

    public long faculty { get; set; }

    public string faculty_name { get; set; }

    public long chair { get; set; }

    public string chair_name { get; set; }

    public int graduation { get; set; }

    public string education_form { get; set; }

    public string education_status { get; set; }
  }
}
