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
      button.Content = (object) this._text;
      button.Style = Application.Current.Resources["VKButtonSecondaryStyle"] as Style;
      TiltEffect.SetSuppressTilt((DependencyObject) button, true);
      button.Width = this.Width + 24.0;
      button.Margin = new Thickness(-12.0);
      button.Tap += new EventHandler<GestureEventArgs>(this.button_Tap);
      this.Children.Add((FrameworkElement) button);
    }

    private void button_Tap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      if (this.Tap == null)
        return;
      this.Tap((object) this, EventArgs.Empty);
    }
  }
}
