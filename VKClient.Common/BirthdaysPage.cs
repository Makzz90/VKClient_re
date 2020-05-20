using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class BirthdaysPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC ucHeader;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    public BirthdaysPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      VKClient.Common.Library.BirthdaysViewModel birthdaysViewModel = new VKClient.Common.Library.BirthdaysViewModel();
      base.DataContext = birthdaysViewModel;
      birthdaysViewModel.BithdaysGroupsViewModel.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/BirthdaysPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
