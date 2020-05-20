using VKClient.Audio.Base.BLExtensions;

namespace VKClient.Common.Backend.DataObjects
{
  public class BirthDate
  {
      public int? Day { get; private set; }

      public int? Month { get; private set; }

      public int? Year { get; private set; }

      public bool IsToday { get; private set; }

      public bool IsTomorrow { get; private set; }

    public BirthDate(User user)
    {
      if (string.IsNullOrEmpty(user != null ? user.bdate :  null))
        return;
      string[] strArray = user.bdate.Split('.');
      if (strArray.Length > 1)
      {
        int result1;
        int.TryParse(strArray[0], out result1);
        int result2;
        int.TryParse(strArray[1], out result2);
        this.Day = new int?(result1);
        this.Month = new int?(result2);
        if (strArray.Length > 2)
        {
          int result3;
          int.TryParse(strArray[2], out result3);
          this.Year = new int?(result3);
        }
      }
      this.IsToday = user.IsBirthdayToday();
      this.IsTomorrow = user.IsBirthdayTomorrow();
    }
  }
}
