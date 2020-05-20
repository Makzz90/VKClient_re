using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.UC;

using VKClient.Audio.Base.Extensions;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PickableNewsfeedSourceItemViewModel : ViewModelBase
  {
    private bool _isSelected;

    public PickableItem PickableItem { get; set; }

    public string Icon
    {
      get
      {
        long id = this.PickableItem.ID;
        switch (id)
        {
          case -101:
            return "/Resources/Camera32px.png";
          case -100:
            return "/Resources/Video32px.png";
          default:
            long num1 = id - -10L;
            long num2 = 3;
            if ((ulong) num1 <= (ulong) num2)
            {
              switch ((uint) num1)
              {
                case 0:
                  return "/Resources/Newsfeed32px.png";
                case 1:
                  return "/Resources/WallPost/PostShare.png";
                case 2:
                  return "/Resources/User32px.png";
                case 3:
                  return "/Resources/Users32px.png";
              }
            }
            return "/Resources/Lists32px.png";
        }
      }
    }

    public string Title
    {
      get
      {
        return this.PickableItem.Name.Capitalize();
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
        this.NotifyPropertyChanged("IsSelected");
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.Foreground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.IconBackground));
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.Opacity));
      }
    }

    public SolidColorBrush Foreground
    {
      get
      {
        if (!this._isSelected)
          return (SolidColorBrush) Application.Current.Resources["PhoneAlmostBlackBrush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      }
    }

    public double Opacity
    {
      get
      {
        return !this._isSelected ? 1.0 : 0.9;
      }
    }

    public SolidColorBrush IconBackground
    {
      get
      {
        if (!this._isSelected)
          return "#ffafbac7".GetColor();
        return (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      }
    }

    public bool IsTopNewsToggleChecked
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled;
      }
      set
      {
        AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled = value;
        this.NotifyPropertyChanged("IsTopNewsToggleChecked");
        EventAggregator.Current.Publish((object) new NewsfeedTopEnabledDisabledEvent());
      }
    }

    public bool FadeOutEnabled { get; set; }

    public bool FadeOutToggleEnabled { get; set; }

    public PickableNewsfeedSourceItemViewModel(PickableItem pickableItem)
    {
      this.PickableItem = pickableItem;
    }
  }
}
