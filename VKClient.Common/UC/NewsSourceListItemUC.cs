using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsSourceListItemUC : UserControl
  {
    internal Border borderFadeOut;
    private bool _contentLoaded;

    public NewsSourceListItemUC()
    {
        this.InitializeComponent();
        this.borderFadeOut.Opacity = 0.0;
        this.Loaded += (RoutedEventHandler)((sender, args) =>
        {
            PickableNewsfeedSourceItemViewModel sourceItemViewModel = this.DataContext as PickableNewsfeedSourceItemViewModel;
            if (sourceItemViewModel == null || !sourceItemViewModel.FadeOutEnabled)
                return;
            this.borderFadeOut.Animate(1.0, 0.0, (object)UIElement.OpacityProperty, 2000, new int?(), null, null, false);
        });
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsSourceListItemUC.xaml", UriKind.Relative));
      this.borderFadeOut = (Border) base.FindName("borderFadeOut");
    }
  }
}
