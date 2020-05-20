using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public sealed class MenuItemViewModel : ViewModelBase
  {
    private readonly MenuSectionName _attachedSection;
    private SolidColorBrush _titleForeground;
    private SolidColorBrush _iconForeground;

    public string Title
    {
      get
      {
        switch (this._attachedSection)
        {
          case MenuSectionName.News:
            return CommonResources.MainMenu_News;
          case MenuSectionName.Notifications:
            return CommonResources.MainMenu_Notifications;
          case MenuSectionName.Messages:
            return CommonResources.MainMenu_Messages;
          case MenuSectionName.Friends:
            return CommonResources.MainMenu_Friends;
          case MenuSectionName.Communities:
            return CommonResources.MainMenu_Communities;
          case MenuSectionName.Photos:
            return CommonResources.MainMenu_Photos;
          case MenuSectionName.Videos:
            return CommonResources.MainMenu_Videos;
          case MenuSectionName.Audios:
            return CommonResources.MainMenu_Audios;
          case MenuSectionName.Games:
            return CommonResources.MainMenu_Games;
          case MenuSectionName.Bookmarks:
            return CommonResources.MainMenu_Bookmarks;
          case MenuSectionName.Settings:
            return CommonResources.MainMenu_Settings;
          default:
            return "";
        }
      }
    }

    public string Icon
    {
      get
      {
        return string.Format("../Resources/MainMenu/{0}32px.png", this._attachedSection.ToString("G"));
      }
    }

    public SolidColorBrush TitleForeground
    {
      get
      {
        return this._titleForeground;
      }
      set
      {
        this._titleForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>(() => this.TitleForeground);
      }
    }

    public SolidColorBrush IconForeground
    {
      get
      {
        return this._iconForeground;
      }
      set
      {
        this._iconForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>(() => this.IconForeground);
      }
    }

    public int Count
    {
      get
      {
        switch (this._attachedSection)
        {
          case MenuSectionName.Notifications:
            return CountersManager.Current.Counters.notifications;
          case MenuSectionName.Messages:
            return CountersManager.Current.Counters.messages;
          case MenuSectionName.Friends:
            return CountersManager.Current.Counters.friends;
          case MenuSectionName.Communities:
            return CountersManager.Current.Counters.groups;
          case MenuSectionName.Games:
            return CountersManager.Current.Counters.app_requests;
          default:
            return 0;
        }
      }
    }

    public string CountString
    {
      get
      {
        return MenuItemViewModel.FormatForUI(this.Count);
      }
    }

    public Visibility CountVisibility
    {
      get
      {
        if (this.Count <= 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public MenuItemViewModel(MenuSectionName attachedSection)
    {
      this._attachedSection = attachedSection;
      this.UpdateCount();
      this.UpdateSelectionState(MenuSectionName.Unknown);
    }

    public void UpdateCount()
    {
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.Count);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.CountString);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.CountVisibility);
    }

    public static string FormatForUI(int count)
    {
      if (count <= 0)
        return "";
      return UIStringFormatterHelper.FormatForUIShort((long) count);
    }

    public void UpdateSelectionState(MenuSectionName selectedSection)
    {
      if (selectedSection == this._attachedSection)
      {
        this.TitleForeground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 115, (byte) 168, (byte) 230));
        this.IconForeground = this.TitleForeground;
      }
      else
      {
        this.TitleForeground = new SolidColorBrush(Colors.White);
        this.IconForeground = (SolidColorBrush) Application.Current.Resources["PhoneMainMenuIconsBrush"];
      }
    }
  }
}
