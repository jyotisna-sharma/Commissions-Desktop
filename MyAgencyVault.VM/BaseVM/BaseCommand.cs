using System;
using System.Windows.Input;

namespace MyAgencyVault.VM.BaseVM
{   
    public class BaseCommand : ICommand
    {
        #region ICommand Members
        // It is used for creating some action on any method 
        Action<object> _execute;
        // It is used for getting any action point and getting true and false 
        Predicate<object> _canExecute;
        // It is a constractor for getting the action and command.
        public BaseCommand(Predicate<object> CanExecute, Action<object> Execute)
        {
            if (Execute == null)
            {
                throw new NullReferenceException("Execute"); 
            }
            _execute = Execute;
            _canExecute = CanExecute;
        }
        // It is a constractor for getting the Action and call the previous constractor
        public BaseCommand(Action<object> Execute) : this(null,Execute) 
        {

        }
        // Some action occurs on a perticular code and check is it valid for an action
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);  
        }
        // It is checking the condition might be changed or not
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; } 
            remove { CommandManager.RequerySuggested -= value; } 
        }
        // Create some action for running your code
        public void Execute(object parameter)
        {
            _execute(parameter);  
        }

        #endregion
    }
}
