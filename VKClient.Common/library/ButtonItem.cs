using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class ButtonItem : VirtualizableItemBase
  {
    private string _text;

    public override double FixedHeight
    {
      get
      {
        return 48.0;
      }
    }

    public event EventHandler Tap;

    public ButtonItem(double width, Thickness margin, string text)
        : base(width, margin, new Thickness())
    {
      this._text = text;
    }

    protected override void GenerateChildren()
    {
      Button button = new Button();
      ((ContentControl) button).Content = this._text;
      ((FrameworkElement) button).Style=(Application.Current.Resources["VKButtonSecondaryStyle"] as Style);
      TiltEffect.SetSuppressTilt((DependencyObject) button, true);
      ((FrameworkElement) button).Width=(this.Width + 24.0);
      ((FrameworkElement) button).Margin=(new Thickness(-12.0));
      ((UIElement) button).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.button_Tap));
      this.Children.Add((FrameworkElement) button);
    }

    private void button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      // ISSUE: reference to a compiler-generated field
      if (this.Tap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.Tap(this, EventArgs.Empty);
    }
  }
}
