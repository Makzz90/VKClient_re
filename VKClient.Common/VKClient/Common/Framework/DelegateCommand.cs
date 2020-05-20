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
            : this(executeAction, (Func<object, bool>)null)
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
            Func<object, bool> func = this.canExecute;
            if (func != null)
                flag = func(parameter);
            return flag;
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler eventHandler = this.CanExecuteChanged;
            if (eventHandler == null)
                return;
            eventHandler((object)this, new EventArgs());
        }

        public void Execute(object parameter)
        {
            this.executeAction(parameter);
        }
    }
}
