using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Library;

namespace VKClient.Common.Framework
{
    [TemplatePart(Name = "DragHandle", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "DropBeforeSpace", Type = typeof(UIElement))]
    [TemplatePart(Name = "DropAfterSpace", Type = typeof(UIElement))]
    [TemplateVisualState(GroupName = "ReorderEnabledStates", Name = "ReorderDisabled")]
    [TemplateVisualState(GroupName = "ReorderEnabledStates", Name = "ReorderEnabled")]
    [TemplateVisualState(GroupName = "DraggingStates", Name = "NotDragging")]
    [TemplateVisualState(GroupName = "DraggingStates", Name = "Dragging")]
    [TemplateVisualState(GroupName = "DropIndicatorStates", Name = "NoDropIndicator")]
    [TemplateVisualState(GroupName = "DropIndicatorStates", Name = "DropBeforeIndicator")]
    [TemplateVisualState(GroupName = "DropIndicatorStates", Name = "DropAfterIndicator")]
    public class ReorderListBoxItem : ListBoxItem
    {
        public static readonly DependencyProperty DropIndicatorHeightProperty = DependencyProperty.Register("DropIndicatorHeight", typeof(double), typeof(ReorderListBoxItem), new PropertyMetadata((object)0.0, new PropertyChangedCallback((d, e) => ((ReorderListBoxItem)d).OnDropIndicatorHeightChanged(e))));
        public static readonly DependencyProperty IsReorderEnabledProperty = DependencyProperty.Register("IsReorderEnabled", typeof(bool), typeof(ReorderListBoxItem), new PropertyMetadata((object)false, new PropertyChangedCallback((d, e) => ((ReorderListBoxItem)d).OnIsReorderEnabledChanged(e))));
        public static readonly DependencyProperty DragHandleTemplateProperty = DependencyProperty.Register("DragHandleTemplate", typeof(DataTemplate), typeof(ReorderListBoxItem), null);
        public const string DragHandlePart = "DragHandle";
        public const string DropBeforeSpacePart = "DropBeforeSpace";
        public const string DropAfterSpacePart = "DropAfterSpace";
        public const string ReorderEnabledStateGroup = "ReorderEnabledStates";
        public const string ReorderDisabledState = "ReorderDisabled";
        public const string ReorderEnabledState = "ReorderEnabled";
        public const string DraggingStateGroup = "DraggingStates";
        public const string NotDraggingState = "NotDragging";
        public const string DraggingState = "Dragging";
        public const string DropIndicatorStateGroup = "DropIndicatorStates";
        public const string NoDropIndicatorState = "NoDropIndicator";
        public const string DropBeforeIndicatorState = "DropBeforeIndicator";
        public const string DropAfterIndicatorState = "DropAfterIndicator";

        public double DropIndicatorHeight
        {
            get
            {
                return (double)this.GetValue(ReorderListBoxItem.DropIndicatorHeightProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.DropIndicatorHeightProperty, (object)value);
            }
        }

        public bool IsReorderEnabled
        {
            get
            {
                return (bool)this.GetValue(ReorderListBoxItem.IsReorderEnabledProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.IsReorderEnabledProperty, (object)value);
            }
        }

        public DataTemplate DragHandleTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ReorderListBoxItem.DragHandleTemplateProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.DragHandleTemplateProperty, (object)value);
            }
        }

        public ContentPresenter DragHandle { get; private set; }

        public ReorderListBoxItem()
        {
            this.DefaultStyleKey = (object)typeof(ReorderListBoxItem);
        }

        protected void OnDropIndicatorHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            VisualStateGroup visualStateGroup = ReorderListBoxItem.GetVisualStateGroup((FrameworkElement)VisualTreeHelper.GetChild((DependencyObject)this, 0), "DropIndicatorStates");
            if (visualStateGroup == null)
                return;
            foreach (VisualState state in (IEnumerable)visualStateGroup.States)
            {
                foreach (Timeline child in (PresentationFrameworkCollection<Timeline>)state.Storyboard.Children)
                    this.UpdateDropIndicatorAnimationHeight((double)e.NewValue, child);
            }
            foreach (VisualTransition transition in (IEnumerable)visualStateGroup.Transitions)
            {
                foreach (Timeline child in (PresentationFrameworkCollection<Timeline>)transition.Storyboard.Children)
                    this.UpdateDropIndicatorAnimationHeight((double)e.NewValue, child);
            }
        }

        private void UpdateDropIndicatorAnimationHeight(double height, Timeline animation)
        {
            DoubleAnimation doubleAnimation = animation as DoubleAnimation;
            if (doubleAnimation == null)
                return;
            string targetName = Storyboard.GetTargetName((Timeline)doubleAnimation);
            PropertyPath targetProperty = Storyboard.GetTargetProperty((Timeline)doubleAnimation);
            if (!(targetName == "DropBeforeSpace") && !(targetName == "DropAfterSpace") || (targetProperty == null || !(targetProperty.Path == "Height")))
                return;
            double? nullable = doubleAnimation.From;
            double num1 = 0.0;
            if ((nullable.GetValueOrDefault() > num1 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
            {
                nullable = doubleAnimation.From;
                double num2 = height;
                if ((nullable.GetValueOrDefault() == num2 ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                    doubleAnimation.From = new double?(height);
            }
            nullable = doubleAnimation.To;
            double num3 = 0.0;
            if ((nullable.GetValueOrDefault() > num3 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                return;
            nullable = doubleAnimation.To;
            double num4 = height;
            if ((nullable.GetValueOrDefault() == num4 ? (!nullable.HasValue ? 1 : 0) : 1) == 0)
                return;
            doubleAnimation.To = new double?(height);
        }

        private static VisualStateGroup GetVisualStateGroup(FrameworkElement element, string groupName)
        {
            VisualStateGroup visualStateGroup1 = (VisualStateGroup)null;
            IList visualStateGroups = VisualStateManager.GetVisualStateGroups(element);
            if (visualStateGroups != null)
            {
                foreach (VisualStateGroup visualStateGroup2 in (IEnumerable)visualStateGroups)
                {
                    if (visualStateGroup2.Name == groupName)
                    {
                        visualStateGroup1 = visualStateGroup2;
                        break;
                    }
                }
            }
            return visualStateGroup1;
        }

        protected void OnIsReorderEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            string stateName = (bool)e.NewValue ? "ReorderEnabled" : "ReorderDisabled";
            if (!(this.DataContext is IMarker))
            {
                VisualStateManager.GoToState((Control)this, stateName, true);
            }
            else
            {
                this.IsReorderEnabled = false;
                VisualStateManager.GoToState((Control)this, "ReorderDisabled", true);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.DragHandle = this.GetTemplateChild("DragHandle") as ContentPresenter;
            if (this.DragHandle == null)
                throw new InvalidOperationException("ReorderListBoxItem must have a DragHandle ContentPresenter part.");
            VisualStateManager.GoToState((Control)this, "ReorderDisabled", false);
            VisualStateManager.GoToState((Control)this, "NotDragging", false);
            VisualStateManager.GoToState((Control)this, "NoDropIndicator", false);
        }
    }
}
