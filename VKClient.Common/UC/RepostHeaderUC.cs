using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class RepostHeaderUC : UserControlVirtualizable
  {
    public const double FIXED_HEIGHT = 56.0;
    private Action _callbackTap;
    internal Grid gridRoot;
    internal Image imageUserOrGroup;
    internal TextBlock textBlockUserOrGroupName;
    internal TextBlock textBlockDate;
    internal Border postSourceBorder;
    private bool _contentLoaded;

    public RepostHeaderUC()
    {
      this.InitializeComponent();
    }

    public void Configure(WallRepostInfo configuration, Action callbackTap)
    {
      if (configuration != null)
      {
        ImageLoader.SetUriSource(this.imageUserOrGroup, configuration.Pic);
        this.textBlockUserOrGroupName.Text = configuration.Name;
        this.textBlockDate.Text = configuration.Subtitle;
        TextBlock blockUserOrGroupName = this.textBlockUserOrGroupName;
        double width = configuration.Width;
        Thickness margin = ((FrameworkElement) this.textBlockUserOrGroupName).Margin;
        // ISSUE: explicit reference operation
        double left = ((Thickness) @margin).Left;
        double maxWidth = width - left;
        blockUserOrGroupName.CorrectText(maxWidth);
        string iconUri = configuration.PostSourcePlatform.GetIconUri();
        if (!string.IsNullOrEmpty(iconUri))
        {
          ((UIElement) this.postSourceBorder).Visibility = Visibility.Visible;
          ImageBrush imageBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(imageBrush, iconUri);
          ((UIElement) this.postSourceBorder).OpacityMask=((Brush) imageBrush);
        }
        else
          ((UIElement) this.postSourceBorder).Visibility = Visibility.Collapsed;
      }
      this._callbackTap = callbackTap;
      if (this._callbackTap == null)
        return;
      MetroInMotion.SetTilt((DependencyObject) this.gridRoot, 2.1);
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (this._callbackTap == null)
        return;
      this._callbackTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/RepostHeaderUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.imageUserOrGroup = (Image) base.FindName("imageUserOrGroup");
      this.textBlockUserOrGroupName = (TextBlock) base.FindName("textBlockUserOrGroupName");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
      this.postSourceBorder = (Border) base.FindName("postSourceBorder");
    }
  }
}
