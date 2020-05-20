using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKClient_Opus;
using Windows.Storage;

namespace VKClient.Common.UC
{
    public class AudioRecorderUC : UserControl
    {
        private AudioRecorderViewModel _viewModel;
        private bool _isOpened;
        private PageBase _currentPage;
        private bool _wasMenuSuppressed;
        private bool _disableHideAnimation;
        private AudioRecorderPreviewUC _previewUC;
        private readonly TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(240.0);
        private readonly Microphone _microphone;
        private readonly byte[] _buffer;
        private readonly OpusRuntimeComponent _opusComponent;
        private bool _isSent;
        private Point? _previousPoint;
        private const int SEND_RECORD_THRESHOLD_X = 160;
        private const int INITIAL_POSITION_THRESHOLD_Y = 80;
        private const int MIN_RECORD_DURATION_MILLISECONDS = 500;
        private bool _isAnimatingCancel;
        private bool _isInInitial;
        private bool _isInSendState;
        internal Grid gridRoot;
        internal Grid gridRecordOverlay;
        internal TranslateTransform translateRecordOverlay;
        internal TranslateTransform translateRecordDuration;
        internal Ellipse ellipseRec;
        internal StackPanel panelSlideToCancel;
        internal Border borderCancelButton;
        internal CompositeTransform transformButton;
        internal Ellipse ellipseVolume;
        internal ScaleTransform scaleVolume;
        internal Image borderStop;
        internal ScaleTransform scaleStop;
        internal Canvas borderSend;
        internal ScaleTransform scaleSend;
        internal Canvas borderCancel;
        private bool _contentLoaded;

        public bool IsOpened
        {
            get
            {
                return this._isOpened;
            }
            set
            {
                if (value == this._isOpened)
                    return;
                if (value)
                {
                    this.Open();
                    EventHandler opened = this.Opened;
                    if (opened == null)
                        return;
                    EventArgs empty = EventArgs.Empty;
                    opened(this, empty);
                }
                else
                {
                    this.Hide(!this._disableHideAnimation);
                    this._disableHideAnimation = false;
                    EventHandler closed = this.Closed;
                    if (closed == null)
                        return;
                    EventArgs empty = EventArgs.Empty;
                    closed(this, empty);
                }
            }
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public AudioRecorderUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            if (DesignerProperties.IsInDesignTool)
                return;
            if (!AudioRecorderHelper.CanRecord)
                return;
            try
            {
                this._microphone = Microphone.Default;
                this._microphone.BufferDuration = this._bufferDuration;
                this._buffer = new byte[this._microphone.GetSampleSizeInBytes(this._bufferDuration)];
                this._opusComponent = new OpusRuntimeComponent();
            }
            catch
            {
            }
        }

        private void Open()
        {
            this._isOpened = true;
            this._currentPage = FramePageUtils.CurrentPage;
            if (this._currentPage != null)
            {
                this._wasMenuSuppressed = this._currentPage.SuppressMenu;
                this._currentPage.SuppressMenu = true;
                this._currentPage.BackKeyPress += (new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress));
            }
            this._viewModel = new AudioRecorderViewModel(this._microphone, this._buffer, this._opusComponent, new Action<double>(this.DecibelValue_OnChanged), new Action(this.OnRecordingStopped), new Action(this.OnRecordingStoppedWithTimeout));
            base.DataContext = this._viewModel;
            this.StartRecording();
            this.UpdateVisibilityState();
            this.ResetValues();
            this.AnimateOverlay(true);
        }

        private void DecibelValue_OnChanged(double percentage)
        {
            double num = 0.7 + percentage * 0.5;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleVolume,
                propertyPath = ScaleTransform.ScaleXProperty,
                from = this.scaleVolume.ScaleX,
                to = num,
                duration = 200
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleVolume,
                propertyPath = ScaleTransform.ScaleYProperty,
                from = this.scaleVolume.ScaleY,
                to = num,
                duration = 200
            });
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private void OnRecordingStopped()
        {
            this.HideRecordingEllipse();
        }

        private void OnRecordingStoppedWithTimeout()
        {
            this.StopRecordingAndShowPreview();
        }

        private void HideRecordingEllipse()
        {
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleVolume,
                propertyPath = ScaleTransform.ScaleXProperty,
                from = this.scaleVolume.ScaleX,
                to = 0.0,
                duration = 200
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleVolume,
                propertyPath = ScaleTransform.ScaleYProperty,
                from = this.scaleVolume.ScaleY,
                to = 0.0,
                duration = 200
            });
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
            ((UIElement)this.ellipseRec).Opacity = 0.0;
        }

        private void Page_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;
            this.IsOpened = false;
            e.Cancel = true;
        }

        private void Hide(bool animated = true)
        {
            try
            {
                this._isOpened = false;
                if (this._currentPage != null)
                {
                    this._currentPage.SuppressMenu = this._wasMenuSuppressed;
                    this._currentPage.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress));
                }
                this.CancelRecording();
                AudioRecorderPreviewUC previewUc = this._previewUC;
                if (previewUc != null)
                    previewUc.Pause();
                if (this._previewUC != null && ((PresentationFrameworkCollection<UIElement>)((Panel)this.gridRoot).Children).Contains((UIElement)this._previewUC))
                    ((PresentationFrameworkCollection<UIElement>)((Panel)this.gridRoot).Children).Remove((UIElement)this._previewUC);
                if (animated)
                    this.AnimateOverlay(false);
                else
                    this.UpdateVisibilityState();
            }
            catch
            {
            }
        }

        private void UpdateVisibilityState()
        {
            base.Visibility = (this._isOpened ? Visibility.Visible : Visibility.Collapsed);
        }

        private void AnimateOverlay(bool show)
        {
            if (show)
            {
                List<AnimationInfo> animInfoList = new List<AnimationInfo>();
                AnimationInfo animationInfo1 = new AnimationInfo();
                animationInfo1.target = (DependencyObject)this.transformButton;
                animationInfo1.propertyPath = CompositeTransform.ScaleXProperty;
                animationInfo1.from = this.transformButton.ScaleX;
                animationInfo1.to = 1.0;
                animationInfo1.duration = 150;
                CubicEase cubicEase1 = new CubicEase();
                int num1 = 0;
                ((EasingFunctionBase)cubicEase1).EasingMode = ((EasingMode)num1);
                animationInfo1.easing = (IEasingFunction)cubicEase1;
                animInfoList.Add(animationInfo1);
                AnimationInfo animationInfo2 = new AnimationInfo();
                animationInfo2.target = (DependencyObject)this.transformButton;
                animationInfo2.propertyPath = CompositeTransform.ScaleYProperty;
                animationInfo2.from = this.transformButton.ScaleY;
                animationInfo2.to = 1.0;
                animationInfo2.duration = 150;
                CubicEase cubicEase2 = new CubicEase();
                int num2 = 0;
                ((EasingFunctionBase)cubicEase2).EasingMode = ((EasingMode)num2);
                animationInfo2.easing = (IEasingFunction)cubicEase2;
                animInfoList.Add(animationInfo2);
                AnimationInfo animationInfo3 = new AnimationInfo();
                animationInfo3.target = (DependencyObject)this.translateRecordOverlay;
                animationInfo3.propertyPath = TranslateTransform.XProperty;
                animationInfo3.from = this.translateRecordOverlay.X;
                animationInfo3.to = 0.0;
                animationInfo3.duration = 150;
                CubicEase cubicEase3 = new CubicEase();
                int num3 = 0;
                ((EasingFunctionBase)cubicEase3).EasingMode = ((EasingMode)num3);
                animationInfo3.easing = (IEasingFunction)cubicEase3;
                animInfoList.Add(animationInfo3);
                int? startTime = new int?();
                // ISSUE: variable of the null type

                AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
            }
            else
            {
                List<AnimationInfo> animInfoList = new List<AnimationInfo>();
                AnimationInfo animationInfo1 = new AnimationInfo();
                animationInfo1.target = (DependencyObject)this.transformButton;
                animationInfo1.propertyPath = CompositeTransform.ScaleXProperty;
                animationInfo1.from = this.transformButton.ScaleX;
                animationInfo1.to = 0.0;
                animationInfo1.duration = 150;
                CubicEase cubicEase1 = new CubicEase();
                int num1 = 0;
                ((EasingFunctionBase)cubicEase1).EasingMode = ((EasingMode)num1);
                animationInfo1.easing = (IEasingFunction)cubicEase1;
                animInfoList.Add(animationInfo1);
                AnimationInfo animationInfo2 = new AnimationInfo();
                animationInfo2.target = (DependencyObject)this.transformButton;
                animationInfo2.propertyPath = CompositeTransform.ScaleYProperty;
                animationInfo2.from = this.transformButton.ScaleY;
                animationInfo2.to = 0.0;
                animationInfo2.duration = 150;
                CubicEase cubicEase2 = new CubicEase();
                int num2 = 0;
                ((EasingFunctionBase)cubicEase2).EasingMode = ((EasingMode)num2);
                animationInfo2.easing = (IEasingFunction)cubicEase2;
                animInfoList.Add(animationInfo2);
                AnimationInfo animationInfo3 = new AnimationInfo();
                animationInfo3.target = (DependencyObject)this.translateRecordOverlay;
                animationInfo3.propertyPath = TranslateTransform.XProperty;
                animationInfo3.from = this.translateRecordOverlay.X;
                animationInfo3.to = base.ActualWidth;
                animationInfo3.duration = 150;
                CubicEase cubicEase3 = new CubicEase();
                int num3 = 1;
                ((EasingFunctionBase)cubicEase3).EasingMode = ((EasingMode)num3);
                animationInfo3.easing = (IEasingFunction)cubicEase3;
                animInfoList.Add(animationInfo3);
                int? startTime = new int?();
                Action completed = new Action(this.UpdateVisibilityState);
                AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
            }
        }

        private void ResetValues()
        {
            this._isSent = false;
            this._isInSendState = false;
            this._isInInitial = false;
            this._previousPoint = new Point?();
            ((UIElement)this.borderStop).Opacity = 1.0;
            this.scaleStop.ScaleX = 1.0;
            this.scaleStop.ScaleY = 1.0;
            ((UIElement)this.borderSend).Opacity = 0.0;
            this.scaleSend.ScaleX = 0.0;
            this.scaleSend.ScaleY = 0.0;
            this.transformButton.ScaleX = 0.0;
            this.transformButton.ScaleY = 0.0;
            this.transformButton.CenterX = 72.0;
            this.transformButton.TranslateX = 0.0;
            this.translateRecordDuration.X = 0.0;
            ((UIElement)this.borderCancel).Opacity = 0.0;
            this.scaleVolume.ScaleX = 0.8;
            this.scaleVolume.ScaleY = 0.8;
            ((Shape)this.ellipseVolume).Fill = ((Brush)Application.Current.Resources["PhoneBlue300_GrayBlue400Brush"]);
            ((UIElement)this.panelSlideToCancel).Opacity = 1.0;
            ((UIElement)this.borderCancelButton).Opacity = 0.0;
            this.translateRecordOverlay.X = (base.Width);
            ((UIElement)this.ellipseRec).Opacity = 1.0;
            if (this._previewUC == null)
                return;
            ((UIElement)this._previewUC).Visibility = Visibility.Collapsed;
        }

        private void StartRecording()
        {
            this._viewModel.StartRecording();
        }

        private void CancelRecording()
        {
            if (this._isSent)
                return;
            this._viewModel.Cancel();
        }

        private void StopRecording()
        {
            if (this._viewModel != null)//my bugfix
                this._viewModel.StopRecording();
        }

        private void StopRecordingAndSend()
        {
            this.StopRecording();
            this.Send();
        }

        public async void StopRecordingAndShowPreview()
        {
            this.StopRecording();
            this.ResetValues();
            StorageFile audioFileAsync = await this._viewModel.GetAudioFileAsync();
            if (audioFileAsync == null)
            {
                this._disableHideAnimation = true;
                this.IsOpened = false;
            }
            else
            {
                int recordDurationSeconds = this._viewModel.RecordDurationSeconds;
                List<int> waveform = this._viewModel.Waveform;
                this._previewUC = new AudioRecorderPreviewUC();
                ((PresentationFrameworkCollection<UIElement>)((Panel)this.gridRoot).Children).Add((UIElement)this._previewUC);
                this._previewUC.CancelTap = (Action)(() =>
                {
                    this._disableHideAnimation = true;
                    this.IsOpened = false;
                });
                this._previewUC.SendTap = new Action(this.Send);
                this._previewUC.SetData(audioFileAsync, recordDurationSeconds, waveform);
            }
        }

        private void Send_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._viewModel.RecordDuration > 500)
                this.StopRecordingAndSend();
            else
                this.IsOpened = false;
        }

        private void Send()
        {
            this._isSent = true;
            this._viewModel.Send();
            this.IsOpened = false;
        }

        private void Touch_OnFrameReported(object sender, TouchFrameEventArgs e)
        {
            TouchPointCollection touchPoints = e.GetTouchPoints((UIElement)this.gridRecordOverlay);
            if (((PresentationFrameworkCollection<TouchPoint>)touchPoints).Count == 0)
                return;
            TouchPoint touchPoint = ((PresentationFrameworkCollection<TouchPoint>)touchPoints)[0];
            Point position = touchPoint.Position;
            Point point1;
            if (!this._previousPoint.HasValue)
            {
                point1 = new Point(0.0, 0.0);
            }
            else
            {
                // ISSUE: explicit reference operation
                double x1 = ((Point)@position).X;
                Point point2 = this._previousPoint.Value;
                // ISSUE: explicit reference operation
                double x2 = ((Point)@point2).X;
                double num1 = x1 - x2;
                // ISSUE: explicit reference operation
                double y1 = ((Point)@position).Y;
                Point point3 = this._previousPoint.Value;
                // ISSUE: explicit reference operation
                double y2 = ((Point)@point3).Y;
                double num2 = y1 - y2;
                point1 = new Point(num1, num2);
            }
            Point point4 = point1;
            this._previousPoint = new Point?(position);
            // ISSUE: explicit reference operation
            double x = ((Point)@point4).X;
            TouchAction action = touchPoint.Action;
            if (action != TouchAction.Move)
            {
                if (action != TouchAction.Up || this._isInSendState)
                    return;
                // ISSUE: explicit reference operation
                if (((Point)@position).Y < -80.0 && !this._isInSendState)
                    this.GoToSendState();
                else if (this.transformButton.TranslateX > -160.0)
                {
                    if (this._viewModel.RecordDuration > 500)
                        this.StopRecordingAndSend();
                    else
                        this.IsOpened = false;
                }
                else
                {
                    if (this.transformButton.TranslateX > -16.0)
                        this.transformButton.CenterX = 44.0;
                    this.IsOpened = false;
                }
            }
            else
            {
                if (this._isInSendState)
                    return;
                // ISSUE: explicit reference operation
                if (((Point)@position).Y > -80.0)
                {
                    if (this._isInInitial)
                    {
                        this._isInInitial = false;
                        // ISSUE: explicit reference operation
                        this.MoveToPosition(((Point)@position).X);
                    }
                    else
                    {
                        CompositeTransform transformButton1 = this.transformButton;
                        double num1 = transformButton1.TranslateX + x;
                        transformButton1.TranslateX = num1;
                        if (this.transformButton.TranslateX > 0.0)
                            this.transformButton.TranslateX = 0.0;
                        else if (this.transformButton.TranslateX > -160.0)
                        {
                            this.ShowHideCancelOverlay(false);
                        }
                        else
                        {
                            this.ShowHideCancelOverlay(true);
                            CompositeTransform transformButton2 = this.transformButton;
                            double num2 = transformButton2.TranslateX - x / 2.0;
                            transformButton2.TranslateX = num2;
                        }
                        this.translateRecordDuration.X = this.transformButton.TranslateX;
                        ((UIElement)this.panelSlideToCancel).Opacity = (AudioRecorderUC.GetSlideToCancelOpacity(this.transformButton.TranslateX));
                    }
                }
                else
                    this.MoveToInitial();
            }
        }

        public void HandleManipulationStarted(Point manipulationOrigin)
        {
            // ISSUE: explicit reference operation
            this.MoveToPosition(((Point)@manipulationOrigin).X);
        }

        public void HandleManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (this._isInSendState)
                return;
            Point translation1 = e.CumulativeManipulation.Translation;
            Point translation2 = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            double x = ((Point)@translation2).X;
            // ISSUE: explicit reference operation
            if (Math.Abs(((Point)@translation1).Y) < 80.0)
            {
                if (this._isInInitial)
                {
                    this._isInInitial = false;
                    // ISSUE: explicit reference operation
                    this.MoveToPosition(((Point)@translation1).X);
                }
                else
                {
                    CompositeTransform transformButton1 = this.transformButton;
                    double num1 = transformButton1.TranslateX + x;
                    transformButton1.TranslateX = num1;
                    if (this.transformButton.TranslateX > 0.0)
                        this.transformButton.TranslateX = 0.0;
                    else if (this.transformButton.TranslateX > -160.0)
                    {
                        this.ShowHideCancelOverlay(false);
                    }
                    else
                    {
                        this.ShowHideCancelOverlay(true);
                        CompositeTransform transformButton2 = this.transformButton;
                        double num2 = transformButton2.TranslateX - x / 2.0;
                        transformButton2.TranslateX = num2;
                    }
                    this.translateRecordDuration.X = this.transformButton.TranslateX;
                    ((UIElement)this.panelSlideToCancel).Opacity = (AudioRecorderUC.GetSlideToCancelOpacity(this.transformButton.TranslateX));
                }
            }
            else
                this.MoveToInitial();
        }

        public void HandleManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            if (this._isInSendState)
                return;
            AudioRecorderPreviewUC previewUc = this._previewUC;
            if ((previewUc != null ? (((UIElement)previewUc).Visibility == 0 ? 1 : 0) : 0) != 0)
                return;
            Point translation = e.TotalManipulation.Translation;
            // ISSUE: explicit reference operation
            if (Math.Abs(((Point)@translation).Y) > 80.0 && !this._isInSendState)
                this.GoToSendState();
            else if (this.transformButton.TranslateX > -160.0)
            {
                if (this._viewModel.RecordDuration > 500)
                    this.StopRecordingAndSend();
                else
                    this.IsOpened = false;
            }
            else
            {
                if (this.transformButton.TranslateX > -16.0)
                    this.transformButton.CenterX = 44.0;
                this.IsOpened = false;
            }
        }

        private static double GetSlideToCancelOpacity(double translateX)
        {
            return translateX < 0.0 ? (translateX >= -160.0 ? 1.0 - translateX / -160.0 : 0.0) : 1.0;
        }

        private void ShowHideCancelOverlay(bool show)
        {
            if (this._isAnimatingCancel)
                return;
            List<AnimationInfo> animInfoList = null;
            if (show && ((UIElement)this.borderCancel).Opacity < 1.0)
            {
                List<AnimationInfo> animationInfoList = new List<AnimationInfo>();
                AnimationInfo animationInfo = new AnimationInfo();
                animationInfo.target = (DependencyObject)this.borderCancel;
                animationInfo.propertyPath = UIElement.OpacityProperty;
                animationInfo.from = ((UIElement)this.borderCancel).Opacity;
                animationInfo.to = 1.0;
                CubicEase cubicEase = new CubicEase();
                int num1 = 0;
                ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num1);
                animationInfo.easing = (IEasingFunction)cubicEase;
                int num2 = 100;
                animationInfo.duration = num2;
                animationInfoList.Add(animationInfo);
                animInfoList = animationInfoList;
            }
            else if (!show && ((UIElement)this.borderCancel).Opacity > 0.0)
            {
                List<AnimationInfo> animationInfoList = new List<AnimationInfo>();
                AnimationInfo animationInfo = new AnimationInfo();
                animationInfo.target = (DependencyObject)this.borderCancel;
                animationInfo.propertyPath = UIElement.OpacityProperty;
                animationInfo.from = ((UIElement)this.borderCancel).Opacity;
                animationInfo.to = 0.0;
                CubicEase cubicEase = new CubicEase();
                int num1 = 0;
                ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num1);
                animationInfo.easing = (IEasingFunction)cubicEase;
                int num2 = 100;
                animationInfo.duration = num2;
                animationInfoList.Add(animationInfo);
                animInfoList = animationInfoList;
            }
            if (animInfoList == null)
                return;
            this._isAnimatingCancel = true;
            AnimationUtil.AnimateSeveral(animInfoList, new int?(), (Action)(() =>
            {
                this._isAnimatingCancel = false;
                if (((UIElement)this.borderCancel).Opacity < 1.0)
                    ((Shape)this.ellipseVolume).Fill = ((Brush)Application.Current.Resources["PhoneBlue300_GrayBlue400Brush"]);
                else
                    ((Shape)this.ellipseVolume).Fill = ((Brush)Application.Current.Resources["PhoneAccentRedBrush"]);
            }));
        }

        private void MoveToInitial()
        {
            if (this._isInInitial)
                return;
            this._isInInitial = true;
            ((Shape)this.ellipseVolume).Fill = ((Brush)Application.Current.Resources["PhoneBlue300_GrayBlue400Brush"]);
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            AnimationInfo animationInfo1 = new AnimationInfo();
            animationInfo1.target = (DependencyObject)this.transformButton;
            animationInfo1.propertyPath = CompositeTransform.TranslateXProperty;
            animationInfo1.from = this.transformButton.TranslateX;
            animationInfo1.to = 0.0;
            animationInfo1.duration = 200;
            CubicEase cubicEase1 = new CubicEase();
            int num1 = 0;
            ((EasingFunctionBase)cubicEase1).EasingMode = ((EasingMode)num1);
            animationInfo1.easing = (IEasingFunction)cubicEase1;
            animInfoList.Add(animationInfo1);
            AnimationInfo animationInfo2 = new AnimationInfo();
            animationInfo2.target = (DependencyObject)this.translateRecordDuration;
            animationInfo2.propertyPath = TranslateTransform.XProperty;
            animationInfo2.from = this.translateRecordDuration.X;
            animationInfo2.to = 0.0;
            animationInfo2.duration = 200;
            CubicEase cubicEase2 = new CubicEase();
            int num2 = 0;
            ((EasingFunctionBase)cubicEase2).EasingMode = ((EasingMode)num2);
            animationInfo2.easing = (IEasingFunction)cubicEase2;
            animInfoList.Add(animationInfo2);
            AnimationInfo animationInfo3 = new AnimationInfo();
            animationInfo3.target = (DependencyObject)this.borderCancel;
            animationInfo3.propertyPath = UIElement.OpacityProperty;
            animationInfo3.from = ((UIElement)this.borderCancel).Opacity;
            animationInfo3.to = 0.0;
            CubicEase cubicEase3 = new CubicEase();
            int num3 = 1;
            ((EasingFunctionBase)cubicEase3).EasingMode = ((EasingMode)num3);
            animationInfo3.easing = (IEasingFunction)cubicEase3;
            int num4 = 200;
            animationInfo3.duration = num4;
            animInfoList.Add(animationInfo3);
            AnimationInfo animationInfo4 = new AnimationInfo();
            animationInfo4.target = (DependencyObject)this.panelSlideToCancel;
            animationInfo4.propertyPath = UIElement.OpacityProperty;
            animationInfo4.from = ((UIElement)this.panelSlideToCancel).Opacity;
            animationInfo4.to = 1.0;
            CubicEase cubicEase4 = new CubicEase();
            int num5 = 0;
            ((EasingFunctionBase)cubicEase4).EasingMode = ((EasingMode)num5);
            animationInfo4.easing = (IEasingFunction)cubicEase4;
            int num6 = 200;
            animationInfo4.duration = num6;
            animInfoList.Add(animationInfo4);
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private void MoveToPosition(double x)
        {
            double num1 = Math.Min(x, 0.0);
            double translateX = num1 <= -160.0 ? num1 - (num1 + 160.0) / 2.0 : num1;
            double slideToCancelOpacity = AudioRecorderUC.GetSlideToCancelOpacity(translateX);
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            AnimationInfo animationInfo1 = new AnimationInfo();
            animationInfo1.target = (DependencyObject)this.transformButton;
            animationInfo1.propertyPath = CompositeTransform.TranslateXProperty;
            animationInfo1.from = this.transformButton.TranslateX;
            animationInfo1.to = translateX;
            animationInfo1.duration = 200;
            CubicEase cubicEase1 = new CubicEase();
            int num2 = 0;
            ((EasingFunctionBase)cubicEase1).EasingMode = ((EasingMode)num2);
            animationInfo1.easing = (IEasingFunction)cubicEase1;
            animInfoList.Add(animationInfo1);
            AnimationInfo animationInfo2 = new AnimationInfo();
            animationInfo2.target = (DependencyObject)this.translateRecordDuration;
            animationInfo2.propertyPath = TranslateTransform.XProperty;
            animationInfo2.from = this.translateRecordDuration.X;
            animationInfo2.to = translateX;
            animationInfo2.duration = 200;
            CubicEase cubicEase2 = new CubicEase();
            int num3 = 0;
            ((EasingFunctionBase)cubicEase2).EasingMode = ((EasingMode)num3);
            animationInfo2.easing = (IEasingFunction)cubicEase2;
            animInfoList.Add(animationInfo2);
            AnimationInfo animationInfo3 = new AnimationInfo();
            animationInfo3.target = (DependencyObject)this.panelSlideToCancel;
            animationInfo3.propertyPath = UIElement.OpacityProperty;
            animationInfo3.from = ((UIElement)this.panelSlideToCancel).Opacity;
            animationInfo3.to = slideToCancelOpacity;
            animationInfo3.duration = 200;
            CubicEase cubicEase3 = new CubicEase();
            int num4 = 0;
            ((EasingFunctionBase)cubicEase3).EasingMode = ((EasingMode)num4);
            animationInfo3.easing = (IEasingFunction)cubicEase3;
            animInfoList.Add(animationInfo3);
            int? startTime = new int?();
            // ISSUE: variable of the null type

            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private void GoToSendState()
        {
            this._isInSendState = true;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleStop,
                propertyPath = ScaleTransform.ScaleXProperty,
                from = this.scaleStop.ScaleX,
                to = 0.0,
                duration = 150
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.scaleStop,
                propertyPath = ScaleTransform.ScaleYProperty,
                from = this.scaleStop.ScaleY,
                to = 0.0,
                duration = 150
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.borderStop,
                propertyPath = UIElement.OpacityProperty,
                from = ((UIElement)this.borderStop).Opacity,
                to = 0.0,
                duration = 150
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.panelSlideToCancel,
                propertyPath = UIElement.OpacityProperty,
                from = ((UIElement)this.panelSlideToCancel).Opacity,
                to = 0.0,
                duration = 150
            });
            List<AnimationInfo> animationsSend = new List<AnimationInfo>()
      {
        new AnimationInfo()
        {
          target = (DependencyObject) this.scaleSend,
          propertyPath = ScaleTransform.ScaleXProperty,
          from = this.scaleSend.ScaleX,
          to = 1.0,
          duration = 150
        },
        new AnimationInfo()
        {
          target = (DependencyObject) this.scaleSend,
          propertyPath = ScaleTransform.ScaleYProperty,
          from = this.scaleSend.ScaleY,
          to = 1.0,
          duration = 150
        },
        new AnimationInfo()
        {
          target = (DependencyObject) this.borderSend,
          propertyPath = UIElement.OpacityProperty,
          from = ((UIElement) this.borderSend).Opacity,
          to = 1.0,
          duration = 150
        },
        new AnimationInfo()
        {
          target = (DependencyObject) this.borderCancelButton,
          propertyPath = UIElement.OpacityProperty,
          from = ((UIElement) this.borderCancelButton).Opacity,
          to = 1.0,
          duration = 150
        }
      };
            int? startTime = new int?();
            Action completed = (() => AnimationUtil.AnimateSeveral(animationsSend, new int?(), null));
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void Cancel_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.IsOpened = false;
        }

        private void RectBackground_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        private void RectBackground_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
        }

        private void RectBackground_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AudioRecorderUC.xaml", UriKind.Relative));
            this.gridRoot = (Grid)base.FindName("gridRoot");
            this.gridRecordOverlay = (Grid)base.FindName("gridRecordOverlay");
            this.translateRecordOverlay = (TranslateTransform)base.FindName("translateRecordOverlay");
            this.translateRecordDuration = (TranslateTransform)base.FindName("translateRecordDuration");
            this.ellipseRec = (Ellipse)base.FindName("ellipseRec");
            this.panelSlideToCancel = (StackPanel)base.FindName("panelSlideToCancel");
            this.borderCancelButton = (Border)base.FindName("borderCancelButton");
            this.transformButton = (CompositeTransform)base.FindName("transformButton");
            this.ellipseVolume = (Ellipse)base.FindName("ellipseVolume");
            this.scaleVolume = (ScaleTransform)base.FindName("scaleVolume");
            this.borderStop = (Image)base.FindName("borderStop");
            this.scaleStop = (ScaleTransform)base.FindName("scaleStop");
            this.borderSend = (Canvas)base.FindName("borderSend");
            this.scaleSend = (ScaleTransform)base.FindName("scaleSend");
            this.borderCancel = (Canvas)base.FindName("borderCancel");
        }
    }
}
