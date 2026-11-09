namespace Behavioral.Command;

/// <summary>
/// Invoker — executes commands and maintains a history stack for undo support.
/// </summary>
public class CartCommandInvoker
{
    private readonly Stack<ICommand> _history = new();

    public void Execute(ICommand command)
    {
        command.Execute();
        _history.Push(command);
    }

    public void UndoLast()
    {
        if (_history.TryPop(out var last))
        {
            Console.WriteLine("[Invoker] Undoing last command...");
            last.Undo();
        }
    }
}
