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
        private bool _isSubscribedToFrameReported;
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
            base.DataContext = (new ViewModelBase());
            base.SuppressOpenMenuTapArea = true;
            base.SuppressMenu = true;
            ((UIElement)this.ucBrushThickness).Visibility = Visibility.Collapsed;
            ((UIElement)this.ucBrushThickness).Opacity = 0.0;
            this.textBlockRecipientName.Text = ("");
            // ISSUE: method pointer
            base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)
                  {
                      this.drawCanvas.Height = (Application.Current.Host.Content.ActualHeight - this.gridRecipient.Height - this.panelControls.Height - FramePageUtils.SoftNavButtonsCurrentSize);
                      this.drawCanvas.Width = (Application.Current.Host.Content.ActualWidth);
                  });
            base.BackKeyPress += (delegate(object sender, CancelEventArgs args)
            {
                if (!this.GetCanNavigateBack())
                {
                    args.Cancel = (true);
                }
            });
        }

        private bool GetCanNavigateBack()
        {
            if (!this._graffitiDrawService.CanUndo || this._isSaving)
                return true;
            return MessageBox.Show(CommonResources.CancelGraffitiConsent, CommonResources.Confirmation, (MessageBoxButton)1) == MessageBoxResult.OK;
        }

        private void SubscribeToFrameReported()
        {
            if (this._isSubscribedToFrameReported)
                return;
            this._isSubscribedToFrameReported = true;
            // ISSUE: method pointer
            Touch.FrameReported += (new TouchFrameEventHandler(this.Touch_OnFrameReported));
        }

        private void UnsubscribeFromFrameReported()
        {
            if (!this._isSubscribedToFrameReported)
                return;
            this._isSubscribedToFrameReported = false;
            // ISSUE: method pointer
            Touch.FrameReported -= (new TouchFrameEventHandler(this.Touch_OnFrameReported));
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            SystemTray.IsVisible = false;
            this.SubscribeToFrameReported();
            AccelerometerHelper.Instance.IsActive = true;
            DeviceOrientationHelper.Instance.OrientationChanged += new EventHandler<DeviceOrientationChangedEventArgs>(this.OnOrientationChanged);
            if (!this._isInitialized)
            {
                User user = null;
                ChatExtended chat = null;
                IDictionary<string, string> queryString = base.NavigationContext.QueryString;
                string index1 = "UserOrChatId";
                long result1;
                long.TryParse(queryString[index1], out result1);
                string index2 = "IsChat";
                bool result2;
                bool.TryParse(queryString[index2], out result2);
                string index3 = "Title";
                this.textBlockRecipientName.Text = (HttpUtility.UrlDecode(queryString[index3]));
                this.textBlockRecipientName.CorrectText(480.0 - ((FrameworkElement)this.gridRecipientAvatar).Width - 32.0 - 12.0);
                this.AlignRecipientInfo();
                ConversationInfo parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("ConversationInfo") as ConversationInfo;
                if (parameterForIdAndReset != null)
                {
                    user = parameterForIdAndReset.User;
                    chat = parameterForIdAndReset.Chat;
                }
                this._viewModel = new GraffitiDrawViewModel(result1, result2, user, chat);
                base.DataContext = this._viewModel;
                this._viewModel.LoadHeaderInfo();
                this._graffitiDrawService.StrokeBrush = new SolidColorBrush(this.ucGraffitiPallete.CurrentColor);
                this._graffitiDrawService.StrokeThickness = this.ucBrushThickness.CurrentThickness;
                this.ucBrushThickness.SetFillColor(this.ucGraffitiPallete.CurrentColor);
                this._isInitialized = true;
            }
            this.HandleOrientationChange(DeviceOrientationHelper.Instance.CurrentOrientation);
            if (!base.State.ContainsKey("GraffitiPageState"))
                return;
            this.RestoreState(base.State["GraffitiPageState"] as GraffitiCacheData);
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.UnsubscribeFromFrameReported();
            AccelerometerHelper.Instance.IsActive = false;
            DeviceOrientationHelper.Instance.OrientationChanged -= new EventHandler<DeviceOrientationChangedEventArgs>(this.OnOrientationChanged);
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.ResetState();
            }
            else
            {
                GraffitiCacheData graffitiCacheData = this._graffitiDrawService.GraffitiCacheData;
                string str = this.ucGraffitiPallete.CurrentColor.ToString();
                graffitiCacheData.SelectedColorHex = str;
                int currentThickness = this.ucBrushThickness.CurrentThickness;
                graffitiCacheData.SelectedStrokeThickness = currentThickness;
                this.SaveState(this._graffitiDrawService.GraffitiCacheData);
            }
        }

        private void SaveState(GraffitiCacheData cacheData)
        {
            base.State["GraffitiPageState"] = cacheData;
        }

        private void RestoreState(GraffitiCacheData cacheData)
        {
            List<GraffitiCacheDataCurve> graffitiCacheDataCurveList = cacheData != null ? Enumerable.ToList<GraffitiCacheDataCurve>(cacheData.Curves) : null;
            if (graffitiCacheDataCurveList == null)
                return;
            List<GraffitiCacheDataCurve>.Enumerator enumerator = graffitiCacheDataCurveList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    GraffitiCacheDataCurve current = enumerator.Current;
                    List<Point> points = current.GetPoints();
                    string colorHex = current.ColorHex;
                    int strokeThickness = current.StrokeThickness;
                    this._graffitiDrawService.StrokeBrush = new SolidColorBrush(colorHex.ToColor());
                    this._graffitiDrawService.StrokeThickness = strokeThickness;
                    for (int index = 0; index < points.Count; ++index)
                        this.HandleTouchPoint(points[index], index == points.Count - 1);
                }
            }
            finally
            {
                enumerator.Dispose();
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
            base.State["GraffitiPageState"] = null;
        }

        private void OnOrientationChanged(object sender, DeviceOrientationChangedEventArgs e)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                this.HandleOrientationChange(e.CurrentOrientation);
            });
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
            GraffitiPreviewUC previewUc = this._previewUC;
            if (previewUc == null)
                return;
            int orientation = (int)this._orientation;
            previewUc.SetOrientation((DeviceOrientation)orientation);
        }

        private void AnimateControls()
        {
            RotateTransform renderTransform1 = ((UIElement)this.borderClose).RenderTransform as RotateTransform;
            RotateTransform renderTransform2 = ((UIElement)this.gridAttach).RenderTransform as RotateTransform;
            RotateTransform renderTransform3 = ((UIElement)this.borderUndo).RenderTransform as RotateTransform;
            RotateTransform renderTransform4 = ((UIElement)this.borderThickness).RenderTransform as RotateTransform;
            if (renderTransform1 == null || renderTransform2 == null || (renderTransform3 == null || renderTransform4 == null))
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
            animationInfo1.target = (DependencyObject)renderTransform1;
            animationInfo1.propertyPath = RotateTransform.AngleProperty;
            double angle1 = renderTransform1.Angle;
            animationInfo1.from = angle1;
            double num2 = (double)num1;
            animationInfo1.to = num2;
            int num3 = 200;
            animationInfo1.duration = num3;
            animInfoList.Add(animationInfo1);
            AnimationInfo animationInfo2 = new AnimationInfo();
            animationInfo2.target = (DependencyObject)renderTransform2;
            animationInfo2.propertyPath = RotateTransform.AngleProperty;
            double angle2 = renderTransform1.Angle;
            animationInfo2.from = angle2;
            double num4 = (double)num1;
            animationInfo2.to = num4;
            int num5 = 200;
            animationInfo2.duration = num5;
            animInfoList.Add(animationInfo2);
            AnimationInfo animationInfo3 = new AnimationInfo();
            animationInfo3.target = (DependencyObject)renderTransform3;
            animationInfo3.propertyPath = RotateTransform.AngleProperty;
            double angle3 = renderTransform1.Angle;
            animationInfo3.from = angle3;
            double num6 = (double)num1;
            animationInfo3.to = num6;
            int num7 = 200;
            animationInfo3.duration = num7;
            animInfoList.Add(animationInfo3);
            AnimationInfo animationInfo4 = new AnimationInfo();
            animationInfo4.target = (DependencyObject)renderTransform4;
            animationInfo4.propertyPath = RotateTransform.AngleProperty;
            double angle4 = renderTransform1.Angle;
            animationInfo4.from = angle4;
            double num8 = (double)num1;
            animationInfo4.to = num8;
            int num9 = 200;
            animationInfo4.duration = num9;
            animInfoList.Add(animationInfo4);
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private void AnimateRecipientInfo()
        {
            CompositeTransform renderTransform1 = ((UIElement)this.gridRecipientAvatar).RenderTransform as CompositeTransform;
            TranslateTransform renderTransform2 = ((UIElement)this.textBlockRecipientName).RenderTransform as TranslateTransform;
            if (renderTransform1 == null || renderTransform2 == null)
                return;
            int num1;
            double num2;
            double num3;
            double num4;
            switch (this._orientation)
            {
                case DeviceOrientation.LandscapeRight:
                    num1 = -90;
                    num2 = 240.0 - ((FrameworkElement)this.gridRecipientAvatar).Width / 2.0;
                    num3 = 240.0 - ((FrameworkElement)this.textBlockRecipientName).ActualWidth / 2.0;
                    num4 = 0.0;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    num1 = 90;
                    num2 = 240.0 - ((FrameworkElement)this.gridRecipientAvatar).Width / 2.0;
                    num3 = 240.0 - ((FrameworkElement)this.textBlockRecipientName).ActualWidth / 2.0;
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
            ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num5);
            IEasingFunction ieasingFunction1 = (IEasingFunction)cubicEase;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            AnimationInfo animationInfo1 = new AnimationInfo();
            animationInfo1.target = (DependencyObject)renderTransform1;
            animationInfo1.propertyPath = CompositeTransform.RotationProperty;
            double rotation = renderTransform1.Rotation;
            animationInfo1.from = rotation;
            double num6 = (double)num1;
            animationInfo1.to = num6;
            int num7 = 200;
            animationInfo1.duration = num7;
            IEasingFunction ieasingFunction2 = ieasingFunction1;
            animationInfo1.easing = ieasingFunction2;
            animInfoList.Add(animationInfo1);
            AnimationInfo animationInfo2 = new AnimationInfo();
            animationInfo2.target = (DependencyObject)renderTransform1;
            animationInfo2.propertyPath = CompositeTransform.TranslateXProperty;
            double translateX = renderTransform1.TranslateX;
            animationInfo2.from = translateX;
            double num8 = num2;
            animationInfo2.to = num8;
            int num9 = 200;
            animationInfo2.duration = num9;
            IEasingFunction ieasingFunction3 = ieasingFunction1;
            animationInfo2.easing = ieasingFunction3;
            animInfoList.Add(animationInfo2);
            AnimationInfo animationInfo3 = new AnimationInfo();
            animationInfo3.target = (DependencyObject)renderTransform2;
            animationInfo3.propertyPath = TranslateTransform.XProperty;
            double x = renderTransform2.X;
            animationInfo3.from = x;
            double num10 = num3;
            animationInfo3.to = num10;
            int num11 = 200;
            animationInfo3.duration = num11;
            IEasingFunction ieasingFunction4 = ieasingFunction1;
            animationInfo3.easing = ieasingFunction4;
            animInfoList.Add(animationInfo3);
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.textBlockRecipientName,
                propertyPath = UIElement.OpacityProperty,
                from = ((UIElement)this.textBlockRecipientName).Opacity,
                to = num4,
                duration = 200,
                easing = ieasingFunction1
            });
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private void AlignRecipientInfo()
        {
            double num = 240.0 - (((FrameworkElement)this.gridRecipientAvatar).Width + 12.0 + ((FrameworkElement)this.textBlockRecipientName).ActualWidth) / 2.0;
            this._recipientAvatarInitialLeft = num;
            this._recipientNameInitialLeft = num + ((FrameworkElement)this.gridRecipientAvatar).Width + 12.0;
            CompositeTransform renderTransform1 = ((UIElement)this.gridRecipientAvatar).RenderTransform as CompositeTransform;
            TranslateTransform renderTransform2 = ((UIElement)this.textBlockRecipientName).RenderTransform as TranslateTransform;
            if (renderTransform1 == null || renderTransform2 == null)
                return;
            renderTransform1.TranslateX = this._recipientAvatarInitialLeft;
            renderTransform1.TranslateY = 12.0;
            renderTransform2.X = this._recipientNameInitialLeft;
            renderTransform2.Y = 17.0;
        }

        private void Touch_OnFrameReported(object sender, TouchFrameEventArgs e)
        {
            int num1 = Math.Min(1, ((PresentationFrameworkCollection<TouchPoint>)e.GetTouchPoints((UIElement)this.drawCanvas)).Count);
            TouchPointCollection touchPoints = e.GetTouchPoints((UIElement)this.drawCanvas);
            for (int index = 0; index < num1; ++index)
            {
                TouchPoint touchPoint = ((PresentationFrameworkCollection<TouchPoint>)touchPoints)[index];
                Point position = touchPoint.Position;
                bool isPointInBounds = this.GetIsPointInBounds(position);
                position.Y = position.Y - 20.0;
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
            if (point.Y >= 0.0 && point.X >= 0.0 && point.Y <= ((FrameworkElement)this.drawCanvas).Height)
            {
                return point.X <= ((FrameworkElement)this.drawCanvas).Width;
            }
            return false;
        }

        private void HandleTouchPoint(Point touchPointPosition, bool isLastPoint = false)
        {
            System.Windows.Shapes.Path path = this._graffitiDrawService.HandleTouchPoint(touchPointPosition, isLastPoint);
            if (path == null)
                return;
            this.drawCanvas.Children.Add((UIElement)path);
        }

        private void UpdateUndoOpacity()
        {
            double num = this._graffitiDrawService.CanUndo ? 1.0 : 0.4;
            ((UIElement)this.borderUndo).Opacity = num;
            ((UIElement)this.gridAttach).Opacity = num;
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
            this._previewUC.SendButtonClickAction = (Action)(async () =>
            {
                graffitiSent = true;
                string uriString = string.Format("/{0}", (object)Guid.NewGuid());
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
                    ParametersRepository.SetParameterForId("Graffiti", (object)new GraffitiAttachmentItem(uriString, width, height));
                    GC.Collect();
                    Navigator.Current.GoBack();
                }
            });
            this._previewUC.HideCallback = (Action)(() =>
            {
                this._isSaving = false;
                if (graffitiSent)
                    return;

                this.SubscribeToFrameReported();
            });
            this.UnsubscribeFromFrameReported();
            this._previewUC.Show(bitmap, this._orientation);
        }

        private WriteableBitmap CreateRenderBitmap()
        {
            Canvas renderCanvas = this.CreateRenderCanvas();
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)((FrameworkElement)renderCanvas).Width, (int)((FrameworkElement)renderCanvas).Height);
            Canvas canvas = renderCanvas;
            TranslateTransform translateTransform = new TranslateTransform();
            writeableBitmap.Render((UIElement)canvas, (Transform)translateTransform);
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
                double num7 = (double)curveData.StrokeThickness / 2.0;
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
            canvas1.Background = (Brush)solidColorBrush;
            Canvas canvas2 = canvas1;
            foreach (CurveData curveData in this._graffitiDrawService.CurvesData)
            {
                List<Point> list = curveData.Points.Select<Point, Point>((Func<Point, Point>)(point => new Point((point.X - minX) * resizeFactor, (point.Y - minY) * resizeFactor))).ToList<Point>();
                if (list.Count != 0)
                {
                    double lineStrokeThickness = (double)curveData.StrokeThickness * resizeFactor;
                    System.Windows.Shapes.Path path = GraffitiDrawPage.CreatePath((IReadOnlyList<Point>)list, lineStrokeThickness, curveData.StrokeBrush);
                    if (path != null)
                        canvas2.Children.Add((UIElement)path);
                }
            }
            return canvas2;
        }

        private static System.Windows.Shapes.Path CreatePath(IReadOnlyList<Point> controlPoints, double lineStrokeThickness, Brush strokeBrush)
        {
            if (controlPoints.Count == 0)
                return (System.Windows.Shapes.Path)null;
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

        private void Undo_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
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
                foreach (System.Windows.Shapes.Path path in (List<System.Windows.Shapes.Path>)curve)
                {
                    if (this.drawCanvas.Children.Contains((UIElement)path))
                        this.drawCanvas.Children.Remove((UIElement)path);
                }
                this.UpdateUndoOpacity();
                this._isUndoing = false;
            }
        }

        private void Undo_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._isUndoing)
                return;
            this._isUndoing = true;
            this._graffitiDrawService.Clear();
            ((PresentationFrameworkCollection<UIElement>)((Panel)this.drawCanvas).Children).Clear();
            this.UpdateUndoOpacity();
            this._isUndoing = false;
        }

        private void Close_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!this.GetCanNavigateBack())
                return;
            Navigator.Current.GoBack();
        }

        private void BrushThickness_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ShowHideThicknessPopup(((UIElement)this.ucBrushThickness).Visibility != 0);
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
            TranslateTransform translateTransform = (TranslateTransform)this.ucBrushThickness.RenderTransform;
            CubicEase cubicEase = new CubicEase();
            int num1 = !show ? 1 : 0;
            cubicEase.EasingMode = (EasingMode)num1;
            IEasingFunction easingFunction1 = (IEasingFunction)cubicEase;
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
            animationInfo.target = (DependencyObject)translateTransform;
            animationInfo.propertyPath = (object)TranslateTransform.YProperty;
            double y = translateTransform.Y;
            animationInfo.from = y;
            double num4 = (double)num2;
            animationInfo.to = num4;
            int num5 = 200;
            animationInfo.duration = num5;
            IEasingFunction easingFunction2 = easingFunction1;
            animationInfo.easing = easingFunction2;
            animInfoList.Add(animationInfo);
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.ucBrushThickness,
                propertyPath = (object)UIElement.OpacityProperty,
                from = this.ucBrushThickness.Opacity,
                to = (double)num3,
                duration = 200,
                easing = easingFunction1
            });
            int? startTime = new int?();
            Action completed = (Action)(() =>
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
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiDrawPage.xaml", UriKind.Relative));
            this.drawCanvas = (Canvas)base.FindName("drawCanvas");
            this.gridRecipient = (Grid)base.FindName("gridRecipient");
            this.gridRecipientAvatar = (Grid)base.FindName("gridRecipientAvatar");
            this.textBlockRecipientName = (TextBlock)base.FindName("textBlockRecipientName");
            this.translateRecipientName = (TranslateTransform)base.FindName("translateRecipientName");
            this.textBlockEncodeElapsed = (TextBlock)base.FindName("textBlockEncodeElapsed");
            this.textBlockRenderImageSize = (TextBlock)base.FindName("textBlockRenderImageSize");
            this.textBlockRenderFileSize = (TextBlock)base.FindName("textBlockRenderFileSize");
            this.panelControls = (StackPanel)base.FindName("panelControls");
            this.gridPallete = (Grid)base.FindName("gridPallete");
            this.ucGraffitiPallete = (GraffitiPalleteUC)base.FindName("ucGraffitiPallete");
            this.borderThickness = (Border)base.FindName("borderThickness");
            this.borderClose = (Border)base.FindName("borderClose");
            this.gridAttach = (Grid)base.FindName("gridAttach");
            this.borderUndo = (Border)base.FindName("borderUndo");
            this.borderThicknessPopupOverlay = (Border)base.FindName("borderThicknessPopupOverlay");
            this.ucBrushThickness = (GraffitiBrushThicknessUC)base.FindName("ucBrushThickness");
        }
    }
}
