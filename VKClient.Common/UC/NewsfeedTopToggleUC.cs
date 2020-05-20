using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class NewsfeedTopToggleUC : UserControl, IHandle<NewsfeedTopEnabledDisabledEvent>, IHandle
  {
    internal Border borderFadeOut;
    private bool _contentLoaded;

    public event EventHandler ToggleControlTap;

    public NewsfeedTopToggleUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      EventAggregator.Current.Subscribe(this);
      ((UIElement) this.borderFadeOut).Opacity = 0.0;
      // ISSUE: method pointer
      base.Loaded+=(delegate(object sender, RoutedEventArgs args)
      {
          PickableNewsfeedSourceItemViewModel pickableNewsfeedSourceItemViewModel = base.DataContext as PickableNewsfeedSourceItemViewModel;
          if (pickableNewsfeedSourceItemViewModel != null && pickableNewsfeedSourceItemViewModel.FadeOutToggleEnabled)
          {
              this.borderFadeOut.Animate(1.0, 0.0, UIElement.OpacityProperty, 2000, default(int?), null, null);
          }
      });
    }

    private void ToggleTopNewsContainer_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    public void Handle(NewsfeedTopEnabledDisabledEvent message)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler toggleControlTap = this.ToggleControlTap;
      if (toggleControlTap == null)
        return;
      EventArgs empty = EventArgs.Empty;
      toggleControlTap(this, empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsfeedTopToggleUC.xaml", UriKind.Relative));
      this.borderFadeOut = (Border) base.FindName("borderFadeOut");
    }
  }
}
