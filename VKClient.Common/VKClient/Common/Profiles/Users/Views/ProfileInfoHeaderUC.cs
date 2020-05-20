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

    public event EventHandler<GestureEventArgs> RootTap;

    public event EventHandler<GestureEventArgs> AddPhotoTap;

    public ProfileInfoHeaderUC()
    {
      this.InitializeComponent();
      this.borderOverlay.Visibility = Visibility.Visible;
      this.Loaded += (RoutedEventHandler) ((sender, args) => this.image.SetBinding(ImageLoader.UriSourceProperty, new Binding("ProfileImageUrl")));
    }

    private void ImageProfile_OnImageOpened(object sender, RoutedEventArgs e)
    {
      this.gridScrim.Visibility = Visibility.Visible;
    }

    public void SetOverlayOpacity(double opacity)
    {
      this.rectBackground.Opacity = opacity;
      this.rectBackground.Visibility = opacity == 0.0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public void Unload()
    {
      ImageLoader.SetUriSource(this.image, null);
    }

    public void Reload()
    {
      this.image.SetBinding(ImageLoader.UriSourceProperty, new Binding("ProfileImageUrl"));
    }

    private void GridRoot_OnTap(object sender, GestureEventArgs e)
    {
      if (this.RootTap == null)
        return;
      this.RootTap(sender, e);
    }

    private void BorderAddPhoto_OnTap(object sender, GestureEventArgs e)
    {
      if (this.AddPhotoTap == null)
        return;
      e.Handled = true;
      this.AddPhotoTap(sender, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Users/Views/ProfileInfoHeaderUC.xaml", UriKind.Relative));
      this.image = (Image) this.FindName("image");
      this.gridScrim = (Grid) this.FindName("gridScrim");
      this.borderOverlay = (Border) this.FindName("borderOverlay");
      this.rectBackground = (Rectangle) this.FindName("rectBackground");
    }
  }
}
