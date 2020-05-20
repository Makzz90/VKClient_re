using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Shared
{
  public class SectionHeaderViewModel : ViewModelBase
  {
    private int _count;
    private HeaderOption _selectedOption;

    public string Title { get; set; }

    public int Count
    {
      get
      {
        return this._count;
      }
      set
      {
        this._count = value;
        this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.Count));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.CountStr));
      }
    }

    public string CountStr
    {
      get
      {
        if (this.Count <= 0)
          return "";
        return UIStringFormatterHelper.FormatForUI((long) this.Count);
      }
    }

    public Visibility ShowAllVisibility { get; set; }

    public Visibility ShowOptionsVisibility { get; set; }

    public List<HeaderOption> HeaderOptions { get; set; }

    public HeaderOption SelectedOption
    {
      get
      {
        return this._selectedOption;
      }
      set
      {
        this._selectedOption = value;
        this.NotifyPropertyChanged<HeaderOption>((System.Linq.Expressions.Expression<Func<HeaderOption>>) (() => this.SelectedOption));
      }
    }

    public Action OnHeaderTap { get; set; }

    public void HandleTap()
    {
      if (this.OnHeaderTap == null)
        return;
      this.OnHeaderTap();
    }
  }
}
