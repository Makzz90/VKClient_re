using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VKClient.Common.Framework;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPointSimpleAttachmentUC : MapAttachmentUCBase
  {
    private Uri _mapUri;
    internal Canvas canvas;
    internal Rectangle rectanglePlaceholder;
    internal Image imageMap;
    internal Image imageMapIcon;
    private bool _contentLoaded;

    public MapPointSimpleAttachmentUC()
    {
      this.InitializeComponent();
    }

    public static double CalculateTotalHeight(double width)
    {
      return MapAttachmentUCBase.GetMapHeight(width);
    }

    public override void OnReady()
    {
      double mapHeight = MapAttachmentUCBase.GetMapHeight(base.Width);
      this._mapUri = this.GetMapUri();
      ((FrameworkElement) this.canvas).Width=(base.Width);
      ((FrameworkElement) this.canvas).Height = mapHeight;
      ((FrameworkElement) this.rectanglePlaceholder).Width=(base.Width);
      ((FrameworkElement) this.rectanglePlaceholder).Height = mapHeight;
      ((FrameworkElement) this.imageMap).Width=(base.Width);
      ((FrameworkElement) this.imageMap).Height = mapHeight;
      Canvas.SetLeft((UIElement) this.imageMapIcon, base.Width / 2.0 - ((FrameworkElement) this.imageMapIcon).Width / 2.0);
      Canvas.SetTop((UIElement) this.imageMapIcon, mapHeight / 2.0 - ((FrameworkElement) this.imageMapIcon).Height);
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, this._mapUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap,  null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._mapUri !=  null) || !this._mapUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._mapUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPointSimpleAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) base.FindName("canvas");
      this.rectanglePlaceholder = (Rectangle) base.FindName("rectanglePlaceholder");
      this.imageMap = (Image) base.FindName("imageMap");
      this.imageMapIcon = (Image) base.FindName("imageMapIcon");
    }
  }
}
