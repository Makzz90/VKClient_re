using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;

namespace VKClient.Common.Shared.ImagePreview
{
  public class PreviewImageUC : UserControl
  {
    private string _previewUri;
    private bool _animationRan;
    internal Grid LayoutRoot;
    internal Rectangle rect;
    internal Image imagePreview;
    private bool _contentLoaded;

    public PreviewImageUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler(this.PreviewImageUC_Loaded));
    }

    private void PreviewImageUC_Loaded(object sender, RoutedEventArgs e)
    {
    }

    public void SetImageUri(string previewUri, BitmapImage originalImage = null)
    {
      if (!(this._previewUri != previewUri))
        return;
      this._previewUri = previewUri;
      if (!ImageCache.Current.HasImageInCache(previewUri) && originalImage != null)
        this.imagePreview.Source = ((ImageSource) originalImage);
      else
        ImageLoader.SetUriSource(this.imagePreview, previewUri);
      this.HandleImageOpened();
    }

    private void HandleImageOpened()
    {
      if (!this._animationRan)
        this.RunAnimation((Action) (() => this.EnsureBigPreview()));
      else
        this.EnsureBigPreview();
    }

    private void EnsureBigPreview()
    {
      if (!(this._previewUri != ImageLoader.GetUriSource(this.imagePreview)))
        return;
      ImageLoader.SetUriSource(this.imagePreview, this._previewUri);
    }

    private void RunAnimation(Action callback)
    {
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo1 = new AnimationInfo();
      animationInfo1.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo1.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleX = (((UIElement) this.imagePreview).RenderTransform as ScaleTransform).ScaleX;
      animationInfo1.from = scaleX;
      double num1 = 1.0;
      animationInfo1.to = num1;
      Transform renderTransform1 = ((UIElement) this.imagePreview).RenderTransform;
      animationInfo1.target = (DependencyObject) renderTransform1;
      // ISSUE: variable of the null type
      animationInfo1.propertyPath = ScaleTransform.ScaleXProperty;
      animInfoList.Add(animationInfo1);
      AnimationInfo animationInfo2 = new AnimationInfo();
      animationInfo2.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo2.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleY = (((UIElement) this.imagePreview).RenderTransform as ScaleTransform).ScaleY;
      animationInfo2.from = scaleY;
      double num2 = 1.0;
      animationInfo2.to = num2;
      Transform renderTransform2 = ((UIElement) this.imagePreview).RenderTransform;
      animationInfo2.target = (DependencyObject) renderTransform2;
      // ISSUE: variable of the null type
      animationInfo2.propertyPath = ScaleTransform.ScaleYProperty;
      animInfoList.Add(animationInfo2);
      AnimationInfo animationInfo3 = new AnimationInfo();
      animationInfo3.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo3.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double opacity = ((UIElement) this.rect).Opacity;
      animationInfo3.from = opacity;
      double num3 = 0.4;
      animationInfo3.to = num3;
      Rectangle rect = this.rect;
      animationInfo3.target = (DependencyObject) rect;
      // ISSUE: variable of the null type
      animationInfo3.propertyPath = UIElement.OpacityProperty;
      animInfoList.Add(animationInfo3);
      int? startTime = new int?(0);
      Action completed = (Action) (() =>
      {
        this._animationRan = true;
        callback();
      });
      AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/ImagePreview/PreviewImageUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.rect = (Rectangle) base.FindName("rect");
      this.imagePreview = (Image) base.FindName("imagePreview");
    }
  }
}
