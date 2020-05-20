using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKMessenger.Library.Events;

namespace VKMessenger.Library.VirtItems
{
  public class TapHandlerItem : VirtualizableItemBase
  {
    private Grid _grid;
    private MessageViewModel _mvm;

    public override double FixedHeight
    {
      get
      {
        return 10.0;
      }
    }

    public TapHandlerItem(double width, double height, MessageViewModel mvm)
      : base(width)
    {
      this._mvm = mvm;
      this._grid = new Grid();
      this.SetWidthHeight(width, height);
      ((Panel) this._grid).Background = ((Brush) new SolidColorBrush(Colors.Transparent));
    }

    public void SetWidthHeight(double width, double height)
    {
      ((FrameworkElement) this._grid).Width = width;
      ((FrameworkElement) this._grid).Height = height;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      ((UIElement) this._grid).IsHitTestVisible = this._mvm.IsInSelectionMode;
      ((UIElement) this._grid).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this._grid_Tap));
      this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
      this.Children.Add((FrameworkElement) this._grid);
    }

    private void _grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      EventAggregator.Current.Publish(new MessageActionEvent()
      {
        Message = this._mvm,
        MessageActionType = MessageActionType.SelectUnselect
      });
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      ((UIElement) this._grid).Tap -= (new EventHandler<System.Windows.Input.GestureEventArgs>(this._grid_Tap));
      this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
    }

    private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsInSelectionMode"))
        return;
      ((UIElement) this._grid).IsHitTestVisible = this._mvm.IsInSelectionMode;
    }
  }
}
