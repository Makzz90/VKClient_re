using System;
using System.Windows.Controls;

namespace VKMessenger.Framework
{
  public class UserSearchDataTemplateSelector : ContentControl
  {
    public event EventHandler<ContentEventArgs> ContentChanged;

    public UserSearchDataTemplateSelector()
    {
      //base.\u002Ector();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      // ISSUE: reference to a compiler-generated field
      if (this.ContentChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ContentChanged(this, new ContentEventArgs(newContent));
    }
  }
}
