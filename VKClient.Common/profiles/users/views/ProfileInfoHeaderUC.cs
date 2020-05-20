using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Users.Views
{
  public class ProfileInfoHeaderUC : UserControl
  {
    internal Image image;
    internal Grid gridScrim;
    internal Border borderOverlay;
    internal Rectangle rectBackground;
    private bool _contentLoaded;

    public event EventHandler<System.Windows.Input.GestureEventArgs> RootTap;

    public event EventHandler<System.Windows.Input.GestureEventArgs> AddPhotoTap;

    public ProfileInfoHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.borderOverlay.Visibility = Visibility.Visible;
      // ISSUE: method pointer
      base.Loaded+=(delegate(object sender, RoutedEventArgs args)
      {
          this.image.SetBinding(ImageLoader.UriSourceProperty, new Binding("ProfileImageUrl"));
      });
    }

    private void ImageProfile_OnImageOpened(object sender, RoutedEventArgs e)
    {
      this.gridScrim.Visibility = Visibility.Visible;
    }

    public void SetOverlayOpacity(double opacity)
    {
      this.rectBackground.Opacity = opacity;
      this.rectBackground.Visibility = (opacity == 0.0 ? Visibility.Collapsed : Visibility.Visible);
    }

    public void Unload()
    {
      ImageLoader.SetUriSource(this.image,  null);
    }

    public void Reload()
    {
      ((FrameworkElement) this.image).SetBinding(ImageLoader.UriSourceProperty, new Binding("ProfileImageUrl"));
    }

    private void GridRoot_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.RootTap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.RootTap(sender, e);
    }

    private void BorderAddPhoto_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.AddPhotoTap == null)
        return;
      e.Handled = true;
      // ISSUE: reference to a compiler-generated field
      this.AddPhotoTap(sender, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Users/Views/ProfileInfoHeaderUC.xaml", UriKind.Relative));
      this.image = (Image) base.FindName("image");
      this.gridScrim = (Grid) base.FindName("gridScrim");
      this.borderOverlay = (Border) base.FindName("borderOverlay");
      this.rectBackground = (Rectangle) base.FindName("rectBackground");
    }
  }
}
