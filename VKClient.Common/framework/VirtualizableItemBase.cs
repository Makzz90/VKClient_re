using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VKClient.Common.Framework
{
  public abstract class VirtualizableItemBase : IVirtualizable, ISupportImpressionTracking
  {
    protected readonly Canvas _view = new Canvas();
    private static int _instanceCount;
    private bool _shownTop;
    private bool _shownBottom;
    private double _width;
    protected Thickness _margin;
    private Thickness _viewMargin;
    private static int _loadedCounter;

    protected List<FrameworkElement> Children = new List<FrameworkElement>();

    public ObservableCollection<IVirtualizable> VirtualizableChildren = new ObservableCollection<IVirtualizable>();

    public VirtualizableState CurrentState { get; private set; }

    public IMyVirtualizingPanel Parent { get; set; }

    public VirtualizableItemBase ParentItem { get; set; }

    public double Width
    {
      get
      {
        return this._width;
      }
      set
      {
        this._width = value;
        ((FrameworkElement) this._view).Width = value;
      }
    }

    public FrameworkElement View
    {
      get
      {
        return (FrameworkElement) this._view;
      }
    }

    public Thickness Margin
    {
      get
      {
        return this._margin;
      }
      set
      {
        this._margin = value;
        ((FrameworkElement) this._view).Margin = value;
      }
    }

    public Thickness ViewMargin
    {
      get
      {
        return this._viewMargin;
      }
      set
      {
        this._viewMargin = value;
        Canvas.SetLeft((UIElement) this._view, ((Thickness) this._viewMargin).Left);
        Canvas.SetTop((UIElement) this._view, ((Thickness) this._viewMargin).Top);
      }
    }

    public abstract double FixedHeight { get; }

    protected VirtualizableItemBase(double width, Thickness margin, Thickness viewMargin = new Thickness())
    {
      this._width = width;
      this.Margin = margin;
      this.ViewMargin = viewMargin;
      ((FrameworkElement) this._view).Width = this.Width;
      VirtualizableItemBase.UpdateInstanceCount(true);
      this.VirtualizableChildren.CollectionChanged += new NotifyCollectionChangedEventHandler(this.VirtualizableChildren_OnCollectionChanged);
    }

    protected VirtualizableItemBase(double width)
    {
      this._width = width;
      VirtualizableItemBase.UpdateInstanceCount(true);
    }

    ~VirtualizableItemBase()
    {
      VirtualizableItemBase.UpdateInstanceCount(false);
    }

    private static void UpdateInstanceCount(bool increase)
    {
      if (increase)
        ++VirtualizableItemBase._instanceCount;
      else
        --VirtualizableItemBase._instanceCount;
    }

    private void VirtualizableChildren_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems == null || e.NewItems.Count <= 0)
        return;
      VirtualizableItemBase newItem = e.NewItems[0] as VirtualizableItemBase;
      if (newItem == null)
        return;
      newItem.ParentItem = this;
    }

    protected void NotifyHeightChanged()
    {
      ISupportChildHeightChange parentItem = this.ParentItem as ISupportChildHeightChange;
      if (parentItem != null)
      {
        parentItem.RespondToChildHeightChange((IVirtualizable) this);
      }
      else
      {
        if (this.Parent == null)
          throw new Exception();
        this.Parent.RearrangeAllItems();
      }
    }

    public void ChangeState(VirtualizableState newState)
    {
      if (newState == this.CurrentState)
        return;
      if (newState == VirtualizableState.LoadedFully || newState == VirtualizableState.LoadedPartially)
        this.Load(newState);
      else
        this.Unload();
    }

    protected void RegenerateChildren()
    {
      if (this.CurrentState == VirtualizableState.Unloaded)
        return;
      using (List<FrameworkElement>.Enumerator enumerator = this.Children.GetEnumerator())
      {
        while (enumerator.MoveNext())
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Remove((UIElement) enumerator.Current);
      }
      this.Children.Clear();
      this.GenerateChildren();
      int num = 0;
      using (List<FrameworkElement>.Enumerator enumerator = this.Children.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          FrameworkElement current = enumerator.Current;
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Insert(num++, (UIElement) current);
        }
      }
      ((FrameworkElement) this._view).Height = this.FixedHeight;
    }

    protected void Unload()
    {
      if (this.CurrentState == VirtualizableState.Unloaded)
        return;
      --VirtualizableItemBase._loadedCounter;
      int num = VirtualizableItemBase._loadedCounter % 10;
      this.ReleaseResourcesOnUnload();
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Clear();
      this.Children.Clear();
      foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
        virtualizableChild.ChangeState(VirtualizableState.Unloaded);
      this.CurrentState = VirtualizableState.Unloaded;
    }

    protected virtual void ReleaseResourcesOnUnload()
    {
      this._shownBottom = false;
      this._shownTop = false;
      this.ResetMenu();
    }

    protected void Load(VirtualizableState newState)
    {
      ((FrameworkElement) this._view).Height = this.FixedHeight;
      if (this.CurrentState == VirtualizableState.Unloaded)
      {
        ++VirtualizableItemBase._loadedCounter;
        int num = VirtualizableItemBase._loadedCounter % 10;
        this.GenerateChildren();
        using (List<FrameworkElement>.Enumerator enumerator = this.Children.GetEnumerator())
        {
          while (enumerator.MoveNext())
            ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) enumerator.Current);
        }
        foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
        {
          virtualizableChild.ChangeState(newState);
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) virtualizableChild.View);
        }
        this.CurrentState = VirtualizableState.LoadedPartially;
      }
      if (newState != VirtualizableState.LoadedFully)
        return;
      this.LoadFullyNonVirtualizableItems();
      foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
      {
        virtualizableChild.ChangeState(VirtualizableState.LoadedFully);
        this.CurrentState = VirtualizableState.LoadedFully;
      }
    }

    protected virtual void LoadFullyNonVirtualizableItems()
    {
    }

    protected virtual void GenerateChildren()
    {
    }

    protected virtual void ShownOnScreen()
    {
    }

    protected void CreateImageWithPlaceholderAddToChildren(Canvas parent, Thickness margin, double width, double height, string uri, object tag = null)
    {
      Rectangle imagePlaceholder = RectangePlaceholder.CreateImagePlaceholder(width, height);
      ((FrameworkElement) imagePlaceholder).Margin = margin;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) parent).Children).Add((UIElement) imagePlaceholder);
      this.CreateImageAddToChildren(parent, margin, width, height, uri, tag);
    }

    protected void CreateImageAddToChildren(Canvas parent, Thickness margin, double width, double height, string uri, object tag = null)
    {
      Image addToChildren = this.CreateAddToChildren<Image>(parent, margin, width, height, tag);
      addToChildren.Stretch=((Stretch) 3);
      if (string.IsNullOrEmpty(uri))
        return;
      ImageLoader.SetSourceForImage(addToChildren, uri, false);
    }

    protected void CreateTextBlockAddToChildren(Canvas parent, Thickness margin, double width, double height, string text, VirtualizableItemBase.TextBlockParam parameters, object tag = null)
    {
      TextBlock addToChildren = this.CreateAddToChildren<TextBlock>(parent, margin, width, height, tag);
      addToChildren.Text = text;
      if (parameters == null)
        return;
      if (parameters.Style != null)
        ((FrameworkElement) addToChildren).Style=(Application.Current.Resources[parameters.Style] as Style);
      if (parameters.FontFamily != null)
        addToChildren.FontFamily=(new FontFamily(parameters.FontFamily));
      if (!double.IsNaN(parameters.FontSize))
        addToChildren.FontSize = parameters.FontSize;
      if (parameters.Foreground != null)
        addToChildren.Foreground = ((Brush) parameters.Foreground);
      addToChildren.TextWrapping=(parameters.Wrap ? (TextWrapping) 2 : (TextWrapping) 1);
    }

    protected T CreateAddToChildren<T>(Canvas parent, Thickness margin, double width, double height, object tag = null) where T : FrameworkElement, new()
    {
      T instance = Activator.CreateInstance<T>();
      ((FrameworkElement) instance).Margin = margin;
      ((FrameworkElement) instance).Width = width;
      ((FrameworkElement) instance).Height = height;
      ((FrameworkElement) instance).Tag = tag;
      T obj = instance;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) parent).Children).Add((UIElement) obj);
      return obj;
    }

    public void UpdateLayout()
    {
      if (this.CurrentState == VirtualizableState.Unloaded)
        return;
      VirtualizableState currentState = this.CurrentState;
      this.ChangeState(VirtualizableState.Unloaded);
      this.ChangeState(currentState);
    }

    public void IsOnScreen()
    {
      foreach (IVirtualizable virtualizableChild in (Collection<IVirtualizable>) this.VirtualizableChildren)
        virtualizableChild.IsOnScreen();
      this.ShownOnScreen();
    }

    public void SetMenu(List<MenuItem> menuItems)
    {
      if (menuItems.Count <= 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      ((Control) contextMenu1).Background = ((Brush) solidColorBrush1);
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      ((Control) contextMenu1).Foreground = ((Brush) solidColorBrush2);
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in menuItems)
        ((PresentationFrameworkCollection<object>) contextMenu2.Items).Add(menuItem);
      ContextMenuService.SetContextMenu((DependencyObject) this._view, contextMenu2);
    }

    public void ResetMenu()
    {
      ContextMenuService.SetContextMenu((DependencyObject) this._view,  null);
    }

    public void TopIsOnScreen()
    {
      if (this._shownTop && this._shownBottom)
        return;
      this._shownTop = true;
      if (!this._shownTop || !this._shownBottom)
        return;
      this.NotifyAboutImpression();
    }

    public void BottomIsOnScreen()
    {
      if (this._shownTop && this._shownBottom)
        return;
      this._shownBottom = true;
      if (!this._shownTop || !this._shownBottom)
        return;
      this.NotifyAboutImpression();
    }

    protected virtual void NotifyAboutImpression()
    {
    }

    public class TextBlockParam
    {
      public string FontFamily { get; set; }

      public double FontSize { get; set; }

      public SolidColorBrush Foreground { get; set; }

      public bool Wrap { get; set; }

      public string Style { get; set; }

      public TextBlockParam()
      {
        this.FontSize = double.NaN;
      }
    }
  }
}
