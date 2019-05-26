using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NFCLogin
{
    /**
     * <summary>
     * Stores an Action so that it can be executed later. Use this class for XAML Command bindings and passing functions around. Implements ICommand interface.
     * </summary>
     */ 
    public class RelayCommand : ICommand
    {
        public delegate void ExecuteMethod();
        private Action _action;

        /**
         * <summary>
         * Constructs a RelayCommand from an Action. Actions are created from parameterless methods with no returns.
         * </summary>
         */ 
        public RelayCommand(Action action)
        {
            _action = action;
        }

        #region ICommand Members

        /**
         * <summary>
         * Determines whether the command can execute in its current state. Always true for RelayCommand.
         * </summary>
         */ 
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        /**
         * <summary>
         * Executes the Action used to create this RelayCommand.
         * </summary>
         */
        public void Execute(object parameter = null)
        {
            _action();
        }

        #endregion
    }
}
