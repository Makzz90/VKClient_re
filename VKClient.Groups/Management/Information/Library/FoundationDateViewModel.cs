using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Groups.Management.Information.Library
{
  public class FoundationDateViewModel : ViewModelBase
  {
    private Visibility _visibility;
    private string _title;
    private List<CustomListPickerItem> _availableYears;
    private List<CustomListPickerItem> _availableDays;
    private CustomListPickerItem _year;
    private CustomListPickerItem _month;
    private CustomListPickerItem _day;

    public InformationViewModel ParentViewModel { get; set; }

    public Visibility Visibility
    {
      get
      {
        return this._visibility;
      }
      set
      {
        this._visibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.Visibility));
      }
    }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.NotifyPropertyChanged<string>((() => this.Title));
      }
    }

    public List<CustomListPickerItem> AvailableYears
    {
      get
      {
        return this._availableYears;
      }
      set
      {
        this._availableYears = value;
        this.NotifyPropertyChanged<List<CustomListPickerItem>>((Expression<Func<List<CustomListPickerItem>>>) (() => this.AvailableYears));
      }
    }

    public List<CustomListPickerItem> AvailableMonths { get; set; }

    public List<CustomListPickerItem> AvailableDays
    {
      get
      {
        return this._availableDays;
      }
      set
      {
        this._availableDays = value;
        this.NotifyPropertyChanged<List<CustomListPickerItem>>((Expression<Func<List<CustomListPickerItem>>>) (() => this.AvailableDays));
      }
    }

    public CustomListPickerItem Year
    {
      get
      {
        return this._year;
      }
      set
      {
        this._year = value;
        this.NotifyPropertyChanged<CustomListPickerItem>((() => this.Year));
        this.UpdateAvailableDays();
      }
    }

    public CustomListPickerItem Month
    {
      get
      {
        return this._month;
      }
      set
      {
        this._month = value;
        this.NotifyPropertyChanged<CustomListPickerItem>((() => this.Month));
        this.UpdateAvailableDays();
      }
    }

    public CustomListPickerItem Day
    {
      get
      {
        return this._day;
      }
      set
      {
        this._day = value;
        this.NotifyPropertyChanged<CustomListPickerItem>((() => this.Day));
      }
    }

    public FoundationDateViewModel(InformationViewModel parentViewModel)
    {
      List<CustomListPickerItem> customListPickerItemList = new List<CustomListPickerItem>();
      CustomListPickerItem customListPickerItem = new CustomListPickerItem();
      customListPickerItem.Name = FoundationDateViewModel.HandleCase(CommonResources.NotDefined);
      int num1 = 1;
      customListPickerItem.IsUnknown = num1 != 0;
      long num2 = 0;
      customListPickerItem.Id = num2;
      customListPickerItemList.Add(customListPickerItem);
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfJanuary),
        Id = 1L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfFebruary),
        Id = 2L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfMarch),
        Id = 3L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfApril),
        Id = 4L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfMay),
        Id = 5L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfJune),
        Id = 6L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfJuly),
        Id = 7L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfAugust),
        Id = 8L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfSeptember),
        Id = 9L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfOctober),
        Id = 10L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfNovember),
        Id = 11L
      });
      customListPickerItemList.Add(new CustomListPickerItem()
      {
        Name = FoundationDateViewModel.HandleCase(CommonResources.OfDecember),
        Id = 12L
      });
      this.AvailableMonths = customListPickerItemList;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.ParentViewModel = parentViewModel;
    }

    public void Read(CommunitySettings information)
    {
        if (information.Type != GroupType.PublicPage)
        {
            this.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.Title = information.public_date_label;
            if (this.Title.EndsWith(":"))
                this.Title = this.Title.Substring(0, this.Title.Length - 1);
            List<CustomListPickerItem> customListPickerItemList = new List<CustomListPickerItem>();
            CustomListPickerItem customListPickerItem = new CustomListPickerItem();
            customListPickerItem.Name = CommonResources.NotDefined;
            customListPickerItem.IsUnknown = true;
            long num = 0;
            customListPickerItem.Id = num;
            customListPickerItemList.Add(customListPickerItem);
            this.AvailableYears = customListPickerItemList;
            for (int year = DateTime.Now.Year; year >= 1800; --year)
                this.AvailableYears.Add(new CustomListPickerItem()
                {
                    Name = year.ToString(),
                    Id = (long)year
                });
            string[] date = information.public_date.Split('.');
            this.Month = this.AvailableMonths[int.Parse(date[1])];
            if (date.Length > 2)
                this.Year = this.AvailableYears.FirstOrDefault<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(y => y.Id == (long)int.Parse(date[2])));
            if (this.Year == null)
                this.Year = this.AvailableYears.First<CustomListPickerItem>();
            this.UpdateAvailableDays();
            this.Day = this.AvailableDays[int.Parse(date[0])];
        }
    }

    private void UpdateAvailableDays()
    {
        CustomListPickerItem day = this.Day;
        List<CustomListPickerItem> customListPickerItemList = new List<CustomListPickerItem>();
        CustomListPickerItem customListPickerItem = new CustomListPickerItem();
        customListPickerItem.Name = CommonResources.NotDefined;
        customListPickerItem.IsUnknown = true;
        long num1 = 0;
        customListPickerItem.Id = num1;
        customListPickerItemList.Add(customListPickerItem);
        List<CustomListPickerItem> source = customListPickerItemList;
        int num2 = 31;
        if (this.Year != null && this.Month != null && (this.Year.Id != 0L && this.Month.Id != 0L))
            num2 = DateTime.DaysInMonth(Convert.ToInt32(this.Year.Id), Convert.ToInt32(this.Month.Id));
        for (int index = 1; index <= num2; ++index)
            source.Add(new CustomListPickerItem()
            {
                Name = index.ToString(),
                Id = (long)index
            });
        this.AvailableDays = source;
        this.Day = day == null || day.Id > (long)num2 ? source.First<CustomListPickerItem>() : source.First<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(d => d.Id == day.Id));
    }

    private static string HandleCase(string s)
    {
      if (!string.IsNullOrEmpty(s))
        return s[0].ToString().ToUpper() + s.Substring(1);
      if (s == null)
        return  null;
      return s.ToUpper();
    }
  }
}
