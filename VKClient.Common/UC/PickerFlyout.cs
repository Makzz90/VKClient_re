using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class PickerFlyout : IFlyout
    {
        private static readonly object Lockobj = new object();
        private bool _isOverlayApplied = true;
        public bool HideOnNavigation = true;
        private const string NoneStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>";
        private Panel _popupContainer;
        private Frame _rootVisual;
        private PageBase _page;
        private Grid _childPanel;
        private Grid _overlay;
        //private FrameworkElement _child;
        private IApplicationBar _applicationBar;
        private bool _deferredShowToLoaded;
        private UIElement _controlToFadeout;
        private bool _wasMenuSuppressed;

        public bool IsOverlayApplied
        {
            get
            {
                return this._isOverlayApplied;
            }
            set
            {
                this._isOverlayApplied = value;
            }
        }

        public FrameworkElement Child { get; set; }

        public double VerticalOffset { get; set; }

        internal double ControlVerticalOffset { get; set; }

        public bool BackButtonPressed { get; set; }

        public Brush BackgroundBrush { get; set; }

        public bool IsOpen { get; set; }

        protected internal bool IsBackKeyOverride { get; set; }

        public bool HasPopup { get; set; }

        internal PageBase Page
        {
            get
            {
                return this._page ?? (this._page = ((FrameworkElement)this.RootVisual).GetFirstLogicalChildByType<PageBase>(false));
            }
        }

        internal Frame RootVisual
        {
            get
            {
                return this._rootVisual ?? (this._rootVisual = Application.Current.RootVisual as Frame);
            }
        }

        public bool KeepAppBar { get; set; }

        public bool SetStatusBarBackground { get; set; }

        internal Panel PopupContainer
        {
            get
            {
                if (this._popupContainer == null)
                {
                    IEnumerable<ContentPresenter> logicalChildrenByType1 = ((FrameworkElement)this.RootVisual).GetLogicalChildrenByType<ContentPresenter>(false);
                    for (int index = 0; index < Enumerable.Count<ContentPresenter>(logicalChildrenByType1); ++index)
                    {
                        IEnumerable<Panel> logicalChildrenByType2 = ((FrameworkElement)Enumerable.ElementAt<ContentPresenter>(logicalChildrenByType1, index)).GetLogicalChildrenByType<Panel>(false);
                        if (Enumerable.Any<Panel>(logicalChildrenByType2))
                        {
                            this._popupContainer = (Panel)Enumerable.First<Panel>(logicalChildrenByType2);
                            break;
                        }
                    }
                }
                return this._popupContainer;
            }
        }

        public event EventHandler Closed;

        public event EventHandler Opened;

        public PickerFlyout()
        {
            this.BackButtonPressed = false;
            this.BackgroundBrush = (Brush)new SolidColorBrush(Color.FromArgb((byte)100, (byte)0, (byte)0, (byte)0));
        }

        private void InitializePopup()
        {
            this._childPanel = this.CreateGrid();
            if (this.IsOverlayApplied)
            {
                this._overlay = this.CreateGrid();
                ((FrameworkElement)this._overlay).UseOptimizedManipulationRouting = false;
                ((UIElement)this._overlay).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((s, e) => this.Hide()));
                if (this.BackgroundBrush != null)
                    ((Panel)this._overlay).Background = this.BackgroundBrush;
            }
            if (this.PopupContainer != null)
            {
                if (this._overlay != null)
                    ((PresentationFrameworkCollection<UIElement>)this.PopupContainer.Children).Add((UIElement)this._overlay);
                ((PresentationFrameworkCollection<UIElement>)this.PopupContainer.Children).Add((UIElement)this._childPanel);
                ((PresentationFrameworkCollection<UIElement>)((Panel)this._childPanel).Children).Add((UIElement)this.Child);
            }
            else
            {
                this._deferredShowToLoaded = true;
                // ISSUE: method pointer
                ((FrameworkElement)this.RootVisual).Loaded += (new RoutedEventHandler(this.RootVisualDeferredShowLoaded));
            }
        }

        private Grid CreateGrid()
        {
            Grid grid1 = new Grid();
            string str = Guid.NewGuid().ToString();
            ((FrameworkElement)grid1).Name = str;
            Grid grid2 = grid1;
            Grid.SetColumnSpan((FrameworkElement)grid2, int.MaxValue);
            Grid.SetRowSpan((FrameworkElement)grid2, int.MaxValue);
            ((UIElement)grid2).Opacity = 0.0;
            this.CalculateVerticalOffset((Panel)grid2);
            return grid2;
        }

        internal void CalculateVerticalOffset()
        {
            this.CalculateVerticalOffset((Panel)this._childPanel);
        }

        internal void CalculateVerticalOffset(Panel panel)
        {
            if (panel == null)
                return;
            int num = 0;
            if (SystemTray.IsVisible && SystemTray.Opacity < 1.0 && SystemTray.Opacity > 0.0)
                num += 32;
            ((FrameworkElement)panel).Margin = (new Thickness(0.0, this.VerticalOffset + (double)num + this.ControlVerticalOffset, 0.0, 0.0));
        }

        private void RootVisualDeferredShowLoaded(object sender, RoutedEventArgs e)
        {
            // ISSUE: method pointer
            ((FrameworkElement)this.RootVisual).Loaded -= (new RoutedEventHandler(this.RootVisualDeferredShowLoaded));
            this._deferredShowToLoaded = false;
            this.Show(null);
        }

        protected internal void SetAlignmentsOnOverlay(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            if (this._childPanel == null)
                return;
            ((FrameworkElement)this._childPanel).HorizontalAlignment = horizontalAlignment;
            ((FrameworkElement)this._childPanel).VerticalAlignment = verticalAlignment;
        }

        public void Show(UIElement controlToFadeout = null)
        {
            this._controlToFadeout = controlToFadeout;
            object lockobj = PickerFlyout.Lockobj;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(lockobj, ref lockTaken);
                // ISSUE: object of a compiler-generated type is created
                // ISSUE: variable of a compiler-generated type
                //       PickerFlyout.<>c__DisplayClass75_0 cDisplayClass750 = new PickerFlyout.<>c__DisplayClass75_0();
                // ISSUE: reference to a compiler-generated field
                //       cDisplayClass750.<>4__this = this;
                if (this.Page == null)
                    return;
                this.IsOpen = true;
                this.InitializePopup();
                if (this._deferredShowToLoaded)
                    return;
                if (!this.IsBackKeyOverride)
                    this.Page.BackKeyPress += (new EventHandler<CancelEventArgs>(this.OnBackKeyPress));
                // ISSUE: method pointer
                ((System.Windows.Controls.Page)this.Page).NavigationService.Navigated += (new NavigatedEventHandler(this.OnNavigated));
                if (this.SetStatusBarBackground)
                    SystemTray.BackgroundColor = ((Application.Current.Resources["PhoneChromeBrush"] as SolidColorBrush).Color);
                if (!this.KeepAppBar)
                {
                    this._applicationBar = this.Page.ApplicationBar;
                    this.Page.ApplicationBar = (null);
                }
                this.AnimateParentControl(true);
                // ISSUE: reference to a compiler-generated field
                int completedStoryboards0 = 0;
                // ISSUE: method pointer
                Action completionCallback = delegate
                        {
                            completedStoryboards0++;
                            if (completedStoryboards0 == 3)
                            {
                                this.Page.Flyouts.Add(this);
                                if (this.Opened != null)
                                {
                                    this.Opened.Invoke(this, null);
                                }
                            }
                        };
                this.RunShowStoryboard((FrameworkElement)this._overlay, completionCallback);
                this.RunShowStoryboard((FrameworkElement)this._childPanel, completionCallback);
                this.RunShowStoryboard(this.Child, completionCallback);
                this._wasMenuSuppressed = this.Page.SuppressMenu;
                this.Page.SuppressMenu = true;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockobj);
            }
        }

        private void AnimateParentControl(bool fadeout)
        {
            if (this._controlToFadeout == null)
                return;
            AnimationUtils.Animate(fadeout ? 0.5 : 1.0, (DependencyObject)this._controlToFadeout, "Opacity", 0.25);
            if (fadeout)
                return;
            this._controlToFadeout.Visibility = Visibility.Visible;
        }

        private void RunShowStoryboard(FrameworkElement element, Action completionCallback)
        {
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            //     PickerFlyout.<>c__DisplayClass77_0 cDisplayClass770 = new PickerFlyout.<>c__DisplayClass77_0();
            // ISSUE: reference to a compiler-generated field
            //     cDisplayClass770.element = element;
            // ISSUE: reference to a compiler-generated field
            //     cDisplayClass770.completionCallback = completionCallback;
            // ISSUE: reference to a compiler-generated field
            if (element == null)
            {
                // ISSUE: reference to a compiler-generated field
                if (completionCallback == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                completionCallback.Invoke();
            }
            else
            {
                // ISSUE: reference to a compiler-generated field
                Storyboard storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>") as Storyboard;
                // ISSUE: reference to a compiler-generated field
                if (storyboard != null)
                {
                    EventHandler _9__1=null;
                    this.Page.Dispatcher.BeginInvoke(delegate
                    {
                        using (IEnumerator<Timeline> enumerator = storyboard.Children.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Storyboard.SetTarget(enumerator.Current, element);
                            }
                        }
                        Timeline arg_5D_0 = storyboard;
                        EventHandler arg_5D_1;
                        if ((arg_5D_1 = _9__1) == null)
                        {
                            arg_5D_1 = (_9__1 = delegate(object s, EventArgs e)
                            {
                                if (completionCallback != null)
                                {
                                    completionCallback.Invoke();
                                }
                            });
                        }
                        arg_5D_0.Completed+=(arg_5D_1);
                        storyboard.Begin();
                    });
                    return;
                }
                element.Opacity=(1.0);
                if (completionCallback != null)
                {
                    completionCallback.Invoke();
                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (!e.IsNavigationInitiator || !this.HideOnNavigation)
                return;
            this.Hide();
        }

        public void Hide()
        {
            if (!this.IsOpen)
                return;
            this.Page.SuppressMenu = this._wasMenuSuppressed;
            this.HideStoryboardCompleted(null, null);
        }

        private void RunHideStoryboard(FrameworkElement element, Action completionCallback)
        {
            completionCallback.Invoke();
        }

        private void HideStoryboardCompleted(object sender, EventArgs e)
        {
            this.IsOpen = false;
            try
            {
                if (this.SetStatusBarBackground)
                    SystemTray.BackgroundColor = ((Application.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color);
                if (this.Page != null)
                {
                    if ((this.Page.Flyouts).Contains((IFlyout)this))
                        (this.Page.Flyouts).Remove((IFlyout)this);
                    this.Page.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.OnBackKeyPress));
                    // ISSUE: method pointer
                    ((System.Windows.Controls.Page)this.Page).NavigationService.Navigated -= (new NavigatedEventHandler(this.OnNavigated));
                    if (this._applicationBar != null)
                    {
                        this.Page.ApplicationBar = this._applicationBar;
                        this._applicationBar = null;
                    }
                    this.AnimateParentControl(false);
                    this._page = null;
                }
            }
            catch
            {
            }
            try
            {
                if (this.PopupContainer != null)
                {
                    if (this.PopupContainer.Children != null)
                    {
                        if (this._overlay != null)
                            ((PresentationFrameworkCollection<UIElement>)this.PopupContainer.Children).Remove((UIElement)this._overlay);
                        ((PresentationFrameworkCollection<UIElement>)this.PopupContainer.Children).Remove((UIElement)this._childPanel);
                    }
                }
            }
            catch
            {
            }
            try
            {
                // ISSUE: reference to a compiler-generated field
                EventHandler closed = this.Closed;
                if (closed == null)
                    return;
                // ISSUE: variable of the null type

                closed(this, null);
            }
            catch
            {
            }
        }

        public void ChangeChild(FrameworkElement newChild, Action callback = null)
        {
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._childPanel).Children).Remove((UIElement)this.Child);
            this.Child = newChild;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._childPanel).Children).Add((UIElement)this.Child);
            this.RunShowStoryboard(this.Child, callback);
        }

        public void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (!this.Page.CanNavigateBack)
                return;
            if (this.HasPopup)
            {
                e.Cancel = true;
            }
            else
            {
                if (!this.IsOpen)
                    return;
                e.Cancel = true;
                this.BackButtonPressed = true;
                this.Hide();
            }
        }
    }
}
