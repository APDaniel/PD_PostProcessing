using System;
using System.Windows.Input;

public class RelayCommandSender<T> : ICommand
{
    private readonly Action<T> execute;
    private readonly Func<T, bool> canExecute;

    public RelayCommandSender(Action<T> execute)
        : this(execute, null)
    {
    }

    public RelayCommandSender(Action<T> execute, Func<T, bool> canExecute)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter)
    {
        if (canExecute == null)
        {
            return true;
        }

        if (parameter is T typedParameter)
        {
            return canExecute(typedParameter);
        }

        return false;
    }

    public void Execute(object parameter)
    {
        if (parameter is T typedParameter)
        {
            execute(typedParameter);
        }
    }
}
