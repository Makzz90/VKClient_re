using System.Windows.Controls;
using System.Windows.Interactivity;

namespace VKClient.Common.Behaviors
{
  public class TextBoxCursotToEndOnTextChangedBehavior : Behavior<TextBox>
  {
    protected override void OnAttached()
    {
      base.OnAttached();
      this.AssociatedObject.TextChanged += new TextChangedEventHandler(this.AssociatedObject_OnTextChanged);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      this.AssociatedObject.TextChanged -= new TextChangedEventHandler(this.AssociatedObject_OnTextChanged);
    }

    private void AssociatedObject_OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
    {
      this.AssociatedObject.Select(this.AssociatedObject.Text.Length, 0);
    }
  }
}
