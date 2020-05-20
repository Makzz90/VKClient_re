using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti.Cache;
using VKClient.Common.Graffiti.ViewModels;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Graffiti.Views
{
  public class GraffitiDrawPage : PageBase
  {
    private readonly GraffitiDrawService _graffitiDrawService = new GraffitiDrawService();
    private const string PAGE_STATE_KEY = "GraffitiPageState";
    private const int ANIMATION_DURATION = 200;
    private bool _isInitialized;
    private GraffitiDrawViewModel _viewModel;
    private double _recipientAvatarInitialLeft;
    private double _recipientNameInitialLeft;
    private const int MAX_TOUCH_COUNT = 1;
    private const int RENDER_IMAGE_WIDTH = 720;
    private const int EXTRA_Y = 20;
    private GraffitiPreviewUC _previewUC;
    private bool _isDrawing;
    private bool _isUndoing;
    private DeviceOrientation _orientation;
    private bool _isSaving;
    private bool _isThicknessPopupAnimating;
    internal Canvas drawCanvas;
    internal Grid gridRecipient;
    internal Grid gridRecipientAvatar;
    internal TextBlock textBlockRecipientName;
    internal TranslateTransform translateRecipientName;
    internal TextBlock textBlockEncodeElapsed;
    internal TextBlock textBlockRenderImageSize;
    internal TextBlock textBlockRenderFileSize;
    internal StackPanel panelControls;
    internal Grid gridPallete;
    internal GraffitiPalleteUC ucGraffitiPallete;
    internal Border borderThickness;
    internal Border borderClose;
    internal Grid gridAttach;
    internal Border borderUndo;
    internal Border borderThicknessPopupOverlay;
    internal GraffitiBrushThicknessUC ucBrushThickness;
    private bool _contentLoaded;

    public GraffitiDrawPage()
    {
      this.InitializeComponent();
      this.DataContext = (object) new ViewModelBase();
      this.SuppressOpenMenuTapArea = true;
      this.SuppressMenu = true;
      this.ucBrushThickness.Visibility = Visibility.Collapsed;
      this.ucBrushThickness.Opacity = 0.0;
      this.textBlockRecipientName.Text = "";
      this.SizeChanged += (SizeChangedEventHandler) ((sender, args) =>
      {
        this.drawCanvas.Height = Application.Current.Host.Content.ActualHeight - this.gridRecipient.Height - this.panelControls.Height - FramePageUtils.SoftNavButtonsCurrentSize;
        this.drawCanvas.Width = Application.Current.Host.Content.ActualWidth;
      });
      this.BackKeyPress += (EventHandler<CancelEventArgs>) ((sender, args) =>
      {
        if (this.GetCanNavigateBack())
          return;
        args.Cancel = true;
      });
    }

    private bool GetCanNavigateBack()
    {
      if (!this._graffitiDrawService.CanUndo)
        return true;
      return MessageBox.Show(CommonResources.CancelGraffitiConsent, CommonResources.Confirmation, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      SystemTray.IsVisible = false;
      Touch.FrameReported += new TouchFrameEventHandler(this.Touch_OnFrameReported);
      AccelerometerHelper.Instance.IsActive = true;
      DeviceOrientationHelper.Instance.OrientationChanged += new EventHandler<DeviceOrientationChangedEventArgs>(this.OnOrientationChanged);
      if (!this._isInitialized)
      {
        User user = (User) null;
        ChatExtended chat = (ChatExtended) null;
        IDictionary<string, string> queryString = this.NavigationContext.QueryString;
        string index1 = "UserOrChatId";
        long result1;
        long.TryParse(queryString[index1], out result1);
        string index2 = "IsChat";
        bool result2;
        bool.TryParse(queryString[index2], out result2);
        string index3 = "Title";
        this.textBlockRecipientName.Text = HttpUtility.UrlDecode(queryString[index3]);
        this.textBlockRecipientName.CorrectText(480.0 - this.gridRecipientAvatar.Width - 32.0 - 12.0);
        this.AlignRecipientInfo();
        ConversationInfo conversationInfo = ParametersRepository.GetParameterForIdAndReset("ConversationInfo") as ConversationInfo;
        if (conversationInfo != null)
        {
          user = conversationInfo.User;
          chat = conversationInfo.Chat;
        }
        this._viewModel = new GraffitiDrawViewModel(result1, result2, user, chat);
        this.DataContext = (object) this._viewModel;
        this._viewModel.LoadHeaderInfo();
        this._graffitiDrawService.StrokeBrush = new SolidColorBrush(this.ucGraffitiPallete.CurrentColor);
        this._graffitiDrawService.StrokeThickness = this.ucBrushThickness.CurrentThickness;
        this.ucBrushThickness.SetFillColor(this.ucGraffitiPallete.CurrentColor);
        this._isInitialized = true;
      }
      this.HandleOrientationChange(DeviceOrientationHelper.Instance.CurrentOrientation);
      if (!this.State.ContainsKey("GraffitiPageState"))
        return;
      this.RestoreState(this.State["GraffitiPageState"] as GraffitiCacheData);
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      Touch.FrameReported -= new TouchFrameEventHandler(this.Touch_OnFrameReported);
      AccelerometerHelper.Instance.IsActive = false;
      DeviceOrientationHelper.Instance.OrientationChanged -= new EventHandler<DeviceOrientationChangedEventArgs>(this.OnOrientationChanged);
      if (e.NavigationMode == NavigationMode.Back)
      {
        this.ResetState();
      }
      else
      {
        GraffitiCacheData graffitiCacheData = this._graffitiDrawService.GraffitiCacheData;
        string @string = this.ucGraffitiPallete.CurrentColor.ToString();
        graffitiCacheData.SelectedColorHex = @string;
        int currentThickness = this.ucBrushThickness.CurrentThickness;
        graffitiCacheData.SelectedStrokeThickness = currentThickness;
        this.SaveState(this._graffitiDrawService.GraffitiCacheData);
      }
    }

    private void SaveState(GraffitiCacheData cacheData)
    {
      this.State["GraffitiPageState"] = (object) cacheData;
    }

    private void RestoreState(GraffitiCacheData cacheData)
    {
      List<GraffitiCacheDataCurve> graffitiCacheDataCurveList = cacheData != null ? cacheData.Curves.ToList<GraffitiCacheDataCurve>() : (List<GraffitiCacheDataCurve>) null;
      if (graffitiCacheDataCurveList == null)
        return;
      foreach (GraffitiCacheDataCurve graffitiCacheDataCurve in graffitiCacheDataCurveList)
      {
        List<Point> points = graffitiCacheDataCurve.GetPoints();
        string colorHex = graffitiCacheDataCurve.ColorHex;
        int strokeThickness = graffitiCacheDataCurve.StrokeThickness;
        this._graffitiDrawService.StrokeBrush = new SolidColorBrush(colorHex.ToColor());
        this._graffitiDrawService.StrokeThickness = strokeThickness;
        for (int index = 0; index < points.Count; ++index)
          this.HandleTouchPoint(points[index], index == points.Count - 1);
      }
      string selectedColorHex = cacheData.SelectedColorHex;
      int selectedStrokeThickness = cacheData.SelectedStrokeThickness;
      this.ucGraffitiPallete.SetColor(selectedColorHex);
      this.ucBrushThickness.SetThickness(selectedStrokeThickness);
      this.ucBrushThickness.SetFillColor(selectedColorHex.ToColor());
      this.UpdateUndoOpacity();
    }

    private void ResetState()
    {
      this.State["GraffitiPageState"] = (object) null;
    }

    private void OnOrientationChanged(object sender, DeviceOrientationChangedEventArgs e)
    {
      Execute.ExecuteOnUIThread((Action) (() => this.HandleOrientationChange(e.CurrentOrientation)));
    }

    private void HandleOrientationChange(DeviceOrientation orientation)
    {
      this._orientation = orientation;
      this.AnimateControls();
      this.AnimateRecipientInfo();
      this.UpdatePreviewOrientation();
    }

    private void UpdatePreviewOrientation()
    {
      GraffitiPreviewUC graffitiPreviewUc = this._previewUC;
      if (graffitiPreviewUc == null)
        return;
      int num = (int) this._orientation;
      graffitiPreviewUc.SetOrientation((DeviceOrientation) num);
    }

    private void AnimateControls()
    {
      RotateTransform rotateTransform1 = this.borderClose.RenderTransform as RotateTransform;
      RotateTransform rotateTransform2 = this.gridAttach.RenderTransform as RotateTransform;
      RotateTransform rotateTransform3 = this.borderUndo.RenderTransform as RotateTransform;
      RotateTransform rotateTransform4 = this.borderThickness.RenderTransform as RotateTransform;
      if (rotateTransform1 == null || rotateTransform2 == null || (rotateTransform3 == null || rotateTransform4 == null))
        return;
      int num1 = 0;
      switch (this._orientation)
      {
        case DeviceOrientation.LandscapeRight:
          num1 = -90;
          break;
        case DeviceOrientation.LandscapeLeft:
          num1 = 90;
          break;
      }
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo1 = new AnimationInfo();
      animationInfo1.target = (DependencyObject) rotateTransform1;
      animationInfo1.propertyPath = (object) RotateTransform.AngleProperty;
      double angle1 = rotateTransform1.Angle;
      animationInfo1.from = angle1;
      double num2 = (double) num1;
      animationInfo1.to = num2;
      int num3 = 200;
      animationInfo1.duration = num3;
      animInfoList.Add(animationInfo1);
      AnimationInfo animationInfo2 = new AnimationInfo();
      animationInfo2.target = (DependencyObject) rotateTransform2;
      animationInfo2.propertyPath = (object) RotateTransform.AngleProperty;
      double angle2 = rotateTransform1.Angle;
      animationInfo2.from = angle2;
      double num4 = (double) num1;
      animationInfo2.to = num4;
      int num5 = 200;
      animationInfo2.duration = num5;
      animInfoList.Add(animationInfo2);
      AnimationInfo animationInfo3 = new AnimationInfo();
      animationInfo3.target = (DependencyObject) rotateTransform3;
      animationInfo3.propertyPath = (object) RotateTransform.AngleProperty;
      double angle3 = rotateTransform1.Angle;
      animationInfo3.from = angle3;
      double num6 = (double) num1;
      animationInfo3.to = num6;
      int num7 = 200;
      animationInfo3.duration = num7;
      animInfoList.Add(animationInfo3);
      AnimationInfo animationInfo4 = new AnimationInfo();
      animationInfo4.target = (DependencyObject) rotateTransform4;
      animationInfo4.propertyPath = (object) RotateTransform.AngleProperty;
      double angle4 = rotateTransform1.Angle;
      animationInfo4.from = angle4;
      double num8 = (double) num1;
      animationInfo4.to = num8;
      int num9 = 200;
      animationInfo4.duration = num9;
      animInfoList.Add(animationInfo4);
      int? startTime = new int?();
      AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
    }

    private void AnimateRecipientInfo()
    {
      CompositeTransform compositeTransform = this.gridRecipientAvatar.RenderTransform as CompositeTransform;
      TranslateTransform translateTransform = this.textBlockRecipientName.RenderTransform as TranslateTransform;
      if (compositeTransform == null || translateTransform == null)
        return;
      int num1;
      double num2;
      double num3;
      double num4;
      switch (this._orientation)
      {
        case DeviceOrientation.LandscapeRight:
          num1 = -90;
          num2 = 240.0 - this.gridRecipientAvatar.Width / 2.0;
          num3 = 240.0 - this.textBlockRecipientName.ActualWidth / 2.0;
          num4 = 0.0;
          break;
        case DeviceOrientation.LandscapeLeft:
          num1 = 90;
          num2 = 240.0 - this.gridRecipientAvatar.Width / 2.0;
          num3 = 240.0 - this.textBlockRecipientName.ActualWidth / 2.0;
          num4 = 0.0;
          break;
        default:
          num1 = 0;
          num2 = this._recipientAvatarInitialLeft;
          num3 = this._recipientNameInitialLeft;
          num4 = 1.0;
          break;
      }
      CubicEase cubicEase = new CubicEase();
      int num5 = 0;
      cubicEase.EasingMode = (EasingMode) num5;
      IEasingFunction easingFunction1 = (IEasingFunction) cubicEase;
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo1 = new AnimationInfo();
      animationInfo1.target = (DependencyObject) compositeTransform;
      animationInfo1.propertyPath = (object) CompositeTransform.RotationProperty;
      double rotation = compositeTransform.Rotation;
      animationInfo1.from = rotation;
      double num6 = (double) num1;
      animationInfo1.to = num6;
      int num7 = 200;
      animationInfo1.duration = num7;
      IEasingFunction easingFunction2 = easingFunction1;
      animationInfo1.easing = easingFunction2;
      animInfoList.Add(animationInfo1);
      AnimationInfo animationInfo2 = new AnimationInfo();
      animationInfo2.target = (DependencyObject) compositeTransform;
      animationInfo2.propertyPath = (object) CompositeTransform.TranslateXProperty;
      double translateX = compositeTransform.TranslateX;
      animationInfo2.from = translateX;
      double num8 = num2;
      animationInfo2.to = num8;
      int num9 = 200;
      animationInfo2.duration = num9;
      IEasingFunction easingFunction3 = easingFunction1;
      animationInfo2.easing = easingFunction3;
      animInfoList.Add(animationInfo2);
      AnimationInfo animationInfo3 = new AnimationInfo();
      animationInfo3.target = (DependencyObject) translateTransform;
      animationInfo3.propertyPath = (object) TranslateTransform.XProperty;
      double x = translateTransform.X;
      animationInfo3.from = x;
      double num10 = num3;
      animationInfo3.to = num10;
      int num11 = 200;
      animationInfo3.duration = num11;
      IEasingFunction easingFunction4 = easingFunction1;
      animationInfo3.easing = easingFunction4;
      animInfoList.Add(animationInfo3);
      animInfoList.Add(new AnimationInfo()
      {
        target = (DependencyObject) this.textBlockRecipientName,
        propertyPath = (object) UIElement.OpacityProperty,
        from = this.textBlockRecipientName.Opacity,
        to = num4,
        duration = 200,
        easing = easingFunction1
      });
      int? startTime = new int?();
      AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
    }

    private void AlignRecipientInfo()
    {
      double num = 240.0 - (this.gridRecipientAvatar.Width + 12.0 + this.textBlockRecipientName.ActualWidth) / 2.0;
      this._recipientAvatarInitialLeft = num;
      this._recipientNameInitialLeft = num + this.gridRecipientAvatar.Width + 12.0;
      CompositeTransform compositeTransform = this.gridRecipientAvatar.RenderTransform as CompositeTransform;
      TranslateTransform translateTransform = this.textBlockRecipientName.RenderTransform as TranslateTransform;
      if (compositeTransform == null || translateTransform == null)
        return;
      compositeTransform.TranslateX = this._recipientAvatarInitialLeft;
      compositeTransform.TranslateY = 12.0;
      translateTransform.X = this._recipientNameInitialLeft;
      translateTransform.Y = 17.0;
    }

    private void Touch_OnFrameReported(object sender, TouchFrameEventArgs e)
    {
      int num = Math.Min(1, e.GetTouchPoints((UIElement) this.drawCanvas).Count);
      TouchPointCollection touchPoints = e.GetTouchPoints((UIElement) this.drawCanvas);
      for (int index = 0; index < num; ++index)
      {
        TouchPoint touchPoint = touchPoints[index];
        Point position = touchPoint.Position;
        bool isPointInBounds = this.GetIsPointInBounds(position);
        position.Y -= 20.0;
        switch (touchPoint.Action)
        {
          case TouchAction.Down:
            if (isPointInBounds)
            {
              this._isDrawing = true;
              this.gridPallete.IsHitTestVisible = false;
              this.HandleTouchPoint(position, false);
              this.UpdateUndoOpacity();
              break;
            }
            break;
          case TouchAction.Move:
            if (this._isDrawing)
            {
              this.HandleTouchPoint(position, false);
              break;
            }
            break;
          case TouchAction.Up:
            if (this._isDrawing)
            {
              this.HandleTouchPoint(position, true);
              this._isDrawing = false;
              this.gridPallete.IsHitTestVisible = true;
              break;
            }
            break;
        }
      }
    }

    private bool GetIsPointInBounds(Point point)
    {
      if (point.Y >= 0.0 && point.X >= 0.0 && point.Y <= this.drawCanvas.Height)
        return point.X <= this.drawCanvas.Width;
      return false;
    }

    private void HandleTouchPoint(Point touchPointPosition, bool isLastPoint = false)
    {
      System.Windows.Shapes.Path path = this._graffitiDrawService.HandleTouchPoint(touchPointPosition, isLastPoint);
      if (path == null)
        return;
      this.drawCanvas.Children.Add((UIElement) path);
    }

    private void UpdateUndoOpacity()
    {
      double num = this._graffitiDrawService.CanUndo ? 1.0 : 0.4;
      this.borderUndo.Opacity = num;
      this.gridAttach.Opacity = num;
    }

    private void Attach_OnClick(object sender, RoutedEventArgs e)
    {
      if (!this._graffitiDrawService.CanUndo || this._isSaving)
        return;
      this._isSaving = true;
      WriteableBitmap bitmap = this.CreateRenderBitmap();
      int orientationRotateAngle = this.GetOrientationRotateAngle();
      if (orientationRotateAngle != 0)
        bitmap = bitmap.Rotate(orientationRotateAngle);
      this._previewUC = new GraffitiPreviewUC();
      this.UpdatePreviewOrientation();
      bool graffitiSent = false;
      this._previewUC.SendButtonClickAction = (Action) (async () =>
      {
        graffitiSent = true;
        string uriString = string.Format("/{0}", (object) Guid.NewGuid());
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        Stream responseStream = await new GraffitiEncoder(bitmap).Encode();
        if (responseStream == null)
        {
          this._isSaving = false;
        }
        else
        {
          ImageCache.Current.TrySetImageForUri(uriString, responseStream);
          responseStream.Close();
          this._isSaving = false;
          ParametersRepository.SetParameterForId("Graffiti", (object) new GraffitiAttachmentItem(uriString, width, height));
          GC.Collect();
          Navigator.Current.GoBack();
        }
      });
      this._previewUC.HideCallback = (Action) (() =>
      {
        this._isSaving = false;
        if (graffitiSent)
          return;
        Touch.FrameReported += new TouchFrameEventHandler(this.Touch_OnFrameReported);
      });
      Touch.FrameReported -= new TouchFrameEventHandler(this.Touch_OnFrameReported);
      this._previewUC.Show(bitmap, this._orientation);
    }

    private WriteableBitmap CreateRenderBitmap()
    {
      Canvas renderCanvas = this.CreateRenderCanvas();
      WriteableBitmap writeableBitmap = new WriteableBitmap((int) renderCanvas.Width, (int) renderCanvas.Height);
      Canvas canvas = renderCanvas;
      TranslateTransform translateTransform = new TranslateTransform();
      writeableBitmap.Render((UIElement) canvas, (Transform) translateTransform);
      writeableBitmap.Invalidate();
      return writeableBitmap;
    }

    private int GetOrientationRotateAngle()
    {
      int num = 0;
      switch (this._orientation)
      {
        case DeviceOrientation.LandscapeRight:
          num = 90;
          break;
        case DeviceOrientation.LandscapeLeft:
          num = 270;
          break;
      }
      return num;
    }

    private Canvas CreateRenderCanvas()
    {
      double actualWidth = this.drawCanvas.ActualWidth;
      double actualHeight = this.drawCanvas.ActualHeight;
      double minX = actualWidth;
      double minY = actualHeight;
      double num1 = 0.0;
      double num2 = 0.0;
      double num3 = 0.0;
      double num4 = 0.0;
      double num5 = 0.0;
      double num6 = 0.0;
      foreach (CurveData curveData in this._graffitiDrawService.CurvesData)
      {
        List<Point> points = curveData.Points;
        double num7 = (double) curveData.StrokeThickness / 2.0;
        if (points.Count != 0)
        {
          foreach (Point point in points)
          {
            double x = point.X;
            double y = point.Y;
            if (x - num7 < minX - num3)
            {
              minX = x;
              num3 = num7;
            }
            if (x + num7 > num1 + num5)
            {
              num1 = x;
              num5 = num7;
            }
            if (y - num7 < minY - num4)
            {
              minY = y;
              num4 = num7;
            }
            if (y + num4 > num2 + num6)
            {
              num2 = y;
              num6 = num7;
            }
          }
        }
      }
      minX -= num3 + 0.5;
      minY -= num4 + 0.5;
      double val1_1 = num1 + (num5 + 0.5);
      double val1_2 = num2 + (num6 + 0.5);
      minX = Math.Max(minX, 0.0);
      minY = Math.Max(minY, 0.0);
      double num8 = Math.Min(val1_1, this.drawCanvas.Width);
      double num9 = Math.Min(val1_2, this.drawCanvas.Height);
      Rect rect = new Rect(minX, minY, num8 - minX, num9 - minY);
      double num10 = Math.Max(rect.Width, rect.Height);
      double resizeFactor = actualWidth / num10;
      resizeFactor = Math.Min(resizeFactor, 2.0) * (720.0 / actualWidth);
      double num11 = rect.Width * resizeFactor;
      double num12 = rect.Height * resizeFactor;
      Canvas canvas1 = new Canvas();
      double num13 = num11;
      canvas1.Width = num13;
      double num14 = num12;
      canvas1.Height = num14;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      canvas1.Background = (Brush) solidColorBrush;
      Canvas canvas2 = canvas1;
      foreach (CurveData curveData in this._graffitiDrawService.CurvesData)
      {
        List<Point> list = curveData.Points.Select<Point, Point>((Func<Point, Point>) (point => new Point((point.X - minX) * resizeFactor, (point.Y - minY) * resizeFactor))).ToList<Point>();
        if (list.Count != 0)
        {
          double lineStrokeThickness = (double) curveData.StrokeThickness * resizeFactor;
          System.Windows.Shapes.Path path = GraffitiDrawPage.CreatePath((IReadOnlyList<Point>) list, lineStrokeThickness, curveData.StrokeBrush);
          if (path != null)
            canvas2.Children.Add((UIElement) path);
        }
      }
      return canvas2;
    }

    private static System.Windows.Shapes.Path CreatePath(IReadOnlyList<Point> controlPoints, double lineStrokeThickness, Brush strokeBrush)
    {
      if (controlPoints.Count == 0)
        return (System.Windows.Shapes.Path) null;
      PathFigure pathFigure = PathDataBuilder.CreatePathFigure(controlPoints[0]);
      double lineStrokeThickness1 = lineStrokeThickness;
      Brush strokeBrush1 = strokeBrush;
      System.Windows.Shapes.Path path = PathDataBuilder.CreatePath(pathFigure, lineStrokeThickness1, strokeBrush1);
      PathSegmentCollection segments = GraffitiDrawService.GetSegments(controlPoints);
      pathFigure.Segments = segments;
      return path;
    }

    private void OnColorSelected(object sender, Color color)
    {
      this._graffitiDrawService.StrokeBrush = new SolidColorBrush(color);
      this.ucBrushThickness.SetFillColor(color);
    }

    private void OnThicknessSelected(object sender, int thickness)
    {
      this._graffitiDrawService.StrokeThickness = thickness;
      this.ShowHideThicknessPopup(false);
    }

    private void Undo_OnTap(object sender, GestureEventArgs e)
    {
      if (this._isUndoing)
        return;
      this._isUndoing = true;
      Curve curve = this._graffitiDrawService.Undo();
      if (curve == null)
      {
        this._isUndoing = false;
      }
      else
      {
        foreach (System.Windows.Shapes.Path path in (List<System.Windows.Shapes.Path>) curve)
        {
          if (this.drawCanvas.Children.Contains((UIElement) path))
            this.drawCanvas.Children.Remove((UIElement) path);
        }
        this.UpdateUndoOpacity();
        this._isUndoing = false;
      }
    }

    private void Undo_OnHold(object sender, GestureEventArgs e)
    {
      if (this._isUndoing)
        return;
      this._isUndoing = true;
      this._graffitiDrawService.Clear();
      this.drawCanvas.Children.Clear();
      this.UpdateUndoOpacity();
      this._isUndoing = false;
    }

    private void Close_OnTap(object sender, GestureEventArgs e)
    {
      if (!this.GetCanNavigateBack())
        return;
      Navigator.Current.GoBack();
    }

    private void BrushThickness_OnTap(object sender, GestureEventArgs e)
    {
      this.ShowHideThicknessPopup(this.ucBrushThickness.Visibility != Visibility.Visible);
    }

    private void BorderThicknessPopupOverlay_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      this.ShowHideThicknessPopup(false);
    }

    private void ShowHideThicknessPopup(bool show)
    {
      if (this._isThicknessPopupAnimating)
        return;
      this._isThicknessPopupAnimating = true;
      TranslateTransform translateTransform = (TranslateTransform) this.ucBrushThickness.RenderTransform;
      CubicEase cubicEase = new CubicEase();
      int num1 = !show ? 1 : 0;
      cubicEase.EasingMode = (EasingMode) num1;
      IEasingFunction easingFunction1 = (IEasingFunction) cubicEase;
      int num2 = show ? -8 : 0;
      int num3 = show ? 1 : 0;
      if (show)
      {
        this.ucBrushThickness.Visibility = Visibility.Visible;
        this.borderThicknessPopupOverlay.Visibility = Visibility.Visible;
        Touch.FrameReported -= new TouchFrameEventHandler(this.Touch_OnFrameReported);
      }
      else
      {
        this.ucBrushThickness.IsHitTestVisible = false;
        this.borderThicknessPopupOverlay.Visibility = Visibility.Collapsed;
        Touch.FrameReported += new TouchFrameEventHandler(this.Touch_OnFrameReported);
      }
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo = new AnimationInfo();
      animationInfo.target = (DependencyObject) translateTransform;
      animationInfo.propertyPath = (object) TranslateTransform.YProperty;
      double y = translateTransform.Y;
      animationInfo.from = y;
      double num4 = (double) num2;
      animationInfo.to = num4;
      int num5 = 200;
      animationInfo.duration = num5;
      IEasingFunction easingFunction2 = easingFunction1;
      animationInfo.easing = easingFunction2;
      animInfoList.Add(animationInfo);
      animInfoList.Add(new AnimationInfo()
      {
        target = (DependencyObject) this.ucBrushThickness,
        propertyPath = (object) UIElement.OpacityProperty,
        from = this.ucBrushThickness.Opacity,
        to = (double) num3,
        duration = 200,
        easing = easingFunction1
      });
      int? startTime = new int?();
      Action completed = (Action) (() =>
      {
        this._isThicknessPopupAnimating = false;
        if (show)
          return;
        this.ucBrushThickness.IsHitTestVisible = true;
        this.ucBrushThickness.Visibility = Visibility.Collapsed;
      });
      AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiDrawPage.xaml", UriKind.Relative));
      this.drawCanvas = (Canvas) this.FindName("drawCanvas");
      this.gridRecipient = (Grid) this.FindName("gridRecipient");
      this.gridRecipientAvatar = (Grid) this.FindName("gridRecipientAvatar");
      this.textBlockRecipientName = (TextBlock) this.FindName("textBlockRecipientName");
      this.translateRecipientName = (TranslateTransform) this.FindName("translateRecipientName");
      this.textBlockEncodeElapsed = (TextBlock) this.FindName("textBlockEncodeElapsed");
      this.textBlockRenderImageSize = (TextBlock) this.FindName("textBlockRenderImageSize");
      this.textBlockRenderFileSize = (TextBlock) this.FindName("textBlockRenderFileSize");
      this.panelControls = (StackPanel) this.FindName("panelControls");
      this.gridPallete = (Grid) this.FindName("gridPallete");
      this.ucGraffitiPallete = (GraffitiPalleteUC) this.FindName("ucGraffitiPallete");
      this.borderThickness = (Border) this.FindName("borderThickness");
      this.borderClose = (Border) this.FindName("borderClose");
      this.gridAttach = (Grid) this.FindName("gridAttach");
      this.borderUndo = (Border) this.FindName("borderUndo");
      this.borderThicknessPopupOverlay = (Border) this.FindName("borderThicknessPopupOverlay");
      this.ucBrushThickness = (GraffitiBrushThicknessUC) this.FindName("ucBrushThickness");
    }
  }
}
