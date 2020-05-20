using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
  public sealed class LinkCreationViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private string _address = "";
    private string _description = "";
    private string _pageTitle = "";
    private readonly long _communityId;
    private readonly GroupLink _link;
    private bool _isAddressReadOnly;
    private SolidColorBrush _addressForeground;

    public bool IsFormCompleted
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this.Address) && !string.IsNullOrWhiteSpace(this.Description) && this.Description.Length >= 2;
      }
    }

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
        this.NotifyPropertyChanged<bool>((() => this.IsAddressEnabled));
      }
    }

    public string Address
    {
      get
      {
        return this._address;
      }
      set
      {
        this._address = value;
        this.NotifyPropertyChanged<string>((() => this.Address));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public string Description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = value;
        this.NotifyPropertyChanged<string>((() => this.Description));
      }
    }

    public string PageTitle
    {
      get
      {
        return this._pageTitle;
      }
      set
      {
        this._pageTitle = value.ToUpper();
        this.NotifyPropertyChanged<string>((() => this.PageTitle));
      }
    }

    public bool IsAddressReadOnly
    {
      get
      {
        return this._isAddressReadOnly;
      }
      set
      {
        this._isAddressReadOnly = value;
        this.NotifyPropertyChanged<bool>((() => this.IsAddressReadOnly));
        this.NotifyPropertyChanged<bool>((() => this.IsAddressEnabled));
      }
    }

    public SolidColorBrush AddressForeground
    {
      get
      {
        return this._addressForeground;
      }
      set
      {
        this._addressForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.AddressForeground));
      }
    }

    public bool IsAddressEnabled
    {
      get
      {
        if (!this.IsAddressReadOnly)
          return this.IsFormEnabled;
        return true;
      }
    }

    public LinkCreationViewModel(long communityId, GroupLink link)
    {
      this._communityId = communityId;
      this._link = link;
      if (link == null)
      {
        this.PageTitle = CommonResources.LinkAdding;
        this.AddressForeground = (SolidColorBrush) Application.Current.Resources["PhoneContrastTitleBrush"];
        this.IsAddressReadOnly = false;
      }
      else
      {
        this.PageTitle = CommonResources.LinkEditing;
        this.AddressForeground = (SolidColorBrush) Application.Current.Resources["PhoneCommunityManagementSectionIconBrush"];
        this.IsAddressReadOnly = true;
        if (link.edit_title == 1)
        {
          this.Address = Extensions.ForUI(link.desc);
          this.Description = Extensions.ForUI(link.name);
        }
        else
        {
          this.Address = Extensions.ForUI(link.name);
          this.Description = Extensions.ForUI(link.desc);
        }
      }
    }

    public void AddEditLink()
    {
      this.SetInProgress(true, "");
      this.IsFormEnabled = false;
      if (this._link == null)
      {
        string url = this.Address;
        if (!url.Contains("://"))
          url = (url.Contains("vk.com") || url.Contains("vkontakte.ru") || url.Contains("vk.cc") ? "https://" : "http://") + url;
        GroupsService.Current.AddLink(this._communityId, url, this.Description, (Action<BackendResult<GroupLink, ResultCode>>) (result => this.AddEditCallback(result.ResultCode, result.Error, result.ResultData)));
      }
      else
        GroupsService.Current.EditLink(this._communityId, this._link.id, this.Description, (Action<BackendResult<int, ResultCode>>) (result => this.AddEditCallback(result.ResultCode, result.Error,  null)));
    }

    private void AddEditCallback(ResultCode resultCode, VKRequestsDispatcher.Error error, GroupLink resultData)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (resultCode == ResultCode.Succeeded)
            {
                if (this._link != null)
                {
                    if (this._link.edit_title == 1)
                        this._link.name = this.Description;
                    else
                        this._link.desc = this.Description;
                }
                EventAggregator current = EventAggregator.Current;
                CommunityLinkAddedOrEdited linkAddedOrEdited = new CommunityLinkAddedOrEdited();
                linkAddedOrEdited.CommunityId = this._communityId;
                int num = this._link != null ? 1 : 0;
                linkAddedOrEdited.IsEditing = num != 0;
                GroupLink groupLink = this._link ?? resultData;
                linkAddedOrEdited.Link = groupLink;
                current.Publish((object)linkAddedOrEdited);
                Navigator.Current.GoBack();
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                VKRequestsDispatcher.Error error1 = error;
                switch (error1 != null ? error1.error_text : (string)null)
                {
                    case null:
                        GenericInfoUC.ShowBasedOnResult((int)resultCode, "", error);
                        break;
                    default:
                        error.error_text += ".";
                        goto case null;
                }
            }
        }));
    }
  }
}
