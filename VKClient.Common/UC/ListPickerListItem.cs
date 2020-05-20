using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.UC
{
  public class ListPickerListItem : INotifyPropertyChanged
  {
    private SolidColorBrush _foreground = new SolidColorBrush(Colors.Black);
    private bool _isSelected;

    public long Id { get; set; }

    public string Title { get; set; }

    public string TitleList { get; private set; }

    public string Prefix
    {
      set
      {
        this.TitleList = string.IsNullOrEmpty(value) ? this.Title : string.Format("{0} {1}", value, this.Title);
      }
    }

    public SolidColorBrush Foreground
    {
      get
      {
        return this._foreground;
      }
      set
      {
        this._foreground = value;
        this.OnPropertyChanged("Foreground");
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.OnPropertyChanged("IsSelected");
        this.Foreground = value ? Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"] as SolidColorBrush : new SolidColorBrush(Colors.Black);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ListPickerListItem(object fromObj)
    {
      this.Title = fromObj.ToString();
      this.TitleList = this.Title;
    }

    public override string ToString()
    {
      return this.Title;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
