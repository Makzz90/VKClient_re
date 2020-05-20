using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC.InplaceGifViewer
{
  public class InplaceGifViewerUC : UserControlVirtualizable, IHandleTap, ISupportPositionTracking
  {
    private string _currentlyAssignedFile = "";
    private static int _mpCount;
    private static Grid _gridWithAttachedPlayer;
    internal Grid LayoutRoot;
    internal GifOverlayUC gifOverlayUC;
    private bool _contentLoaded;

    public InplaceGifViewerViewModel VM
    {
      get
      {
        return base.DataContext as InplaceGifViewerViewModel;
      }
      set
      {
        if (this.VM != null)
          this.VM.PropertyChanged -= new PropertyChangedEventHandler(this.VM_PropertyChanged);
        base.DataContext = value;
        if (this.VM == null)
          return;
        this.VM.PropertyChanged += new PropertyChangedEventHandler(this.VM_PropertyChanged);
        this.UpdateVideoPlayer(false);
      }
    }

    public InplaceGifViewerUC()
    {
      this.InitializeComponent();
      // ISSUE: method pointer
      base.SizeChanged += (new SizeChangedEventHandler( this.InplaceGifViewerUC_SizeChanged));
    }

    private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "LocalVideoFile"))
        return;
      this.UpdateVideoPlayer(false);
    }

    private void UpdateVideoPlayer(bool force = false)
    {
      string file = this.VM.LocalVideoFilePath;
      if (this._currentlyAssignedFile == file && !force)
        return;
      if (InplaceGifViewerUC._gridWithAttachedPlayer != null)
      {
        ((PresentationFrameworkCollection<UIElement>) ((Panel) InplaceGifViewerUC._gridWithAttachedPlayer).Children).RemoveAt(0);
        InplaceGifViewerUC._gridWithAttachedPlayer =  null;
        --InplaceGifViewerUC._mpCount;
      }
      this._currentlyAssignedFile = file;
      if (string.IsNullOrWhiteSpace(file))
        return;
      if (this.VM.UseOldGifPlayer)
        base.Dispatcher.BeginInvoke((Action) (() =>
        {
          GifViewerUC gifViewerUc = new GifViewerUC();
          gifViewerUc.Init(file, this.VM.DocHeader.GetSize());
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this.LayoutRoot).Children).Insert(0, (UIElement) gifViewerUc);
          InplaceGifViewerUC._gridWithAttachedPlayer = this.LayoutRoot;
        }));
      else
        base.Dispatcher.BeginInvoke((Action) (() =>
        {
          string localVideoFile = this.VM.LocalVideoFile;
          if (string.IsNullOrEmpty(localVideoFile))
            return;
          FFmpegGifPlayerUC ffmpegGifPlayerUc1 = new FFmpegGifPlayerUC();
          int num = 0;
          ((UIElement) ffmpegGifPlayerUc1).IsHitTestVisible=(num != 0);
          FFmpegGifPlayerUC ffmpegGifPlayerUc2 = ffmpegGifPlayerUc1;
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this.LayoutRoot).Children).Insert(0, (UIElement) ffmpegGifPlayerUc2);
          InplaceGifViewerUC._gridWithAttachedPlayer = this.LayoutRoot;
          ++InplaceGifViewerUC._mpCount;
          ffmpegGifPlayerUc2.VideoUrl = localVideoFile;
        }));
    }

    private void InplaceGifViewerUC_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size previousSize1 = e.PreviousSize;
      // ISSUE: explicit reference operation
      if (((Size) @previousSize1).Width <= 0.0)
        return;
      Size previousSize2 = e.PreviousSize;
      // ISSUE: explicit reference operation
      if (((Size) @previousSize2).Height <= 0.0)
        return;
      Size previousSize3 = e.PreviousSize;
      // ISSUE: explicit reference operation
      double width1 = ((Size) @previousSize3).Width;
      Size newSize1 = e.NewSize;
      // ISSUE: explicit reference operation
      double width2 = ((Size) @newSize1).Width;
      if (width1 == width2)
      {
        Size previousSize4 = e.PreviousSize;
        // ISSUE: explicit reference operation
        double height1 = ((Size) @previousSize4).Height;
        Size newSize2 = e.NewSize;
        // ISSUE: explicit reference operation
        double height2 = ((Size) @newSize2).Height;
        if (height1 == height2)
          return;
      }
      this.UpdateVideoPlayer(true);
    }

    public override void ReleaseResources()
    {
      base.ReleaseResources();
      this.VM.ReleaseResorces();
    }

    public void OnTap()
    {
      this.VM.HandleTap();
    }

    public void TrackPositionChanged(Rect bounds, double offset)
    {
      double height = base.Height;
      double num1 = offset;
      double num2 = num1 + height;
      // ISSUE: explicit reference operation
      double y = ((Rect) @bounds).Y;
      // ISSUE: explicit reference operation
      double num3 = y + ((Rect) @bounds).Height;
      if (num2 < y || num1 > num3)
      {
        InplaceGifViewerViewModel vm = this.VM;
        if (vm == null)
          return;
        vm.Stop();
      }
      else
      {
        double num4 = num1 >= y ? (num2 <= num3 ? 100.0 : (num3 - num1) * 100.0 / height) : (num2 - y) * 100.0 / height;
        if (num4 > 60.0)
        {
          InplaceGifViewerViewModel vm = this.VM;
          if (vm == null)
            return;
          vm.HandleOnScreen();
        }
        else
        {
          if (num4 >= 20.0)
            return;
          InplaceGifViewerViewModel vm = this.VM;
          if (vm == null)
            return;
          vm.Stop();
        }
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/InplaceGifViewer/InplaceGifViewerUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.gifOverlayUC = (GifOverlayUC) base.FindName("gifOverlayUC");
    }
  }
}
