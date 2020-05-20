using System;
using System.Windows.Input;

namespace VKClient.Common.Framework
{
  public class DelegateCommand : ICommand
  {
    private Func<object, bool> canExecute;
    private Action<object> executeAction;

    public event EventHandler CanExecuteChanged;

    public DelegateCommand(Action<object> executeAction)
      : this(executeAction,  null)
    {
    }

    public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
    {
      if (executeAction == null)
        throw new ArgumentNullException("executeAction");
      this.executeAction = executeAction;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      bool flag = true;
      Func<object, bool> canExecute = this.canExecute;
      if (canExecute != null)
        flag = canExecute(parameter);
      return flag;
    }

    public void RaiseCanExecuteChanged()
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler canExecuteChanged = this.CanExecuteChanged;
      if (canExecuteChanged == null)
        return;
      canExecuteChanged(this, new EventArgs());
    }

    public void Execute(object parameter)
    {
      this.executeAction(parameter);
    }
  }
}
