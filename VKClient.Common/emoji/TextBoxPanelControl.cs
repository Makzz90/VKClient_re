using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace VKClient.Common.Emoji
{
    public class TextBoxPanelControl : UserControl
    {
        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register("RootFrameTransform", typeof(double), typeof(TextBoxPanelControl), new PropertyMetadata(new PropertyChangedCallback(TextBoxPanelControl.OnRootFrameTransformChanged)));
        private static int _instanceCount;
        private double _pageWidth;
        public EventHandler<bool> IsOpenedChanged;
        public EventHandler<bool> IsEmojiOpenedChanged;
        public EventHandler<bool> IsFocusedChanged;
        private bool _isOpen;
        private bool _isTextBoxTargetFocused;
        private TranslateTransform _frameTransform;
        private FrameworkElement _childElement;
        private bool _isInitialized;
        private Frame _frame;
        private PageBase _currentPage;
        private bool _lastIsOpenState;
        private bool _lastIsEmojiOpenState;
        private bool _lastIsFocusedState;
        private bool _isTextBoxHiding;
        private double _softNavButtonsCurrentWidth;
        private bool _ignoreGotFocus;
        private bool _ignoreLostFocus;
        internal Grid LayoutRoot;
        internal Rectangle rectOverlay;
        private bool _contentLoaded;

        public TextBox TextBoxTarget { get; set; }

        public double ExtraBottomMargin { get; set; }

        public double PortraitOrientationHeight
        {
            get
            {
                return KeyboardHelper.PortraitHeight;
            }
        }

        public bool IsTextBoxTargetFocused
        {
            get
            {
                return this._isTextBoxTargetFocused;
            }
        }

        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }
            set
            {
                if (value == this._isOpen)
                    return;
                if (value)
                    this.Open();
                else
                    this.Hide();
            }
        }

        public TextBoxPanelControl()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            TextBoxPanelControl.ReportInstanceCount(true);
            this.UpdateVisibilityState();
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.TextBoxPanelControl_OnLoaded));
            base.Unloaded += (new RoutedEventHandler(this.TextBoxPanelControl_OnUnloaded));
        }

        ~TextBoxPanelControl()
        {
            //try
            //{
            TextBoxPanelControl.ReportInstanceCount(false);
            //}
            //finally
            //{
            //  // ISSUE: explicit finalizer call
            //  // ISSUE: explicit non-virtual call
            //  this.Finalize();
            //}
        }

        public void InitializeWithChildControl(FrameworkElement element)
        {
            if (!(((UIElement)element).RenderTransform is TranslateTransform))
                ((UIElement)element).RenderTransform = ((Transform)new TranslateTransform());
            this._childElement = element;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this.LayoutRoot).Children).Add((UIElement)this._childElement);
        }

        private void TextBoxPanelControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this._isInitialized)
                return;
            this._frameTransform = (TranslateTransform)((PresentationFrameworkCollection<Transform>)((TransformGroup)((UIElement)FramePageUtils.Frame).RenderTransform).Children)[0];
            Binding binding1 = new Binding("Y");
            TranslateTransform frameTransform = this._frameTransform;
            binding1.Source = frameTransform;
            Binding binding2 = binding1;
            base.SetBinding(TextBoxPanelControl.RootFrameTransformProperty, binding2);
            this._pageWidth = Application.Current.Host.Content.ActualWidth;
            base.SizeChanged += (new SizeChangedEventHandler(this.OnSizeChanged));
            this.OnSizeChanged(null, null);
            InputPane forCurrentView = InputPane.GetForCurrentView();
            forCurrentView.Showing += this.InputPane_OnShowing;

            this._isInitialized = true;
        }

        private void TextBoxPanelControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Cleanup();
            InputPane.GetForCurrentView().Showing -= this.InputPane_OnShowing;
            this._isInitialized = false;
        }

        private void InputPane_OnShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Windows.Foundation.Rect occludedRect = args.OccludedRect;
            double width = occludedRect.Width;
            double height = occludedRect.Height;
            double pageWidth = this._pageWidth;
            Orientation screenOrientation = TextBoxPanelControl.GetScreenOrientation();
            if (screenOrientation == Orientation.Horizontal)
                pageWidth -= this._softNavButtonsCurrentWidth;
            double num1 = pageWidth / width;
            double num2 = width * num1;
            double d = height * num1;
            if (double.IsNaN(d) || double.IsInfinity(d) || (d == 0.0 || this._isTextBoxHiding))
            {
                this._isTextBoxHiding = false;
            }
            else
            {
                if (screenOrientation != Orientation.Vertical)
                {
                    if (screenOrientation == Orientation.Horizontal)
                        KeyboardHelper.LandscapeHeight = d;
                }
                else
                    KeyboardHelper.PortraitHeight = d;
                this.InitializeOrientation(screenOrientation);
            }
        }

        private void Cleanup()
        {
            base.ClearValue(TextBoxPanelControl.RootFrameTransformProperty);
        }

        private static void ReportInstanceCount(bool increase)
        {
            TextBoxPanelControl._instanceCount += increase ? 1 : -1;
        }

        private void Open()
        {
            this._isOpen = true;
            this._frame = (Frame)FramePageUtils.Frame;
            this._currentPage = FramePageUtils.CurrentPage;
            if (this._currentPage != null)
                this._currentPage.BackKeyPress += (new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress));
            if (this._frame != null)
            {
                this._frame.Navigated += (new NavigatedEventHandler(this.Frame_OnNavigated));
            }
            this.UpdateVisibilityState();
        }

        private void Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            this.Hide();
        }

        private void Hide()
        {
            this._isOpen = false;
            if (this._currentPage != null)
                this._currentPage.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress));
            if (this._frame != null)
            {
                this._frame.Navigated -= (new NavigatedEventHandler(this.Frame_OnNavigated));
            }
            this.UpdateVisibilityState();
        }

        private void UpdateVisibilityState()
        {
            ((UIElement)this.LayoutRoot).Visibility = (this._isTextBoxTargetFocused || this._isOpen ? Visibility.Visible : Visibility.Collapsed);
            ((UIElement)this.LayoutRoot).Opacity = (this._isOpen ? 1.0 : 0.0);
            bool isOpened = this._isTextBoxTargetFocused || this._isOpen;
            bool isOpen = this._isOpen;
            bool boxTargetFocused = this._isTextBoxTargetFocused;
            if (isOpened != this._lastIsOpenState)
            {
                EventHandler<bool> isOpenedChanged = this.IsOpenedChanged;
                if (isOpenedChanged != null)
                {
                    int num = isOpened ? 1 : 0;
                    isOpenedChanged(this, num != 0);
                }
                EventAggregator.Current.Publish(new PanelControlOpenedChangedEvent(isOpened));
                this._lastIsOpenState = isOpened;
                if (isOpen != this._lastIsEmojiOpenState && this._childElement != null && ((UIElement)this.LayoutRoot).Visibility == Visibility.Visible)
                {
                    TranslateTransform renderTransform = ((UIElement)this._childElement).RenderTransform as TranslateTransform;
                    if (renderTransform != null)
                    {
                        ExponentialEase exponentialEase = new ExponentialEase();
                        int num1 = 0;
                        ((EasingFunctionBase)exponentialEase).EasingMode = ((EasingMode)num1);
                        double num2 = 6.0;
                        exponentialEase.Exponent = num2;
                        IEasingFunction easing = (IEasingFunction)exponentialEase;
                        ((DependencyObject)renderTransform).Animate(((FrameworkElement)this.LayoutRoot).Height, 0.0, TranslateTransform.YProperty, 350, new int?(), easing, null, false);
                    }
                }
            }
            if (isOpen != this._lastIsEmojiOpenState)
            {
                EventHandler<bool> emojiOpenedChanged = this.IsEmojiOpenedChanged;
                if (emojiOpenedChanged != null)
                {
                    int num = isOpen ? 1 : 0;
                    emojiOpenedChanged(this, num != 0);
                }
                this._lastIsEmojiOpenState = isOpen;
            }
            if (boxTargetFocused == this._lastIsFocusedState)
                return;
            this.IsFocusedChanged(this, boxTargetFocused);
            this._lastIsFocusedState = boxTargetFocused;
            EventAggregator.Current.Publish(new PanelControlFocusedChangedEvent(boxTargetFocused));
        }

        public void BindTextBox(TextBox textBox)
    {
      this.TextBoxTarget = textBox;
      textBox.GotFocus += (new RoutedEventHandler(this.TextBoxOnGotFocus));
      textBox.LostFocus += (new RoutedEventHandler(this.TextBoxOnLostFocus));
    }

        public void UnbindTextBox()
        {
            this.TextBoxTarget.GotFocus -= (new RoutedEventHandler(this.TextBoxOnGotFocus));
            this.TextBoxTarget.LostFocus -= (new RoutedEventHandler(this.TextBoxOnLostFocus));
            this.TextBoxTarget = null;
        }

        private void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            this._isTextBoxHiding = false;
            if (this._ignoreGotFocus)
            {
                this._ignoreGotFocus = false;
            }
            else
            {
                this._isTextBoxTargetFocused = true;
                if (this.IsOpen)
                    this.IsOpen = false;
                else
                    this.UpdateVisibilityState();
            }
        }

        private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            this._isTextBoxHiding = true;
            if (this._ignoreLostFocus)
            {
                this._ignoreLostFocus = false;
            }
            else
            {
                this._isTextBoxTargetFocused = false;
                this.UpdateVisibilityState();
            }
        }

        private void Page_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;
            this.IsOpen = false;
            e.Cancel = true;
        }

        private static void OnRootFrameTransformChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((TextBoxPanelControl)source).OnRootFrameTransformChanged();
        }

        private void OnRootFrameTransformChanged()
        {
            this._frameTransform.Y = 0.0;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null)
            {
                System.Windows.Size newSize = e.NewSize;
                double num = newSize.Width > newSize.Height ? newSize.Width : newSize.Height;
                double actualWidth = Application.Current.Host.Content.ActualWidth;
                double actualHeight = Application.Current.Host.Content.ActualHeight;
                this._softNavButtonsCurrentWidth = Math.Max((actualWidth > actualHeight ? actualWidth : actualHeight) - num, 0.0);
            }
            this.InitializeOrientation(TextBoxPanelControl.GetScreenOrientation());
        }

        private static Orientation GetScreenOrientation()
        {
            PageOrientation orientation = ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation;
            int num;
            switch (orientation)
            {
                case PageOrientation.PortraitUp:
                case PageOrientation.PortraitDown:
                    num = 1;
                    break;
                default:
                    num = orientation == PageOrientation.Portrait ? 1 : 0;
                    break;
            }
            return num == 0 ? Orientation.Horizontal : Orientation.Vertical;
        }

        private void InitializeOrientation(Orientation orientation)
        {
            if (orientation != Orientation.Vertical)
            {
                if (orientation != Orientation.Vertical)
                    return;
                ((FrameworkElement)this.LayoutRoot).Height = KeyboardHelper.LandscapeHeight;
            }
            else
                ((FrameworkElement)this.LayoutRoot).Height = KeyboardHelper.PortraitHeight;
        }

        internal void IgnoreNextLostGotFocus()
        {
            this._ignoreGotFocus = true;
            this._ignoreLostFocus = true;
        }

        public void ShowOverlay()
        {
            ((UIElement)this.rectOverlay).Visibility = Visibility.Visible;
        }

        public void HideOverlay()
        {
            ((UIElement)this.rectOverlay).Visibility = Visibility.Collapsed;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new System.Uri("/VKClient.Common;component/Emoji/TextBoxPanelControl.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.rectOverlay = (Rectangle)base.FindName("rectOverlay");
        }
    }
}
