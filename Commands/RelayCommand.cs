using System;
using System.Windows.Input;

namespace PD_ScriptTemplate.Commands
{
    /// <summary>
    /// This class represents a command relay to be used to initialize a method though a command layer
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private Members
        /// <summary>
        /// Action to run
        /// </summary>
        private Action mAction;
        
        #endregion

        #region Public Events
        /// <summary>
        /// The event that fired when the CanExecute value has changed
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public RelayCommand(Action action)
        {
            mAction = action;
        }
        #endregion

        #region Commands Methods
        /// <summary>
        /// A relay command can always execute
        /// </summary>
        public bool CanExecute(object parameter) { return true; }
        /// <summary>
        /// Executes the commands Action
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            mAction();
        }
        #endregion
    }
}
