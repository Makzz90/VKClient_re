using System;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.VirtItems
{
  public class UCItem : VirtualizableItemBase, ISupportPositionTracking
  {
    private readonly Func<UserControlVirtualizable> _getUserControlFunc;
    private readonly Func<double> _getUserControlHeightFunc;
    private readonly Action<UserControlVirtualizable> _releaseResourcesCallback;
    private UserControlVirtualizable _uc;
    private readonly double _width;
    private readonly double _landscapeWidth;
    private bool _isLandscape;

    public UserControlVirtualizable UC
    {
      get
      {
        return this._uc;
      }
    }

    public bool IsLandscape
    {
      get
      {
        return this._isLandscape;
      }
      set
      {
        if (this._isLandscape == value)
          return;
        this._isLandscape = value;
        this.Width = this._isLandscape ? this._landscapeWidth : this._width;
        this.UpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._getUserControlHeightFunc();
      }
    }

    public UCItem(double width, Thickness margin, Func<UserControlVirtualizable> getUserControlFunc, Func<double> getUserControlHeightFunc, Action<UserControlVirtualizable> releaseResourcesCallback = null, double landscapeWidth = 0.0, bool isLandscape = false)
      : base(width, margin, new Thickness())
    {
      this._getUserControlFunc = getUserControlFunc;
      this._getUserControlHeightFunc = getUserControlHeightFunc;
      this._releaseResourcesCallback = releaseResourcesCallback;
      this._width = width;
      this._landscapeWidth = landscapeWidth;
      this._isLandscape = isLandscape;
      if (!this._isLandscape)
        return;
      this.Width = this._landscapeWidth;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this._uc = this._getUserControlFunc();
      ((FrameworkElement) this._uc).Width = this.Width;
      this._uc.OnReady();
      this.Children.Add((FrameworkElement) this._uc);
    }

    protected override void LoadFullyNonVirtualizableItems()
    {
      base.LoadFullyNonVirtualizableItems();
      this._uc.LoadFullyNonVirtualizableItems();
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this._uc.ReleaseResources();
      if (this._releaseResourcesCallback != null)
        this._releaseResourcesCallback(this._uc);
      this._uc =  null;
    }

    protected override void ShownOnScreen()
    {
      base.ShownOnScreen();
      this._uc.ShownOnScreen();
    }

    public void TrackPositionChanged(Rect bounds, double offset)
    {
      ISupportPositionTracking uc = this._uc as ISupportPositionTracking;
      if (uc == null)
        return;
      Rect bounds1 = bounds;
      double offset1 = offset;
      uc.TrackPositionChanged(bounds1, offset1);
    }
  }
}
