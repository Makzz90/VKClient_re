using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Backend.DataObjects
{
  public class Relative
  {
    public long uid
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

    public string name { get; set; }

    public RelativeType type { get; set; }

    public string GetVKFormatted(List<User> users)
    {
      string str = "";
      if (string.IsNullOrEmpty(this.name))
      {
        User user = users == null ?  null : users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == this.id));
        if (user != null)
          str = user.Name;
      }
      else
        str = this.name;
      if (this.id <= 0L)
        return str;
      return string.Format("[id{0}|{1}]", this.id, str);
    }
  }
}
