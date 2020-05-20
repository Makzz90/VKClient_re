using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace VKClient.Common.Behaviors
{
  public class TextBoxTextChangedUpdateBehavior : Behavior<TextBox>
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
      BindingExpression bindingExpression = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);
      if (bindingExpression == null)
        return;
      bindingExpression.UpdateSource();
    }
  }
}
