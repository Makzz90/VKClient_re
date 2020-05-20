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
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace VKClient.Common.Emoji
{
    public class TextBoxPanelControl : UserControl
    {
        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register("RootFrameTransform", typeof(double), typeof(TextBoxPanelControl), new PropertyMetadata(new PropertyChangedCallback(TextBoxPanelControl.OnRootFrameTransformChanged)));
        public EventHandler<bool> IsOpenedChanged = (EventHandler<bool>)((param0, param1) => { });
        public EventHandler<bool> IsEmojiOpenedChanged = (EventHandler<bool>)((param0, param1) => { });
        public EventHandler<bool> IsFocusedChanged = (EventHandler<bool>)((param0, param1) => { });
        private static int _instanceCount;
        private double _pageWidth;
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
            this.InitializeComponent();
            TextBoxPanelControl.ReportInstanceCount(true);
            this.UpdateVisibilityState();
            this.Loaded += new RoutedEventHandler(this.TextBoxPanelControl_OnLoaded);
            this.Unloaded += new RoutedEventHandler(this.TextBoxPanelControl_OnUnloaded);
        }

        ~TextBoxPanelControl()
        {
            TextBoxPanelControl.ReportInstanceCount(false);
        }

        public void InitializeWithChildControl(FrameworkElement element)
        {
            if (!(element.RenderTransform is TranslateTransform))
                element.RenderTransform = (Transform)new TranslateTransform();
            this._childElement = element;
            this.LayoutRoot.Children.Add((UIElement)this._childElement);
        }

        private void TextBoxPanelControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this._isInitialized)
                return;
            this._frameTransform = (TranslateTransform)((TransformGroup)FramePageUtils.Frame.RenderTransform).Children[0];
            Binding binding = new Binding("Y")
            {
                Source = (object)this._frameTransform
            };
            this.SetBinding(TextBoxPanelControl.RootFrameTransformProperty, binding);
            this._pageWidth = Application.Current.Host.Content.ActualWidth;
            this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
            this.OnSizeChanged(null, null);
            InputPane forCurrentView = InputPane.GetForCurrentView();




            forCurrentView.Showing += this.InputPane_OnShowing;
            /*
            //WindowsRuntimeMarshal.AddEventHandler<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(new Func<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>, EventRegistrationToken>(forCurrentView.add_Showing), new Action<EventRegistrationToken>(forCurrentView.remove_Showing), new TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>((object)this, __methodptr(InputPane_OnShowing)));

            var eventName = "Showing";
            var myButton = forCurrentView;
            var runtimeEvent = myButton.GetType().GetEvent(eventName);
            Func<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>, EventRegistrationToken> add = (a) => { return (EventRegistrationToken)runtimeEvent.AddMethod.Invoke(myButton, new object[] { a }); };
            Action<EventRegistrationToken> remove = (a) => { runtimeEvent.RemoveMethod.Invoke(runtimeEvent, new object[] { a }); };
            WindowsRuntimeMarshal.AddEventHandler<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(add, remove, new TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>(this.InputPane_OnShowing));
            */
            this._isInitialized = true;
        }

        private void TextBoxPanelControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Cleanup();
            
            // Оригинал
            //WindowsRuntimeMarshal.RemoveEventHandler<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(new Action<EventRegistrationToken>(InputPane.GetForCurrentView().remove_Showing), new TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>((object)this, __methodptr(InputPane_OnShowing)));

            // Мой код
            //InputPane forCurrentView = InputPane.GetForCurrentView();
            //var eventName = "Showing";
            //var myButton = forCurrentView;
            //var runtimeEvent = myButton.GetType().GetEvent(eventName);
            //Action<EventRegistrationToken> remove = (a) => { runtimeEvent.RemoveMethod.Invoke(runtimeEvent, new object[] { a }); };

            // Мой код версия 2
            InputPane forCurrentView = InputPane.GetForCurrentView();
            forCurrentView.Showing -= this.InputPane_OnShowing;


            this._isInitialized = false;
        }

        private void InputPane_OnShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Windows.Foundation.Rect occludedRect = args.OccludedRect;
            double width = occludedRect.Width;
            double height = occludedRect.Height;
            double num1 = this._pageWidth;
            Orientation screenOrientation = TextBoxPanelControl.GetScreenOrientation();
            if (screenOrientation == Orientation.Horizontal)
                num1 -= this._softNavButtonsCurrentWidth;
            double num2 = num1 / width;
            double num3 = width * num2;
            double d = height * num2;
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
            this.ClearValue(TextBoxPanelControl.RootFrameTransformProperty);
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
                this._currentPage.BackKeyPress += new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress);
            if (this._frame != null)
                this._frame.Navigated += new NavigatedEventHandler(this.Frame_OnNavigated);
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
                this._currentPage.BackKeyPress -= new EventHandler<CancelEventArgs>(this.Page_OnBackKeyPress);
            if (this._frame != null)
                this._frame.Navigated -= new NavigatedEventHandler(this.Frame_OnNavigated);
            this.UpdateVisibilityState();
        }

        private void UpdateVisibilityState()
        {
            this.LayoutRoot.Visibility = this._isTextBoxTargetFocused || this._isOpen ? Visibility.Visible : Visibility.Collapsed;
            this.LayoutRoot.Opacity = this._isOpen ? 1.0 : 0.0;
            bool isOpened = this._isTextBoxTargetFocused || this._isOpen;
            bool flag1 = this._isOpen;
            bool flag2 = this._isTextBoxTargetFocused;
            if (isOpened != this._lastIsOpenState)
            {
                EventHandler<bool> eventHandler = this.IsOpenedChanged;
                if (eventHandler != null)
                {
                    int num = isOpened ? 1 : 0;
                    eventHandler((object)this, num != 0);
                }
                EventAggregator.Current.Publish((object)new PanelControlOpenedChangedEvent(isOpened));
                this._lastIsOpenState = isOpened;
                if (flag1 != this._lastIsEmojiOpenState && this._childElement != null && this.LayoutRoot.Visibility == Visibility.Visible)
                {
                    TranslateTransform target = this._childElement.RenderTransform as TranslateTransform;
                    if (target != null)
                    {
                        ExponentialEase exponentialEase = new ExponentialEase();
                        int num1 = 0;
                        exponentialEase.EasingMode = (EasingMode)num1;
                        double num2 = 6.0;
                        exponentialEase.Exponent = num2;
                        IEasingFunction easing = (IEasingFunction)exponentialEase;
                        target.Animate(this.LayoutRoot.Height, 0.0, (object)TranslateTransform.YProperty, 350, new int?(), easing, null, false);
                    }
                }
            }
            if (flag1 != this._lastIsEmojiOpenState)
            {
                EventHandler<bool> eventHandler = this.IsEmojiOpenedChanged;
                if (eventHandler != null)
                {
                    int num = flag1 ? 1 : 0;
                    eventHandler((object)this, num != 0);
                }
                this._lastIsEmojiOpenState = flag1;
            }
            if (flag2 == this._lastIsFocusedState)
                return;
            this.IsFocusedChanged((object)this, flag2);
            this._lastIsFocusedState = flag2;
            EventAggregator.Current.Publish((object)new PanelControlFocusedChangedEvent(flag2));
        }

        public void BindTextBox(TextBox textBox)
        {
            this.TextBoxTarget = textBox;
            textBox.GotFocus += new RoutedEventHandler(this.TextBoxOnGotFocus);
            textBox.LostFocus += new RoutedEventHandler(this.TextBoxOnLostFocus);
        }

        public void UnbindTextBox()
        {
            this.TextBoxTarget.GotFocus -= new RoutedEventHandler(this.TextBoxOnGotFocus);
            this.TextBoxTarget.LostFocus -= new RoutedEventHandler(this.TextBoxOnLostFocus);
            this.TextBoxTarget = (TextBox)null;
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
                if (orientation != Orientation.Horizontal)
                    return;
                this.LayoutRoot.Height = KeyboardHelper.LandscapeHeight;
            }
            else
                this.LayoutRoot.Height = KeyboardHelper.PortraitHeight;
        }

        internal void IgnoreNextLostGotFocus()
        {
            this._ignoreGotFocus = true;
            this._ignoreLostFocus = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/Emoji/TextBoxPanelControl.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
        }
    }
}