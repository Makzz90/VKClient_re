using System;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Audio.Base.DataObjects
{
  public abstract class ConversationsUCBase : UserControl
  {
    public bool IsShareContentMode { get; set; }

    public event EventHandler<Action> ConversationTap;

    protected ConversationsUCBase()
    {
      //base.\u002Ector();
    }

    public abstract void SetListHeader(FrameworkElement element);

    public abstract void PrepareForViewIfNeeded();

    protected void OnConversationTap(Action callback)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.ConversationTap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ConversationTap(this, callback);
    }
  }
}
