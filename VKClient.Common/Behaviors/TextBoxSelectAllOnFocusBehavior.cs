using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace VKClient.Common.Behaviors
{
  public class TextBoxSelectAllOnFocusBehavior : Behavior<TextBox>
  {
    protected override void OnAttached()
    {
      base.OnAttached();
      // ISSUE: method pointer
      ((UIElement) this.AssociatedObject).GotFocus += (new RoutedEventHandler( this.AssociatedObject_OnGotFocus));
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      // ISSUE: method pointer
      ((UIElement) this.AssociatedObject).GotFocus-=(new RoutedEventHandler( this.AssociatedObject_OnGotFocus));
    }

    private void AssociatedObject_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.AssociatedObject.SelectAll();
    }
  }
}
