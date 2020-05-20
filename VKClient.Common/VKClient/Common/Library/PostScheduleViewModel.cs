using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PostScheduleViewModel : ViewModelBase
  {
    private const int DEFAULT_MINUTES_DELAY = 240;
    private const int MIN_MINUTES_INTERVAL = 5;
    private DateTime _postDate;
    private DateTime _postTime;
    private string _postDateStr;
    private string _postTimeStr;

    public DateTime PostDate
    {
      get
      {
        return this._postDate;
      }
      set
      {
        this._postDate = value;
        if (this.GetDateFromPicked() < DateTime.Now)
          this.InitWithDefaults(1);
        this.PostDateStr = this._postDate.ToString("dd MMMM yyyy");
        this.NotifyPropertyChanged("PostDate");
      }
    }

    public DateTime PostTime
    {
      get
      {
        return this._postTime;
      }
      set
      {
        this._postTime = value;
        if (this.GetDateFromPicked() < DateTime.Now)
          this.InitWithDefaults(1);
        this._postTime = PostScheduleViewModel.CorrectMinutes(this._postTime);
        this.PostTimeStr = this._postTime.ToString("HH:mm");
        this.NotifyPropertyChanged("PostTime");
      }
    }

    public string PostDateStr
    {
      get
      {
        return this._postDateStr;
      }
      set
      {
        this._postDateStr = value;
        this.NotifyPropertyChanged("PostDateStr");
      }
    }

    public string PostTimeStr
    {
      get
      {
        return this._postTimeStr;
      }
      set
      {
        this._postTimeStr = value;
        this.NotifyPropertyChanged("PostTimeStr");
      }
    }

    public PostScheduleViewModel(DateTime? dateTime = null)
    {
      if (dateTime.HasValue)
      {
        this._postDate = this._postTime = dateTime.Value;
        this.PostDate = this._postDate;
        this.PostTime = this._postTime;
      }
      else
        this.InitWithDefaults(240);
    }

    private DateTime GetDateFromPicked()
    {
      return new DateTime(this._postDate.Year, this._postDate.Month, this._postDate.Day, this._postTime.Hour, this._postTime.Minute, this._postTime.Second);
    }

    private static DateTime CorrectMinutes(DateTime dateTime)
    {
      int num = dateTime.Minute % 5;
      if (num > 0)
        dateTime = dateTime.AddMinutes((double) (5 - num));
      return dateTime;
    }

    public DateTime GetScheduledDateTime()
    {
      return this.GetDateFromPicked();
    }

    private void InitWithDefaults(int minutesDelay = 1)
    {
      DateTime dateTime = DateTime.Now;
      this._postTime = dateTime = dateTime.AddMinutes((double) minutesDelay);
      this._postDate = dateTime;
      this.PostDate = this._postDate;
      this.PostTime = this._postTime;
    }
  }
}
